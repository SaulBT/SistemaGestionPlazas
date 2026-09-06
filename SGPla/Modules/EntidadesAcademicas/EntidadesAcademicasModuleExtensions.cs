using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.ActualizarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas;
using SGPla.Modules.EntidadesAcademicas.Application.ConsultarEntidadesAcademicas.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.CrearEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica;
using SGPla.Modules.EntidadesAcademicas.Application.EliminarEntidadAcademica.Ports;
using SGPla.Modules.EntidadesAcademicas.Application.Ports;
using SGPla.Modules.EntidadesAcademicas.Infra;

namespace SGPla.Modules.EntidadesAcademicas;

public static class EntidadesAcademicasModuleExtensions
{
    public static IServiceCollection AddEntidadesAcademicasModule(
        this IServiceCollection services)
    {
        services.AddScoped<IEntidadAcademicaRepository, EntidadAcademicaRepository>();
        services.AddScoped<IConsultarEntidadesAcademicasService, ConsultarEntidadesAcademicasService>();
        services.AddScoped<IConsultarEntidadAcademicaService, ConsultarEntidadAcademicaService>();
        services.AddScoped<ICrearEntidadAcademicaService, CrearEntidadAcademicaService>();
        services.AddScoped<IActualizarEntidadAcademicaService, ActualizarEntidadAcademicaService>();
        services.AddScoped<IEliminarEntidadAcademicaService, EliminarEntidadAcademicaService>();

        return services;
    }
}
