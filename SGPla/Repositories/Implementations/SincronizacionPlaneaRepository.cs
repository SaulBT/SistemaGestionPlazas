using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SGPla.Commons;
using SGPla.Data;
using SGPla.Models;
using SGPla.Models.DTOs.Planea;
using SGPla.Repositories.Interfaces;

namespace SGPla.Repositories.Implementations
{
    public sealed class SincronizacionPlaneaRepository(GestionDePlazasDbContext context) : ISincronizacionPlaneaRepository
    {
        private readonly GestionDePlazasDbContext _context = context;

        public async Task<IReadOnlyList<PeriodoPorSincronizar>> ObtenerPeriodosVigentesAsync(DateOnly hoy, int ventanaAnticipacionMeses, CancellationToken cancellationToken = default)
        {
            var limite = hoy.AddMonths(ventanaAnticipacionMeses);
            // Son pocos periodos: se evalúan en memoria para usar las fechas derivadas del código
            // cuando el periodo no tiene fechas registradas.
            var periodos = await _context.Periodo.AsNoTracking()
                .Select(p => new { p.IdPeriodo, p.Codigo, p.FechaInicio, p.FechaFin })
                .ToListAsync(cancellationToken);
            return periodos
                .Select(p => new
                {
                    p.IdPeriodo,
                    Codigo = p.Codigo.Trim(),
                    Fechas = p.FechaInicio is { } inicio && p.FechaFin is { } fin
                        ? (inicio, fin)
                        : PlaneaPeriodos.FechasPorCodigo(p.Codigo)
                })
                .Where(p => p.Fechas is { } f && f.Item2 >= hoy && f.Item1 <= limite)
                .OrderBy(p => p.Codigo, StringComparer.Ordinal)
                .Select(p => new PeriodoPorSincronizar(p.IdPeriodo, p.Codigo))
                .ToList();
        }

        public Task<int> ContarPeriodosSinFechasAsync(CancellationToken cancellationToken = default) =>
            _context.Periodo.CountAsync(p => p.FechaInicio == null || p.FechaFin == null, cancellationToken);

        public async Task<int> IniciarBitacoraAsync(int idPeriodo, CancellationToken cancellationToken = default)
        {
            var bitacora = new SincronizacionPlanea { IdPeriodo = idPeriodo, FechaInicio = DateTime.Now, Estado = PlaneaConstantes.ESTADO_EN_PROCESO };
            _context.SincronizacionPlanea.Add(bitacora);
            await _context.SaveChangesAsync(cancellationToken);
            var id = bitacora.IdSincronizacionPlanea;
            _context.ChangeTracker.Clear();
            return id;
        }

