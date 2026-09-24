using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.DTOs.Docentes;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers;

[Route("Docentes")]
[Authorize(Policy = PoliticasAutorizacion.EntidadAcademica)]
public sealed class DocentesMvcController : Controller
{
    private readonly IDocenteDirectorioMvcService _service;

    public DocentesMvcController(IDocenteDirectorioMvcService service) => _service = service;

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(DocenteDirectorioMvcFiltro filtro, CancellationToken cancellationToken)
    {
        filtro = filtro with { Pagina = Math.Max(1, filtro.Pagina), TamanoPagina = Math.Clamp(filtro.TamanoPagina, 1, 100) };
        var resultado = await _service.BuscarAsync(filtro, cancellationToken);
        ViewData["Busqueda"] = filtro.Busqueda;
        ViewData["Pagina"] = filtro.Pagina;
        ViewData["TamanoPagina"] = filtro.TamanoPagina;
        ViewData["Total"] = resultado.Total;
        ViewData["TotalPaginas"] = (int)Math.Ceiling(resultado.Total / (double)filtro.TamanoPagina);
        return View("IndexMvc", resultado.Items);
    }
}
