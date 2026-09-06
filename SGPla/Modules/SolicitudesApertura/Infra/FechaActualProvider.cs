using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;

namespace SGPla.Modules.SolicitudesApertura.Infra;

public sealed class FechaActualProvider : IFechaActualProvider
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _zonaHoraria;

    public FechaActualProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _zonaHoraria = ObtenerZonaHorariaMexico();
    }

    public DateOnly ObtenerFechaActual()
    {
        var fechaHoraMexico = TimeZoneInfo.ConvertTime(
            _timeProvider.GetUtcNow(),
            _zonaHoraria);

        return DateOnly.FromDateTime(fechaHoraMexico.DateTime);
    }

    private static TimeZoneInfo ObtenerZonaHorariaMexico()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        }
    }
}
