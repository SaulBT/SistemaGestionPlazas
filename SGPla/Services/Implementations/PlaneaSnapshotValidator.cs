using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using SGPla.Models.DTOs.Integracion;
using SGPla.Services.Interfaces;

namespace SGPla.Services.Implementations;

public sealed class PlaneaSnapshotValidator : IPlaneaSnapshotValidator
{
    private static readonly Regex NrcValido = new("^[A-Z0-9._-]{1,20}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex NumeroPersonalValido = new("^[A-Z0-9._-]{1,50}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public PlaneaSnapshotValidado Validar(string clavePeriodo, DateOnly inicioPeriodo, DateOnly finPeriodo,
        IReadOnlyDictionary<string, int> programacionesPorNrc, IReadOnlyList<PlaneaRegistro> registros)
    {
        ArgumentNullException.ThrowIfNull(programacionesPorNrc);
        ArgumentNullException.ThrowIfNull(registros);
        if (registros.Count == 0) throw new InvalidDataException("PLANEA devolvió un arreglo vacío; el snapshot vigente se conserva.");

        var docentesPorProgramacion = new Dictionary<int, List<(PlaneaRegistro Registro, string Numero, string Nombre, DateOnly Inicio, DateOnly Fin)>>();
        var nombresPorNumero = new Dictionary<string, Dictionary<string, (string Nombre, int Conteo)>>(StringComparer.Ordinal);
        var sesiones = new Dictionary<SesionClave, PlaneaSesionValidada>();
        var ignorados = 0;
        var duplicados = 0;
        var advertencias = 0;
        var nrcSinProgramacion = 0;

        foreach (var registro in registros)
        {
            var periodo = Requerido(registro.Periodo, "PERIODO");
            if (!string.Equals(periodo, clavePeriodo, StringComparison.Ordinal))
                throw new InvalidDataException("La respuesta contiene registros de un periodo distinto al solicitado.");

            var nrc = Requerido(registro.Nrc, "NRC").ToUpperInvariant();
            if (!NrcValido.IsMatch(nrc))
                throw new InvalidDataException($"El NRC {nrc} tiene un formato no válido.");
            if (!programacionesPorNrc.TryGetValue(nrc, out var programacionId))
            {
                nrcSinProgramacion++;
                ignorados++;
                continue;
            }

            var numeroPersonal = NormalizarNumeroPersonal(registro.NumeroPersonal);
            var nombre = NormalizarNombre(registro.Nombre);
            // PLANEA envía algunos identificadores reservados sin nombre; se conserva el horario
            // y se trata ese docente como desconocido.
            if (numeroPersonal is null || nombre is null)
            {
                numeroPersonal = null;
                nombre = null;
            }

            if (numeroPersonal is not null && nombre is not null)
            {
                var claveNombre = NormalizarTexto(nombre);
                if (!nombresPorNumero.TryGetValue(numeroPersonal, out var variantes))
                    nombresPorNumero[numeroPersonal] = variantes = new Dictionary<string, (string, int)>(StringComparer.Ordinal);
                if (variantes.TryGetValue(claveNombre, out var existente)) variantes[claveNombre] = (existente.Nombre, existente.Conteo + 1);
                else variantes[claveNombre] = (nombre, 1);
            }

            var horariosValidos = new List<(byte Dia, TimeOnly Inicio, TimeOnly Fin)>();
            foreach (var horario in registro.Horarios)
            {
                if (horario.DiaSemana is < 1 or > 6)
                    throw new InvalidDataException($"El NRC {nrc} contiene un día de semana no válido.");
                var inicioNulo = string.IsNullOrWhiteSpace(horario.Inicio);
                var finNulo = string.IsNullOrWhiteSpace(horario.Fin);
                if (inicioNulo && finNulo) continue;
                if (inicioNulo || finNulo)
                    throw new InvalidDataException($"El NRC {nrc} contiene un par incompleto de horas.");

                var horaInicio = ParsearHora(horario.Inicio!, nrc, horario.DiaSemana);
                var horaFin = ParsearHora(horario.Fin!, nrc, horario.DiaSemana);
                if (horaInicio >= horaFin)
                    throw new InvalidDataException($"El NRC {nrc} contiene horas inválidas o un horario que cruza medianoche.");

                horariosValidos.Add((horario.DiaSemana, horaInicio, horaFin));
            }

            if (horariosValidos.Count == 0)
            {
                ignorados++;
                continue;
            }

            var fechaInicio = ParsearFecha(registro.FechaInicio, "FECHA_INICIO");
            var fechaFin = ParsearFecha(registro.FechaFin, "FECHA_FIN");
            if (fechaInicio > fechaFin) throw new InvalidDataException($"El NRC {nrc} tiene un rango de fechas inverso.");

            foreach (var horario in horariosValidos)
            {
                var sesion = new PlaneaSesionValidada(programacionId, horario.Dia, horario.Inicio, horario.Fin,
                    fechaInicio, fechaFin, NormalizarLugar(registro.Edificio, 100), NormalizarLugar(registro.Aula, 100));
                var clave = new SesionClave(programacionId, horario.Dia, horario.Inicio, horario.Fin, fechaInicio,
                    fechaFin, NormalizarTexto(sesion.Edificio), NormalizarTexto(sesion.Aula));
                if (!sesiones.TryAdd(clave, sesion))
                    duplicados++;
                else
                {
                    if (fechaInicio < inicioPeriodo || fechaFin > finPeriodo) advertencias++;
                }
            }

            if (numeroPersonal is not null && nombre is not null)
            {
                var claveNombre = NormalizarTexto(nombre);
                if (!nombresPorNumero.TryGetValue(numeroPersonal, out var variantes))
                    nombresPorNumero[numeroPersonal] = variantes = new Dictionary<string, (string, int)>(StringComparer.Ordinal);
                if (variantes.TryGetValue(claveNombre, out var existente)) variantes[claveNombre] = (existente.Nombre, existente.Conteo + 1);
                else variantes[claveNombre] = (nombre, 1);
            }

            if (numeroPersonal is not null && nombre is not null)
            {
                if (!docentesPorProgramacion.TryGetValue(programacionId, out var lista))
                    docentesPorProgramacion[programacionId] = lista = [];
                lista.Add((registro, numeroPersonal, nombre, fechaInicio, fechaFin));
            }
        }

        var docentesValidados = new List<PlaneaDocenteValidado>();
        foreach (var (programacionId, candidatos) in docentesPorProgramacion)
        {
            var ids = candidatos.Select(x => x.Numero).Distinct(StringComparer.Ordinal).ToArray();
            if (ids.Length > 1) advertencias++;
            var elegido = candidatos.OrderByDescending(x => string.Equals(x.Registro.IndPrincipal?.Trim(), "SI", StringComparison.OrdinalIgnoreCase))
                .ThenBy(x => int.TryParse(x.Registro.IndDocente, NumberStyles.Integer, CultureInfo.InvariantCulture, out var indice) ? indice : int.MaxValue)
                .ThenBy(x => x.Numero, StringComparer.Ordinal).First();
            var variante = nombresPorNumero[elegido.Numero].OrderByDescending(x => x.Value.Conteo)
                .ThenBy(x => x.Key, StringComparer.Ordinal).First().Value.Nombre;
            docentesValidados.Add(new PlaneaDocenteValidado(programacionId, elegido.Numero, variante,
                candidatos.Min(x => x.Inicio), candidatos.Max(x => x.Fin)));
        }

        advertencias += nombresPorNumero.Values.Count(x => x.Count > 1);

        var listaSesiones = sesiones.Values.ToArray();
        advertencias += ContarSesionesConTraslape(listaSesiones);
        return new PlaneaSnapshotValidado(listaSesiones, docentesValidados, ignorados, duplicados,
            advertencias, nrcSinProgramacion);
    }

    private static int ContarSesionesConTraslape(IReadOnlyList<PlaneaSesionValidada> sesiones)
    {
        var advertencias = 0;
        foreach (var grupo in sesiones.GroupBy(x => new { x.ProgramacionAcademicaId, x.DiaSemana }))
        {
            var activas = new PriorityQueue<PlaneaSesionValidada, int>();
            var arbol = new ArbolConteoRango(1440);
            foreach (var actual in grupo.OrderBy(x => x.FechaInicio).ThenBy(x => x.HoraInicio))
            {
                while (activas.TryPeek(out var expirada, out _) && expirada.FechaFin < actual.FechaInicio)
                {
                    activas.Dequeue();
                    arbol.Actualizar(Minuto(expirada.HoraInicio), Minuto(expirada.HoraFin), -1);
                }

                if (arbol.HayElementos(Minuto(actual.HoraInicio), Minuto(actual.HoraFin))) advertencias++;
                activas.Enqueue(actual, actual.FechaFin.DayNumber);
                arbol.Actualizar(Minuto(actual.HoraInicio), Minuto(actual.HoraFin), 1);
            }
        }
        return advertencias;
    }

    private static int Minuto(TimeOnly hora) => hora.Hour * 60 + hora.Minute;

    private static DateOnly ParsearFecha(string? valor, string nombreCampo)
    {
        var texto = Requerido(valor, nombreCampo);
        if (!DateOnly.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            throw new InvalidDataException($"PLANEA devolvió una fecha inválida en {nombreCampo}.");
        return fecha;
    }

    private static TimeOnly ParsearHora(string valor, string nrc, byte dia)
    {
        var texto = valor.Trim();
        if (!TimeOnly.TryParseExact(texto, "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            throw new InvalidDataException($"El NRC {nrc} contiene una hora inválida para el día {dia}.");
        return hora;
    }

    private static string? NormalizarNumeroPersonal(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        var normalizado = valor.Trim().ToUpperInvariant();
        if (!NumeroPersonalValido.IsMatch(normalizado))
            throw new InvalidDataException("PLANEA devolvió un ID_DOCENTE fuera del formato permitido.");
        return normalizado;
    }

    private static string? NormalizarNombre(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        var normalizado = valor.Trim();
        if (normalizado.Length > 200) throw new InvalidDataException("PLANEA devolvió un NOMBRE de docente demasiado largo.");
        return normalizado;
    }

    private static string? NormalizarLugar(string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        var normalizado = valor.Trim();
        if (normalizado.Length > maximo) throw new InvalidDataException("PLANEA devolvió un edificio o aula demasiado largo.");
        return normalizado;
    }

    private static string Requerido(string? valor, string campo) =>
        string.IsNullOrWhiteSpace(valor) ? throw new InvalidDataException($"PLANEA omitió el campo requerido {campo}.") : valor.Trim();

    private static string NormalizarTexto(string? texto) => string.IsNullOrEmpty(texto) ? "" :
        new string(texto.Trim().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Normalize(NormalizationForm.FormC).ToUpperInvariant();

    private sealed record SesionClave(int ProgramacionAcademicaId, byte DiaSemana, TimeOnly HoraInicio,
        TimeOnly HoraFin, DateOnly FechaInicio, DateOnly FechaFin, string Edificio, string Aula);

    private sealed class ArbolConteoRango
    {
        private readonly int[] _maximo;
        private readonly int[] _perezoso;
        private readonly int _cantidad;

        public ArbolConteoRango(int cantidad)
        {
            _cantidad = cantidad;
            _maximo = new int[cantidad * 4];
            _perezoso = new int[cantidad * 4];
        }

        public bool HayElementos(int inicio, int fin) => Consultar(1, 0, _cantidad - 1, inicio, fin) > 0;

        public void Actualizar(int inicio, int fin, int delta) => Actualizar(1, 0, _cantidad - 1, inicio, fin, delta);

        private int Consultar(int nodo, int izquierda, int derecha, int inicio, int fin)
        {
            if (inicio <= izquierda && derecha <= fin) return _maximo[nodo];
            Propagar(nodo);
            var medio = (izquierda + derecha) / 2;
            var maximo = int.MinValue;
            if (inicio <= medio) maximo = Math.Max(maximo, Consultar(nodo * 2, izquierda, medio, inicio, fin));
            if (fin > medio) maximo = Math.Max(maximo, Consultar(nodo * 2 + 1, medio + 1, derecha, inicio, fin));
            return maximo;
        }

        private void Actualizar(int nodo, int izquierda, int derecha, int inicio, int fin, int delta)
        {
            if (inicio <= izquierda && derecha <= fin)
            {
                _maximo[nodo] += delta;
                _perezoso[nodo] += delta;
                return;
            }
            Propagar(nodo);
            var medio = (izquierda + derecha) / 2;
            if (inicio <= medio) Actualizar(nodo * 2, izquierda, medio, inicio, fin, delta);
            if (fin > medio) Actualizar(nodo * 2 + 1, medio + 1, derecha, inicio, fin, delta);
            _maximo[nodo] = Math.Max(_maximo[nodo * 2], _maximo[nodo * 2 + 1]);
        }

        private void Propagar(int nodo)
        {
            var delta = _perezoso[nodo];
            if (delta == 0) return;
            foreach (var hijo in new[] { nodo * 2, nodo * 2 + 1 })
            {
                _maximo[hijo] += delta;
                _perezoso[hijo] += delta;
            }
            _perezoso[nodo] = 0;
        }
    }
}
