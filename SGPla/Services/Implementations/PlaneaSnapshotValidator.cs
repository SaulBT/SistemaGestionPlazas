using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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

        var sesionesPorClave = new Dictionary<SesionClave, PlaneaSesionValidada>();
        var docentePorProgramacion = new Dictionary<int, PlaneaDocenteValidado>();
        var nombrePorNumeroPersonal = new Dictionary<string, string>(StringComparer.Ordinal);
        var ignorados = 0;
        var duplicados = 0;
        var advertencias = 0;

        foreach (var registro in registros)
        {
            var periodo = Requerido(registro.Periodo, "PERIODO");
            if (!string.Equals(periodo, clavePeriodo, StringComparison.Ordinal))
                throw new InvalidDataException("La respuesta contiene registros de un periodo distinto al solicitado.");

            var nrc = Requerido(registro.Nrc, "NRC").ToUpperInvariant();
            if (!NrcValido.IsMatch(nrc) || !programacionesPorNrc.TryGetValue(nrc, out var programacionId))
                throw new InvalidDataException($"El NRC {nrc} no tiene una programación activa y única en el periodo solicitado.");

            var fechaInicio = ParsearFecha(registro.FechaInicio, "FECHA_INICIO");
            var fechaFin = ParsearFecha(registro.FechaFin, "FECHA_FIN");
            if (fechaInicio > fechaFin) throw new InvalidDataException($"El NRC {nrc} tiene un rango de fechas inverso.");

            var numeroPersonal = NormalizarNumeroPersonal(registro.NumeroPersonal);
            var nombre = NormalizarNombre(registro.Nombre);
            if ((numeroPersonal is null) != (nombre is null))
                throw new InvalidDataException($"El NRC {nrc} debe informar ID_DOCENTE y NOMBRE juntos o dejar ambos nulos.");

            var totalSesionesRegistro = 0;
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

                var sesion = new PlaneaSesionValidada(programacionId, horario.DiaSemana, horaInicio, horaFin,
                    fechaInicio, fechaFin, NormalizarLugar(registro.Edificio, 100), NormalizarLugar(registro.Aula, 100));
                var clave = new SesionClave(programacionId, horario.DiaSemana, horaInicio, horaFin, fechaInicio,
                    fechaFin, NormalizarTexto(sesion.Edificio), NormalizarTexto(sesion.Aula));
                totalSesionesRegistro++;
                if (!sesionesPorClave.TryAdd(clave, sesion))
                    duplicados++;
                else
                {
                    if (fechaInicio < inicioPeriodo || fechaFin > finPeriodo) advertencias++;
                }
            }

            if (totalSesionesRegistro == 0)
            {
                ignorados++;
                continue;
            }

            if (numeroPersonal is not null && nombre is not null)
            {
                if (nombrePorNumeroPersonal.TryGetValue(numeroPersonal, out var nombreAnterior)
                    && !string.Equals(NormalizarTexto(nombreAnterior), NormalizarTexto(nombre), StringComparison.Ordinal))
                    throw new InvalidDataException("PLANEA devolvió nombres distintos para el mismo ID_DOCENTE.");
                nombrePorNumeroPersonal[numeroPersonal] = nombre;

                if (docentePorProgramacion.TryGetValue(programacionId, out var docenteAnterior)
                    && !string.Equals(docenteAnterior.NumeroPersonal, numeroPersonal, StringComparison.Ordinal))
                    throw new InvalidDataException($"El NRC {nrc} identifica más de un docente.");
                if (docenteAnterior is null)
                    docentePorProgramacion[programacionId] = new PlaneaDocenteValidado(programacionId,
                        numeroPersonal, nombre, fechaInicio, fechaFin);
                else
                    docentePorProgramacion[programacionId] = docenteAnterior with
                    {
                        FechaInicio = fechaInicio < docenteAnterior.FechaInicio ? fechaInicio : docenteAnterior.FechaInicio,
                        FechaFin = fechaFin > docenteAnterior.FechaFin ? fechaFin : docenteAnterior.FechaFin
                    };
            }
        }

        var sesiones = sesionesPorClave.Values.ToArray();
        advertencias += ContarSesionesConTraslape(sesiones);
        return new PlaneaSnapshotValidado(sesiones, docentePorProgramacion.Values.ToArray(), ignorados,
            duplicados, advertencias);
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
