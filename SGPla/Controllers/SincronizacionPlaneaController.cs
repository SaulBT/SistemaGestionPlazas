using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Planea;
using SGPla.Models.ViewModels.SincronizacionPlanea;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    [Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
    public class SincronizacionPlaneaController(IConsultaPlaneaService servicio) : Controller
    {
        private static readonly List<string> HeadersBitacora = ["Periodo", "Inicio", "Fin", "Estado", "NRC recibidos", "Nuevos", "Ya registrados", "Sin plan", "Sin EE en catálogo", "Horarios", "Acciones"];
        private static readonly List<string> HeadersCopias = ["Periodo", "NRC", "Código EE", "Experiencia Educativa", "Plan", "Región", "Campus", "Horarios", "Alta", "Acciones"];

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is ValidacionExcepction ex)
            {
                TempData["Error"] = ex.Message;
                context.ExceptionHandled = true;
                context.Result = RedirectToAction(nameof(Index));
            }
            base.OnActionExecuted(context);
        }

        public async Task<IActionResult> Index(int? idPeriodo, string? estado, int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default)
        {
            var filtro = new FiltroBitacoraPlaneaDTO { IdPeriodo = idPeriodo, Estado = estado, Pagina = pagina, Cantidad = cantidad };
            var (items,total) = await servicio.ObtenerBitacoraAsync(filtro, cancellationToken);
            var rows = items.Select(x => new TableRowModel { Cells = [
                new() { Value=x.CodigoPeriodo }, new() { Value=x.FechaInicio.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) },
                new() { Value=x.FechaFin?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? "—" }, new() { Value=x.Estado },
                new() { Value=Fmt(x.NrcRecibidos) }, new() { Value=Fmt(x.NrcNuevos) }, new() { Value=Fmt(x.NrcExistentes) },
                new() { Value=Fmt(x.NrcSinPlan) }, new() { Value=Fmt(x.NrcSinExperiencia) }, new() { Value=Fmt(x.HorariosInsertados) },
                new() { Actions=[new TableActionModel { Accion="Ver detalle", OnClick=$"abrirDetallePlanea({JsonSerializer.Serialize(new { x.Advertencias, x.MensajeError })})" }] }
            ]}).ToList();
            return View(new IndexViewModel { Table = rows.Count == 0 ? TablaFactory.GenerarTablaConMensaje(HeadersBitacora, "No hay sincronizaciones registradas.") : new TableModel { Headers=HeadersBitacora, Rows=rows, Pagination=new PaginationInfo { CurrentPage=filtro.Pagina, PageSize=filtro.Cantidad, TotalItems=total, OnPageChange="cambiarPagina" } },
                Periodos=await servicio.ObtenerPeriodosConSincronizacionAsync(cancellationToken), Estados=PlaneaConstantes.ESTADOS.Select(s => new OptionModel { Value=s, Text=s }).ToList(), IdPeriodo=idPeriodo, Estado=filtro.Estado, PaginaActual=filtro.Pagina, CantidadPorPagina=filtro.Cantidad });
        }

        public async Task<IActionResult> Copias(int? idPeriodo, string? busqueda, int? idPlanEstudios, int? idRegion, int pagina = 1, int cantidad = 20, CancellationToken cancellationToken = default)
        {
            var filtro = new FiltroCopiaPlaneaDTO { IdPeriodo=idPeriodo, Busqueda=busqueda, IdPlanEstudios=idPlanEstudios, IdRegion=idRegion, Pagina=pagina, Cantidad=cantidad };
            var (items,total) = await servicio.ObtenerCopiasAsync(filtro, cancellationToken);
            var rows = items.Select(x => new TableRowModel { Cells = [
                new() { Value=x.CodigoPeriodo }, new() { Value=x.Nrc }, new() { Value=x.CodigoExperiencia }, new() { Value=x.NombreExperiencia },
                new() { Value=x.CodigoPlan ?? "—" }, new() { Value=x.Region ?? "—" }, new() { Value=x.Campus ?? "—" }, new() { Value=x.Horarios.ToString() },
                new() { Value=x.FechaAlta.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) },
                new() { Actions=[new TableActionModel { Accion="Ver", Url=Url.Action(nameof(DetalleCopia), new { id=x.IdExperienciaEducativaPeriodo, idPeriodo, busqueda, idPlanEstudios, idRegion, pagina=filtro.Pagina, cantidad=filtro.Cantidad }) }] }
            ]}).ToList();
            return View(new CopiasViewModel { Table=rows.Count == 0 ? TablaFactory.GenerarTablaConMensaje(HeadersCopias, "No hay copias registradas.") : new TableModel { Headers=HeadersCopias, Rows=rows, Pagination=new PaginationInfo { CurrentPage=filtro.Pagina, PageSize=filtro.Cantidad, TotalItems=total, OnPageChange="cambiarPagina" } },
                Periodos=await servicio.ObtenerPeriodosConSincronizacionAsync(cancellationToken), Planes=await servicio.ObtenerPlanesConCopiasAsync(idPeriodo,cancellationToken), Regiones=await servicio.ObtenerRegionesAsync(cancellationToken),
                Busqueda=filtro.Busqueda, IdPeriodo=idPeriodo, IdPlanEstudios=idPlanEstudios, IdRegion=idRegion, PaginaActual=filtro.Pagina, CantidadPorPagina=filtro.Cantidad });
        }

        public async Task<IActionResult> DetalleCopia(int id, int? idPeriodo, string? busqueda, int? idPlanEstudios, int? idRegion, int pagina=1, int cantidad=20, CancellationToken cancellationToken=default)
        {
            var detalle = await servicio.ObtenerDetalleCopiaAsync(id,cancellationToken);
            if (detalle is null) return NotFound();
            var volver = Url.Action(nameof(Copias), new { idPeriodo, busqueda, idPlanEstudios, idRegion, pagina, cantidad })!;
            return View(new DetalleCopiaViewModel { Detalle=detalle, UrlVolver=volver, TablaHorarios=new TableModel { Headers=["Día","Inicio","Fin","Edificio","Aula","Fecha inicio","Fecha fin"], Rows=detalle.Horarios.Select(h => new TableRowModel { Cells=[
                new() { Value=h.Dia }, new() { Value=h.HoraInicio.ToString("HH:mm") }, new() { Value=h.HoraFin.ToString("HH:mm") }, new() { Value=h.Edificio ?? "—" }, new() { Value=h.Aula ?? "—" },
                new() { Value=h.FechaInicio?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—" }, new() { Value=h.FechaFin?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—" }
            ]}).ToList() } });
        }
        private static string Fmt(int? valor) => valor?.ToString(CultureInfo.InvariantCulture) ?? "—";
    }
}
