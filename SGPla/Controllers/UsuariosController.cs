using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGPla.Commons;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models.ViewModels;
using SGPla.Models.ViewModels.Usuarios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IAreaAcademicaRepository _areaAcademicaRepository;
        private readonly IEntidadAcademicaRepository _entidadAcademicaRepository;
        private readonly IAreaAcademicaService _areaAcademicaService; //TODO: Reemplazar los métodos que usan el repository
        private readonly IEntidadAcademicaService _entidadAcademicaService; //TODO: Reemplazar los métodos que usan el repository
        private int paginaActual = 1;

        public UsuariosController(
            IUsuarioService usuarioService,
            ILogger<UsuariosController> logger,
            IAreaAcademicaRepository areaAcademicaRepository,
            IAreaAcademicaService areaAcademicaService,
            IEntidadAcademicaRepository entidadAcademicaRepository,
            IEntidadAcademicaService entidadAcademicaService)
        {
            _usuarioService = usuarioService;
            _logger = logger;
            _areaAcademicaRepository = areaAcademicaRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
            _areaAcademicaService = areaAcademicaService;
            _entidadAcademicaService = entidadAcademicaService;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int pagina = 1, int cantidad = 10)
        {
            paginaActual = pagina;
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

            List<EntidadAcademica> entidades;

            if (idAreaAcademica.HasValue)
            {
                entidades = await _entidadAcademicaRepository
                    .ObtenerPorIdAreaAcademicaAsync(idAreaAcademica.Value);
            }
            else
            {
                entidades = new List<EntidadAcademica>();
                idEntidadAcademica = null;
            }

            var entidadesCombo = entidades
                .Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = idEntidadAcademica.HasValue &&
                               e.IdEntidadAcademica == idEntidadAcademica.Value
                })
                .ToList();

            return View(new IndexViewModel
            {
                Table = await LlenarTabla(busqueda, region, idAreaAcademica, idEntidadAcademica, pagina, cantidad),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,

                RegionSeleccionada = region,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                Busqueda = busqueda,

                PaginaActual = paginaActual,
                CantidadPorPagina = cantidad
            });
        }


        private async Task<TableModel> LlenarTabla(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica, int pagina = 1, int cantidad = 10)
        {
            try
            {
                FiltrosUsuarioDTO filtros = new FiltrosUsuarioDTO
                {
                    Busqueda = busqueda,
                    Region = region,
                    IdAreaAcademica = idAreaAcademica,
                    IdEntidadAcademica = idEntidadAcademica,
                    Pagina = pagina,
                    Cantidad = cantidad
                };
                var usuarios = await _usuarioService.BuscarPorFiltroPaginadoAsync(filtros);
                if (usuarios.Items.Count == 0)
                {
                    paginaActual = 1;
                    filtros.Pagina = paginaActual;
                    usuarios = await _usuarioService.BuscarPorFiltroPaginadoAsync(filtros);
                }
                return new TableModel
                {
                    Headers = new List<string>
                        {
                            "Nombre", "Correo", "Cargo", "Rol", "Entidad/Área", "Región", "Acciones"
                        },
                    Rows = usuarios.Items.Select(usuario => new TableRowModel
                    {
                        Cells = new List<TableCellModel>
                        {
                            new() { Value = usuario.Nombre },
                            new() { Value = usuario.Correo },
                            new() { Value = usuario.Cargo },
                            new() { Value = usuario.Rol },
                            new()
                        {
                        Value = !string.IsNullOrEmpty(usuario.NombreEntidadAcademica)
                            ? usuario.NombreEntidadAcademica
                            : usuario.NombreAreaAcademica
                    },
                    new() { Value = usuario.Region },
                    new()
                    {
                        Actions = new List<TableActionModel>
                        {
                            new()
                            {
                                Accion = "ver",
                                Url = Url.Action("VerUsuario", "Usuarios", new { id = usuario.IdUsuario, rol = usuario.Rol })
                            },
                            new()
                            {
                                Accion = "editar",
                                Url = Url.Action("EditarUsuario", "Usuarios", new { id = usuario.IdUsuario, rol = usuario.Rol })
                            },
                            new()
                            {
                                Accion = "eliminar",
                                OnClick = $"abrirModalConfirmacion('¿Desea eliminar este usuario?', function() {{ eliminarUsuario({usuario.IdUsuario}, \"{usuario.Rol}\"); }})"
                            }
                        }
                    }
                }
                    }).ToList(),
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = paginaActual,
                        PageSize = cantidad,
                        TotalItems = usuarios.TotalCount,
                        OnPageChange = "cambiarPagina"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                TempData["Error"] = "Error al cargar los usuarios";

                return new TableModel();
            }
        }


        // GET: Usuarios/VerUsuario
        public async Task<IActionResult> VerUsuario(int id, string rol)
        {

            if (id == 0 || string.IsNullOrEmpty(rol))
            {
                return BadRequest();
            }

            try
            {
                var referencia = new ReferenciaUsuarioDTO
                {
                    IdUsuario = id,
                    Rol = rol
                };

                var usuario = await _usuarioService.ObtenerPorIdAsync(referencia);

                if (usuario == null)
                {
                    return NotFound();
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del usuario {Id}", id);
                TempData["Error"] = "Error al cargar los detalles del usuario";
                return RedirectToAction(nameof(Index));
            }
        }

        /* * * * * * Crear Usuario * * * * * */

        //GET: Usuarios/CrearUsuario
        public async Task<IActionResult> CrearUsuario(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? rol)
        {
            return View(await ObtenerModelo(region, idAreaAcademica, idEntidadAcademica, rol));
        }

        private async Task<CrearUsuarioViewModel> ObtenerModelo(string? region, int? idAreaAcademica, int? idEntidadAcademica, string? rol)
        {
            var rolesCombo = Constantes.Roles
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == rol
                })
                .ToList();

            var regionesCombo = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == region
                })
                .ToList();

            var areas = await _areaAcademicaRepository.ObtenerTodosAsync();

            var areasCombo = areas
                .Select(a => new OptionModel
                {
                    Value = a.IdAreaAcademica.ToString(),
                    Text = a.Nombre,
                    Selected = idAreaAcademica.HasValue &&
                               a.IdAreaAcademica == idAreaAcademica.Value
                })
                .ToList();

            List<EntidadAcademica> entidades;

            if (idAreaAcademica.HasValue)
            {
                entidades = await _entidadAcademicaRepository
                    .ObtenerPorIdAreaAcademicaAsync(idAreaAcademica.Value);
            }
            else
            {
                entidades = new List<EntidadAcademica>();
                idEntidadAcademica = null;
            }

            var entidadesCombo = entidades
                .Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = idEntidadAcademica.HasValue &&
                               e.IdEntidadAcademica == idEntidadAcademica.Value
                })
                .ToList();
            return (new CrearUsuarioViewModel
            {
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,
                Roles = rolesCombo
            });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntidades(int idAreaAcademica, string region)
        {
            var entidades = await _entidadAcademicaRepository.ObtenerPorIdAreaAcademicaYRegionAsync(idAreaAcademica, region);


            var result = entidades.Select(e => new
            {
                value = e.IdEntidadAcademica,
                text = e.Nombre
            });

            return Json(result);
        }

        //POST: Usuarios/CrearUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.Rol?.ToLower() == "coordinador de entidad académica")
                {
                    if (!model.IdAreaAcademica.HasValue)
                        ModelState.AddModelError("IdAreaAcademica", "Seleccione un área");

                    if (string.IsNullOrEmpty(model.Region))
                        ModelState.AddModelError("Region", "Seleccione una región");

                    if (!model.IdEntidadAcademica.HasValue)
                        ModelState.AddModelError("IdEntidadAcademica", "Seleccione una entidad");
                }

                if (model.Rol?.ToLower() == "coordinador de área académica")
                {
                    if (!model.IdAreaAcademica.HasValue)
                        ModelState.AddModelError("IdAreaAcademica", "Seleccione un área");
                }
                await CargarCombos(model);
                return View(model);


            }

            var dto = new CrearUsuarioDTO
            {
                Nombre = model.Nombre,
                Cargo = model.Cargo,
                Correo = model.Correo,
                Rol = model.Rol,
                IdAreaAcademica = model.IdAreaAcademica,
                IdEntidadAcademica = model.IdEntidadAcademica
            };

            if (model.Rol.ToLower() == Constantes.CoordinadorEa.ToLower())
                dto.IdAreaAcademica = null;


            try
            {
                await _usuarioService.CrearAsync(dto);

                TempData["Success"] = "Usuario creado correctamente";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                //ModelState.AddModelError("Correo", ex.Message);
                TempData["Error"] = ex.Message;
                await CargarCombos(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                TempData["Error"] = ex.Message;
                await CargarCombos(model);
                return View(model);
            }
        }

        private async Task CargarCombos(CrearUsuarioViewModel model)
        {

            model.Roles = Constantes.Roles
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Rol
                }).ToList();


            model.Regiones = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Region
                }).ToList();


            var areas = await _areaAcademicaRepository.ObtenerTodosAsync();

            model.Areas = areas.Select(a => new OptionModel
            {
                Value = a.IdAreaAcademica.ToString(),
                Text = a.Nombre,
                Selected = model.IdAreaAcademica.HasValue &&
                           a.IdAreaAcademica == model.IdAreaAcademica.Value
            }).ToList();


            if (model.IdAreaAcademica.HasValue && !string.IsNullOrEmpty(model.Region))
            {
                var entidades = await _entidadAcademicaRepository
                    .ObtenerPorIdAreaAcademicaYRegionAsync(model.IdAreaAcademica.Value, model.Region);

                model.Entidades = entidades.Select(e => new OptionModel
                {
                    Value = e.IdEntidadAcademica.ToString(),
                    Text = e.Nombre,
                    Selected = model.IdEntidadAcademica.HasValue &&
                               e.IdEntidadAcademica == model.IdEntidadAcademica.Value
                }).ToList();
            }
            else
            {
                model.Entidades = new List<OptionModel>();
            }
        }

        /* * * * * * Editar Usuario * * * * * */

        public async Task<IActionResult> EditarUsuario(int id, string rol)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(new ReferenciaUsuarioDTO { IdUsuario = id, Rol = rol });

            var model = new CrearUsuarioViewModel
            {
                IdUsuario = id,
                Nombre = usuario.Nombre,
                Cargo = usuario.Cargo,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                IdAreaAcademica = usuario.IdAreaAcademica,
                IdEntidadAcademica = usuario.IdEntidadAcademica,
                Region = usuario.Region
            };

            await CargarCombos(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.Rol?.ToLower() == "coordinador de entidad académica")
                {
                    if (!model.IdAreaAcademica.HasValue)
                        ModelState.AddModelError("IdAreaAcademica", "Seleccione un área");

                    if (string.IsNullOrEmpty(model.Region))
                        ModelState.AddModelError("Region", "Seleccione una región");

                    if (!model.IdEntidadAcademica.HasValue)
                        ModelState.AddModelError("IdEntidadAcademica", "Seleccione una entidad");
                }

                if (model.Rol?.ToLower() == "coordinador de área académica")
                {
                    if (!model.IdAreaAcademica.HasValue)
                        ModelState.AddModelError("IdAreaAcademica", "Seleccione un área");
                }
                await CargarCombos(model);
                return View(model);


            }

            var dto = new EditarUsuarioDTO
            {
                IdUsuario = model.IdUsuario,
                Nombre = model.Nombre,
                Cargo = model.Cargo,
                Rol = model.Rol,
                IdAreaAcademica = model.IdAreaAcademica,
                IdEntidadAcademica = model.IdEntidadAcademica
            };

            if (model.Rol.ToLower() == Constantes.CoordinadorEa.ToLower())
                dto.IdAreaAcademica = null;

            try
            {
                await _usuarioService.EditarAsync(dto);
                TempData["Success"] = "Usuario actualizado correctamente";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                await CargarCombos(model);
                return View(model);
            }
        }

        /* * * * * * Eliminar Usuario * * * * * */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id, string rol)
        {
            try
            {
                var referencia = new ReferenciaUsuarioDTO
                {
                    IdUsuario = id,
                    Rol = rol
                };
               
                await _usuarioService.EliminarAsync(referencia);
                TempData["Success"] = "Usuario eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado {Id}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                TempData["Error"] = "Error al eliminar el usuario";
                return RedirectToAction(nameof(Index));
            }

        }


    }
}