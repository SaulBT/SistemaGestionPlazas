using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.Articulos.Application.ActualizarArticulo;
using SGPla.Modules.Articulos.Application.ConsultarArticulo;
using SGPla.Modules.Articulos.Application.ConsultarArticulos;
using SGPla.Modules.Articulos.Application.CrearArticulo;
using SGPla.Modules.Articulos.Application.EliminarArticulo;
using SGPla.Modules.Articulos.Application.Ports;
using SGPla.Modules.Articulos.Infra;

namespace SGPla.Modules.Articulos;

public static class ArticulosModuleExtensions
{
    public static IServiceCollection AddArticulosModule(
        this IServiceCollection services)
    {
        services.AddScoped<IArticuloRepository, ArticuloRepository>();
        services.AddScoped<IConsultarArticulosService, ConsultarArticulosService>();
        services.AddScoped<IConsultarArticuloService, ConsultarArticuloService>();
        services.AddScoped<ICrearArticuloService, CrearArticuloService>();
        services.AddScoped<IActualizarArticuloService, ActualizarArticuloService>();
        services.AddScoped<IEliminarArticuloService, EliminarArticuloService>();

        return services;
    }
}
