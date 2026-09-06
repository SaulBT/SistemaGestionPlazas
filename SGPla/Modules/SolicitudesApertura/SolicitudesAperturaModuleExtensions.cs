using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Infra;

namespace SGPla.Modules.SolicitudesApertura;

public static class SolicitudesAperturaModuleExtensions
{
    public static IServiceCollection AddSolicitudesAperturaModule(
        this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddSingleton<IFechaActualProvider, FechaActualProvider>();
        services.AddScoped<IArchivoOficioStorage, ArchivoOficioStorage>();
        services.AddScoped<ISolicitudAperturaRepository, SolicitudAperturaRepository>();
        services.AddScoped<ICrearSolicitudAperturaService, CrearSolicitudAperturaService>();

        return services;
    }
}
