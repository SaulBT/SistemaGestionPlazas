using Microsoft.Extensions.DependencyInjection;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.ActualizarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares;
using SGPla.Modules.PeriodosEscolares.Application.ConsultarPeriodosEscolares.Ports;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.CrearPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar;
using SGPla.Modules.PeriodosEscolares.Application.EliminarPeriodoEscolar.Ports;
using SGPla.Modules.PeriodosEscolares.Application.Ports;
using SGPla.Modules.PeriodosEscolares.Infra;

namespace SGPla.Modules.PeriodosEscolares;

public static class PeriodosEscolaresModuleExtensions
{
    public static IServiceCollection AddPeriodosEscolaresModule(
        this IServiceCollection services)
    {
        services.AddScoped<IPeriodoEscolarRepository, PeriodoEscolarRepository>();
        services.AddScoped<IConsultarPeriodosEscolaresService, ConsultarPeriodosEscolaresService>();
        services.AddScoped<IConsultarPeriodoEscolarService, ConsultarPeriodoEscolarService>();
        services.AddScoped<ICrearPeriodoEscolarService, CrearPeriodoEscolarService>();
        services.AddScoped<IActualizarPeriodoEscolarService, ActualizarPeriodoEscolarService>();
        services.AddScoped<IEliminarPeriodoEscolarService, EliminarPeriodoEscolarService>();

        return services;
    }
}
