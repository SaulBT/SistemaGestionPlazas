using Microsoft.AspNetCore.Mvc;
using SGPla.Commons;
using SGPla.Models.Components;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models.ViewModels.EntidadesAcademicas;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class EntidadesAcademicasController : Controller
    {
        private readonly IEntidadAcademicaService _entidadAcademicaService;
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly ILogger<EntidadesAcademicasController> _logger;

        public EntidadesAcademicasController(IEntidadAcademicaService entidadAcademicaService, ILogger<EntidadesAcademicasController> logger, IAreaAcademicaService areaAcademicaService)
        {
            _entidadAcademicaService = entidadAcademicaService;
            _logger = logger;
            _areaAcademicaService = areaAcademicaService;
        }

        // GET: EntidadesAcademicas

        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica)
        {
            // combos
            var regionesCombo = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();

            var areas = await _areaAcademicaService.ObtenerTodasAsync();
            var areasCombo = areas
                .Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();


            return View(new IndexViewModel
            {
                Table = await LlenarTabla(busqueda, region, idAreaAcademica),
                Regiones = regionesCombo,
                Areas = areasCombo,
                RegionSeleccionada = region,
                idAreaSeleccionada = idAreaAcademica,
                Busqueda = busqueda?.ToString()
            });
        }

        private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idAreaAcademica)
        {
            try
            {
                FiltroEntidadAcademicaDTO filtros = new FiltroEntidadAcademicaDTO
                {
                    Nombre = busqueda ?? string.Empty,
                    Region = region ?? string.Empty,
                    IdAreaAcademica = idAreaAcademica ?? -1
                };
                var entidades = await _entidadAcademicaService.ObtenerPorFiltroAsync(filtros, 1);

                return new TableModel
                {
                    Headers = new List<string>
                    {
                        "Nombre", "Domicilio", "Telefono",
                        "Área Académica", "Región", "Acciones"
                    },
                    Rows = entidades.Select(e => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new TableCellModel { Value = e.Nombre },
                            new TableCellModel { Value = e.Domicilio },
                            new TableCellModel { Value = e.Telefono },
                            new TableCellModel { Value = e.NombreAreaAcademica ?? "N/A" },
                            new TableCellModel { Value = e.Region },
                            new()
                            {
                                Actions = new List<TableActionModel>
                                {
                                    new()
                                    {
                                        Accion = "ver",
                                        Url = Url.Action("VerEntidadAcademica", "EntidadesAcademicas", new { id = e.IdEntidadAcademica})
                                    },
                                    new()
                                    {
                                        Accion = "programa educativo",
                                        Url = Url.Action("Buscar", "ProgramasEducativos", new
                                        {
                                            IdEntidadAcademica = e.IdEntidadAcademica
                                        })
                                    },
                                    new()
                                    {
                                        Accion = "editar",
                                        Url = Url.Action("EditarEntidadAcademica", "EntidadesAcademicas", new { id = e.IdEntidadAcademica})
                                    },
                                    new()
                                    {
                                        Accion = "eliminar",
                                        OnClick = $"abrirModalConfirmacion('¿Desea eliminar esta entidad académica?', function() {{ eliminarEntidadAcademica({e.IdEntidadAcademica}); }})"
                                    }
                                }
                            }
                        }
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de entidades académicas");
                TempData["Error"] = ex.Message;

                return new TableModel();
            }

        }

        // Crear

        public async Task<IActionResult> CrearEntidadAcademica(string? region, int? idAreaAcademica)
        {
            return View(await ObtenerModelo(region, idAreaAcademica));
        }

        private async Task<CrearEntidadAcademicaViewModel> ObtenerModelo(string? region, int? idAreaAcademica)
        {
            var regionesCombo = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();

            var areas = await _areaAcademicaService.ObtenerTodasAsync();

            var areasCombo = areas
                .Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();

            return (new CrearEntidadAcademicaViewModel
            {
                Regiones = regionesCombo,
                Areas = areasCombo
            });
        }

        private async Task CargarCombos(CrearEntidadAcademicaViewModel model)
        {
            model.Regiones = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Region
                }).ToList();

            var areas = await _areaAcademicaService.ObtenerTodasAsync();

            model.Areas = areas.Select(a => new OptionModel
            {
                Value = a.IdAreaAcademica.ToString(),
                Text = a.Nombre,
                Selected = model.IdAreaAcademica.HasValue &&
                           a.IdAreaAcademica == model.IdAreaAcademica.Value
            }).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntidadAcademica(CrearEntidadAcademicaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos(model);
                return View(model);
            }

            var dto = new CrearEntidadAcademicaDTO
            {
                Clave = model.Clave,
                Nombre = model.Nombre,
                CalleNumero = model.CalleNumero,
                Colonia = model.Colonia,
                Cp = model.Cp,
                Municipio = model.Municipio,
                Telefono = model.Telefono,
                Conmutador = model.Conmutador,
                Extension = model.Extension,
                Fax = model.Fax,
                IdAreaAcademica = model.IdAreaAcademica,
                Region = model.Region
            };

            try
            {
                await _entidadAcademicaService.CrearAsync(dto);
                TempData["Success"] = "Entidad académica creada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la entidad académica");
                TempData["Error"] = ex.Message;
                await CargarCombos(model);
                return View(model);
            }
        }

        // Ver

        public async Task<IActionResult> VerEntidadAcademica(int id)
        {
            try
            {
                var entidad = await _entidadAcademicaService.ObtenerPorIdAsync(id);
                if (entidad == null)
                {
                    TempData["Error"] = "No se encontró la entidad académica.";
                    return RedirectToAction("Index");
                }
                return View(entidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la entidad académica");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Editar

        public async Task<IActionResult> EditarEntidadAcademica(int id)
        {
            var entidad = await _entidadAcademicaService.ObtenerPorIdAsync(id);

            var model = new CrearEntidadAcademicaViewModel
            {
                IdEntidadAcademica = id,
                Clave = entidad.Clave,
                Nombre = entidad.Nombre,
                CalleNumero = entidad.CalleNumero,
                Colonia = entidad.Colonia,
                Cp = entidad.Cp,
                Municipio = entidad.Municipio,
                Telefono = entidad.Telefono,
                Conmutador = entidad.Conmutador,
                Extension = entidad.Extension,
                Fax = entidad.Fax,
                IdAreaAcademica = entidad.IdAreaAcademica,
                Region = entidad.Region
            };

            await CargarCombos(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEntidadAcademica(CrearEntidadAcademicaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos(model);
                return View(model);
            }

            var dto = new DatosEntidadAcademicaDTO
            {
                IdEntidadAcademica = model.IdEntidadAcademica,
                Clave = model.Clave,
                Nombre = model.Nombre,
                CalleNumero = model.CalleNumero,
                Colonia = model.Colonia,
                Cp = model.Cp,
                Municipio = model.Municipio,
                Telefono = model.Telefono,
                Conmutador = model.Conmutador,
                Extension = model.Extension,
                Fax = model.Fax,
                IdAreaAcademica = model.IdAreaAcademica.Value,
                Region = model.Region
            };

            try
            {
                await _entidadAcademicaService.EditarAsync(dto);
                TempData["Success"] = "Entidad académica editada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar la entidad académica");
                TempData["Error"] = ex.Message;
                await CargarCombos(model);
                return View(model);
            }
        }

        // Eliminar

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEntidadAcademica(int id)
        {
            try
            {
                await _entidadAcademicaService.EliminarAsync(id);
                TempData["Success"] = "Entidad académica eliminada correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la entidad académica {Id}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
