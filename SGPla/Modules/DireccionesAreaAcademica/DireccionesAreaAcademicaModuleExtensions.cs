using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.ActualizarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas;
using SGPla.Modules.DireccionesAreaAcademica.Application.ConsultarAreasAcademicas.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.CrearAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica;
using SGPla.Modules.DireccionesAreaAcademica.Application.EliminarAreaAcademica.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Application.Ports;
using SGPla.Modules.DireccionesAreaAcademica.Infra;

namespace SGPla.Modules.DireccionesAreaAcademica;

public static class DireccionesAreaAcademicaModuleExtensions
{
    public static IServiceCollection AddDireccionesAreaAcademicaModule(
        this IServiceCollection services)
    {
        services.AddScoped<IAreaAcademicaRepository, AreaAcademicaRepository>();
        services.AddScoped<IConsultarAreasAcademicasService, ConsultarAreasAcademicasService>();
        services.AddScoped<IConsultarAreaAcademicaService, ConsultarAreaAcademicaService>();
        services.AddScoped<ICrearAreaAcademicaService, CrearAreaAcademicaService>();
        services.AddScoped<IActualizarAreaAcademicaService, ActualizarAreaAcademicaService>();
        services.AddScoped<IEliminarAreaAcademicaService, EliminarAreaAcademicaService>();

        return services;
    }
}
