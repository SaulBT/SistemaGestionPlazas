using System.Globalization;
using System.Text.RegularExpressions;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;

namespace SGPla.Parsers
{
    public static partial class PlaneaNormalizador
    {
        public static DatosPeriodoPlanea Normalizar(PlaneaRespuesta respuesta)
        {
            var advertencias = new AcumuladorAdvertencias();
            var nrcsVistos = new HashSet<string>(StringComparer.Ordinal);
            var copias = new Dictionary<string, CopiaPlanea>(StringComparer.Ordinal);
            var docentes = new Dictionary<(string Nrc, string? NumeroPersonal, string Nombre), DocenteCopiaPlanea>();
            var nrcSinPlan = 0;
            foreach (var fila in respuesta.Resultado ?? [])
            {
                var nrc = Limpiar(fila.Nrc);
                var materia = Limpiar(fila.Materia)?.ToUpperInvariant();
                var curso = Limpiar(fila.Curso)?.ToUpperInvariant();
                var titulo = NormalizarEspacios(fila.Titulo);
                if (nrc is null || nrc.Length > PlaneaConstantes.LONGITUD_NRC || materia is null || curso is null || titulo is null)
                {
                    advertencias.Agregar("Filas sin NRC, materia, curso o título válidos", nrc);
                    continue;
                }
                // PLANEA repite el NRC en una fila por docente: los docentes se toman de todas las filas.
                if (NormalizarEspacios(fila.NombreDocente) is { } nombreDocente)
                {
                    var numeroPersonal = Truncar(Limpiar(fila.NumeroPersonalDocente)?.ToUpperInvariant(), 15);
                    nombreDocente = Truncar(nombreDocente, 150)!;
                    docentes.TryAdd((nrc, numeroPersonal, nombreDocente),
                        new DocenteCopiaPlanea(nrc, numeroPersonal, nombreDocente, ParsearIndicador(fila.Imparte)));
                }
                if (!nrcsVistos.Add(nrc)) continue;
                var codigoPlan = Limpiar(fila.CodigoPlan)?.ToUpperInvariant();
                if (codigoPlan is null) { nrcSinPlan++; continue; }
                var codigoExperiencia = $"{materia} {curso}";
                if (codigoExperiencia.Length > PlaneaConstantes.LONGITUD_CODIGO_EE || codigoPlan.Length > PlaneaConstantes.LONGITUD_CODIGO_PLAN)
                {
                    advertencias.Agregar("Códigos de EE o de plan con longitud inválida", nrc);
                    continue;
                }
                copias[nrc] = new CopiaPlanea(nrc, codigoExperiencia, codigoPlan,
                    Truncar(titulo, PlaneaConstantes.LONGITUD_TITULO)!, Truncar(Limpiar(fila.Campus), 5),
                    Truncar(Limpiar(fila.Nivel), 5), Truncar(Limpiar(fila.Region), 50), Truncar(Limpiar(fila.Area), 100));
            }

            var horarios = new Dictionary<(string, string, TimeOnly, TimeOnly, string?, string?, DateOnly?, DateOnly?), HorarioCopiaPlanea>();
            foreach (var bloque in respuesta.Horarios ?? [])
            {
                var nrc = Limpiar(bloque.Nrc);
                if (nrc is null || !nrcsVistos.Contains(nrc)) { advertencias.Agregar("Horarios de NRC que no vienen en resultado", nrc); continue; }
                if (!copias.ContainsKey(nrc)) continue;
                if (!int.TryParse(bloque.IdHorario, NumberStyles.None, CultureInfo.InvariantCulture, out var idHorario))
                { advertencias.Agregar("Horarios con rhs_id inválido", nrc); continue; }
                var edificio = Truncar(Limpiar(bloque.Edificio), 50);
                var aula = Truncar(Limpiar(bloque.Aula), 100);
                var fechaInicio = ParsearFecha(bloque.FechaInicio);
                var fechaFin = ParsearFecha(bloque.FechaFin);
                var tieneDias = false;
                foreach (var (dia, inicio, fin) in DiasDe(bloque))
                {
                    if (Limpiar(inicio) is null && Limpiar(fin) is null) continue;
                    tieneDias = true;
                    if (!TryParsearHora(inicio, out var horaInicio) || !TryParsearHora(fin, out var horaFin) || horaFin <= horaInicio)
                    { advertencias.Agregar("Horas de horario inválidas", $"{nrc}/{dia}"); continue; }
                    var clave = (nrc, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin);
                    horarios.TryAdd(clave, new HorarioCopiaPlanea(idHorario, nrc, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin));
                }
                if (!tieneDias) advertencias.Agregar("Bloques de horario sin días", nrc);
            }
            var docentesCopias = docentes.Values.Where(d => copias.ContainsKey(d.Nrc)).ToList();
            return new DatosPeriodoPlanea(nrcsVistos.Count, nrcSinPlan, copias.Values.ToList(), horarios.Values.ToList(), docentesCopias, advertencias.Resumir());
        }

        public static string? Limpiar(string? valor)
        {
            if (valor is null) return null;
            var limpio = valor.Trim();
            return limpio.Length == 0 || limpio is "-" or "---" ? null : limpio;
        }
        public static string? NormalizarEspacios(string? valor)
        {
            var limpio = Limpiar(valor);
            return limpio is null ? null : EspaciosMultiples().Replace(limpio, " ");
        }
        public static bool TryParsearHora(string? valor, out TimeOnly hora)
        {
            hora = default;
            if (valor is null || valor.Length != 4 || !valor.All(char.IsAsciiDigit)) return false;
            if (!int.TryParse(valor.AsSpan(0, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var h)
                || !int.TryParse(valor.AsSpan(2, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var m) || h > 23 || m > 59) return false;
            hora = new TimeOnly(h, m); return true;
        }
        public static bool? ParsearIndicador(string? valor) => Limpiar(valor)?.ToUpperInvariant() switch
        {
            "SI" or "SÍ" or "S" => true,
            "NO" or "N" => false,
            _ => null
        };
        public static DateOnly? ParsearFecha(string? valor) => DateOnly.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) ? fecha : null;
        public static string? Truncar(string? valor, int longitud) => valor is null || valor.Length <= longitud ? valor : valor[..longitud];

        private static IEnumerable<(string Dia, string? Inicio, string? Fin)> DiasDe(PlaneaHorario h) =>
        [ ("Lunes", h.LunIni, h.LunFin), ("Martes", h.MarIni, h.MarFin), ("Miercoles", h.MieIni, h.MieFin), ("Jueves", h.JueIni, h.JueFin), ("Viernes", h.VieIni, h.VieFin), ("Sabado", h.SabIni, h.SabFin) ];
        [GeneratedRegex("\\s+")]
        private static partial Regex EspaciosMultiples();

        private sealed class AcumuladorAdvertencias
        {
            private readonly Dictionary<string, (int Count, List<string> Examples)> _items = new(StringComparer.Ordinal);
            public void Agregar(string categoria, string? ejemplo)
            {
                if (!_items.TryGetValue(categoria, out var item)) item = (0, []);
                item.Count++;
                if (ejemplo is not null && item.Examples.Count < 5) item.Examples.Add(ejemplo);
                _items[categoria] = item;
            }
            public IReadOnlyList<string> Resumir() => _items.OrderByDescending(x => x.Value.Count)
                .Select(x => $"{x.Value.Count} × {x.Key}" + (x.Value.Examples.Count == 0 ? "" : $" (ej. {string.Join(", ", x.Value.Examples)})")).ToList();
        }
    }
}
