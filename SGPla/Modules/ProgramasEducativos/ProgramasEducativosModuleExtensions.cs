using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.ActualizarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos;
using SGPla.Modules.ProgramasEducativos.Application.ConsultarProgramasEducativos.Ports;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.CrearProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo;
using SGPla.Modules.ProgramasEducativos.Application.EliminarProgramaEducativo.Ports;
using SGPla.Modules.ProgramasEducativos.Application.Ports;
using SGPla.Modules.ProgramasEducativos.Infra;

namespace SGPla.Modules.ProgramasEducativos;

public static class ProgramasEducativosModuleExtensions
{
    public static IServiceCollection AddProgramasEducativosModule(
        this IServiceCollection services)
    {
        services.AddScoped<IProgramaEducativoRepository, ProgramaEducativoRepository>();
        services.AddScoped<IConsultarProgramasEducativosService, ConsultarProgramasEducativosService>();
        services.AddScoped<IConsultarProgramaEducativoService, ConsultarProgramaEducativoService>();
        services.AddScoped<ICrearProgramaEducativoService, CrearProgramaEducativoService>();
        services.AddScoped<IActualizarProgramaEducativoService, ActualizarProgramaEducativoService>();
        services.AddScoped<IEliminarProgramaEducativoService, EliminarProgramaEducativoService>();

        return services;
    }
}
