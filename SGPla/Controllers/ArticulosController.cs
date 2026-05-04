using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SGPla.Mappers;
using SGPla.Models;
using SGPla.Models.DTOs.Articulo;
using SGPla.Models.ViewModels.Articulos;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class ArticulosController : Controller
    {
        private readonly IArticuloService _articuloService;
        private readonly ILogger<ArticulosController> _logger;

        public ArticulosController(IArticuloService articuloService, ILogger<ArticulosController> logger)
        {
            _articuloService = articuloService;
            _logger = logger;
        }

        //GET: Articulos
        public async Task<IActionResult> Index(string? busqueda)
        {
            return View(new IndexViewModel 
            { 
                Table = await LlenarTabla(busqueda),
                Busqueda = busqueda 
            });
        }
        private async Task<TableModel> LlenarTabla(string? busqueda)
        {
            try
            {
                IEnumerable<DetallesArticuloDTO> articulos;
                if (busqueda.IsNullOrEmpty() || busqueda.IsWhiteSpace())
                {
                    articulos = await _articuloService.ObtenerTodosAsync();
                } else
                {
                    articulos = await _articuloService.BuscarPorTerminoAsync(busqueda);
                }
                
                return new TableModel
                {
                    Headers = new List<string> { "Artículo", "Descripción", "Acciones" },
                    Rows = articulos.Select(a => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                    {
                        new TableCellModel { Value = a.Numero},
                        new TableCellModel { Value = a.Descripcion },
                        new TableCellModel
                        {
                            Actions = new List<TableActionModel>
                            {
                                new TableActionModel()
                                {
                                    Accion = "Editar",
                                    OnClick = $"abrirModalEditarArticulo({a.IdArticulo}, '{a.Numero}', '{a.Descripcion}')"
                                },
                                new TableActionModel()
                                {
                                    Accion = "Eliminar",
                                    OnClick = $"abrirModalConfirmacion('¿Desea eliminar este artículo?', function() {{ eliminarArticulo({a.IdArticulo}); }})"
                                }
                            }
                        }
                    },

                    }).ToList()
                };
            }
     
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llenar la tabla de artículos");
                TempData["Error"] = "Ocurrió un error al cargar los artículos. Por favor, inténtelo de nuevo más tarde.";

                return new TableModel();
            }
        }

        /* * * * * * Editar Articulo * * * * * */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarArticuloDTO dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultado = await _articuloService.EditarArticuloAsync(dto);
                    TempData["Success"] = "Cambios guardados con éxito";
                    
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            else
            {
                var model = new IndexViewModel
                {
                    Table = await LlenarTabla(null),

                    Formulario = new ArticuloFormularioViewModel
                    {
                        Numero = dto.Numero,
                        Descripcion = dto.Descripcion
                    }
                };
                return View("Index", model);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearArticuloDTO dto)
        {
            if (!ModelState.IsValid)
            {

                if (dto.Numero.IsNullOrEmpty())
                {
                    ModelState.AddModelError("Numero", "Llene el campo");
                    foreach (var x in ViewData.ModelState["Numero"]?.Errors)
                    {
                        Console.WriteLine();
                    }
                    //Console.WriteLine(ViewData.ModelState["Numero"]?.Errors)
                }
                if (dto.Descripcion.IsNullOrEmpty())
                    ModelState.AddModelError("Descripcion", "Llene el campo");

                    var model = new IndexViewModel
                {
                    Table = await LlenarTabla(null),
                    
                    Formulario = new ArticuloFormularioViewModel
                    {
                        Numero = dto.Numero,
                        Descripcion = dto.Descripcion
                    }
                };

                return View("Index", model);
            }

            try
            {
                var resultado = await _articuloService.CrearArticuloAsync(dto);

                TempData["Success"] =
                    $"Artículo creado exitosamente";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string busqueda)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var resultados = await _articuloService.BuscarPorTerminoAsync(busqueda);
                      return View("Index", resultados);

                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View("Index");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var resultado = await _articuloService.EliminarArticuloAsync(id);
                if (resultado)
                {
                    TempData["Success"] = $"Artículo eliminado exitosamente";
                }
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