        public async Task CerrarBitacoraAsync(int idSincronizacion, string estado, int? registrosRecibidos, DatosPeriodoPlanea? datos,
            ResumenAplicacionPlanea? resumen, string? advertencias, string? mensajeError, CancellationToken cancellationToken = default)
        {
            await _context.SincronizacionPlanea.Where(s => s.IdSincronizacionPlanea == idSincronizacion)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.FechaFin, DateTime.Now).SetProperty(s => s.Estado, estado)
                    .SetProperty(s => s.RegistrosRecibidos, registrosRecibidos)
                    .SetProperty(s => s.NrcRecibidos, datos == null ? null : datos.NrcRecibidos)
                    .SetProperty(s => s.NrcSinPlan, datos == null ? null : datos.NrcSinPlan)
                    .SetProperty(s => s.NrcNuevos, resumen == null ? null : resumen.NrcNuevos)
                    .SetProperty(s => s.NrcExistentes, resumen == null ? null : resumen.NrcExistentes)
                    .SetProperty(s => s.NrcSinExperiencia, resumen == null ? null : resumen.NrcSinExperiencia)
                    .SetProperty(s => s.HorariosInsertados, resumen == null ? null : resumen.HorariosInsertados)
                    .SetProperty(s => s.Advertencias, advertencias).SetProperty(s => s.MensajeError, mensajeError), cancellationToken);
        }

        public Task<int> MarcarInterrumpidasAsync(TimeSpan umbral, CancellationToken cancellationToken = default)
        {
            var limite = DateTime.Now - umbral;
            return _context.SincronizacionPlanea.Where(s => s.Estado == PlaneaConstantes.ESTADO_EN_PROCESO && s.FechaInicio < limite)
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.Estado, PlaneaConstantes.ESTADO_INTERRUMPIDA)
                    .SetProperty(s => s.FechaFin, DateTime.Now)
                    .SetProperty(s => s.MensajeError, "La ejecución no terminó (reinicio o caída de la aplicación)."), cancellationToken);
        }

        public async Task<ResumenAplicacionPlanea> RegistrarNuevasAsync(int idPeriodo, string codigoPeriodo, int idSincronizacion,
            DatosPeriodoPlanea datos, CancellationToken cancellationToken = default)
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                await using var transaccionEf = await _context.Database.BeginTransactionAsync(cancellationToken);
                var conexion = (SqlConnection)_context.Database.GetDbConnection();
                var transaccion = (SqlTransaction)transaccionEf.GetDbTransaction();
                await AdquirirBloqueoAsync(conexion, transaccion, codigoPeriodo, cancellationToken);
                await EjecutarAsync(conexion, transaccion, SincronizacionPlaneaSql.CrearTablasTemporales, cancellationToken);
                await CopiarAsync(conexion, transaccion, "#CopiaPlanea", CrearTablaCopias(datos.Copias), cancellationToken);
                await CopiarAsync(conexion, transaccion, "#HorarioPlanea", CrearTablaHorarios(datos.Horarios), cancellationToken);
                await CopiarAsync(conexion, transaccion, "#DocentePlanea", CrearTablaDocentes(datos.Docentes), cancellationToken);
                await EjecutarAsync(conexion, transaccion, SincronizacionPlaneaSql.ResolverReferencias, cancellationToken);
                var resumen = await LeerResumenAsync(conexion, transaccion, idPeriodo, idSincronizacion, cancellationToken);
                await transaccionEf.CommitAsync(cancellationToken);
                return resumen;
            }
            finally { await _context.Database.CloseConnectionAsync(); }
        }

        private static async Task AdquirirBloqueoAsync(SqlConnection conexion, SqlTransaction transaccion, string codigoPeriodo, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("DECLARE @r int; EXEC @r = sp_getapplock @Resource = @recurso, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 0; SELECT @r;", conexion, transaccion) { CommandTimeout = 300 };
            cmd.Parameters.Add("@recurso", SqlDbType.NVarChar, 255).Value = "SincronizacionPlanea:" + codigoPeriodo;
            if (Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) < 0) throw new SincronizacionEnCursoException(codigoPeriodo);
        }
        private static async Task EjecutarAsync(SqlConnection conexion, SqlTransaction transaccion, string sql, CancellationToken ct)
        { await using var cmd = new SqlCommand(sql, conexion, transaccion) { CommandTimeout = 300 }; await cmd.ExecuteNonQueryAsync(ct); }
        private static async Task CopiarAsync(SqlConnection conexion, SqlTransaction transaccion, string tabla, DataTable datos, CancellationToken ct)
        {
            if (datos.Rows.Count == 0) return;
            using var bulk = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, transaccion) { DestinationTableName = tabla, BatchSize = 5000, BulkCopyTimeout = 300 };
            foreach (DataColumn col in datos.Columns) bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            await bulk.WriteToServerAsync(datos, ct);
        }
        private static async Task<ResumenAplicacionPlanea> LeerResumenAsync(SqlConnection conexion, SqlTransaction transaccion, int idPeriodo, int idSincronizacion, CancellationToken ct)
        {
            await using var cmd = new SqlCommand(SincronizacionPlaneaSql.RegistrarNuevas, conexion, transaccion) { CommandTimeout = 300 };
            cmd.Parameters.Add("@idPeriodo", SqlDbType.Int).Value = idPeriodo;
            cmd.Parameters.Add("@idSincronizacion", SqlDbType.Int).Value = idSincronizacion;
            cmd.Parameters.Add("@idPlanEstudios", SqlDbType.Int).Value = DBNull.Value;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("SQL no devolvió el resumen de sincronización.");
            return new ResumenAplicacionPlanea(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
        }
        private static DataTable CrearTablaCopias(IReadOnlyList<CopiaPlanea> copias)
        {
            var t = new DataTable();
            t.Columns.Add("nrc", typeof(string)); t.Columns.Add("codigoExperiencia", typeof(string)); t.Columns.Add("codigoPlan", typeof(string)); t.Columns.Add("titulo", typeof(string));
            t.Columns.Add("campus", typeof(string)); t.Columns.Add("nivel", typeof(string)); t.Columns.Add("region", typeof(string)); t.Columns.Add("area", typeof(string));
            foreach (var c in copias) t.Rows.Add(c.Nrc, c.CodigoExperiencia, c.CodigoPlan, c.Titulo, Db(c.Campus), Db(c.Nivel), Db(c.Region), Db(c.Area));
            return t;
        }
        private static DataTable CrearTablaHorarios(IReadOnlyList<HorarioCopiaPlanea> horarios)
        {
            var t = new DataTable(); t.Columns.Add("idHorario", typeof(int)); t.Columns.Add("nrc", typeof(string)); t.Columns.Add("dia", typeof(string));
            t.Columns.Add("horaInicio", typeof(TimeSpan)); t.Columns.Add("horaFin", typeof(TimeSpan)); t.Columns.Add("edificio", typeof(string)); t.Columns.Add("aula", typeof(string));
            t.Columns.Add("fechaInicio", typeof(DateTime)); t.Columns.Add("fechaFin", typeof(DateTime));
            foreach (var h in horarios) t.Rows.Add(h.IdHorario, h.Nrc, h.Dia, h.HoraInicio.ToTimeSpan(), h.HoraFin.ToTimeSpan(), Db(h.Edificio), Db(h.Aula), Db(h.FechaInicio?.ToDateTime(TimeOnly.MinValue)), Db(h.FechaFin?.ToDateTime(TimeOnly.MinValue)));
            return t;
        }
        private static DataTable CrearTablaDocentes(IReadOnlyList<DocenteCopiaPlanea> docentes)
        {
            var t = new DataTable(); t.Columns.Add("nrc", typeof(string)); t.Columns.Add("numeroPersonal", typeof(string));
            t.Columns.Add("nombre", typeof(string)); t.Columns.Add("imparte", typeof(bool));
            foreach (var d in docentes) t.Rows.Add(d.Nrc, Db(d.NumeroPersonal), d.Nombre, Db(d.Imparte));
            return t;
        }
        private static object Db(object? value) => value ?? DBNull.Value;
    }
}
