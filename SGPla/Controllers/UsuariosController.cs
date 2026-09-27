using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGPla.Commons;
using SGPla.Commons.Factories;
using SGPla.Models;
using SGPla.Models.Components;
using SGPla.Models.DTOs.Usuarios;
using SGPla.Models.ViewModels;
using SGPla.Models.ViewModels.Usuarios;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;
using System.Numerics;

namespace SGPla.Controllers
{
    [Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IAreaAcademicaService _areaAcademicaService;
        private readonly ICatalogosMvcService _catalogosMvcService;
        private int paginaActual = 1;

        private static List<string> HEADERS_TABLA_INDEX = ["Nombre", "Correo", "Cargo", "Rol", "Entidad/Área", "Región", "Acciones"];

        public UsuariosController(
            IUsuarioService usuarioService,
            ILogger<UsuariosController> logger,
            IAreaAcademicaService areaAcademicaService,
            ICatalogosMvcService catalogosMvcService)
        {
            _usuarioService = usuarioService;
            _logger = logger;
            _areaAcademicaService = areaAcademicaService;
            _catalogosMvcService = catalogosMvcService;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? busqueda, int? regionId, int? idAreaAcademica, int? idEntidadAcademica, int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default)
        {
            pagina = Math.Max(1, pagina);
            cantidad = Math.Clamp(cantidad, 1, 100);
            paginaActual = pagina;
            // combos
            var regionesCombo = await ObtenerRegionesFiltroComboAsync(regionId, cancellationToken);

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
                entidades = await ObtenerEntidadesNormalizadasAsync(idAreaAcademica, regionId, cancellationToken);
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
                Table = await LlenarTabla(busqueda, regionId, idAreaAcademica, idEntidadAcademica, pagina, cantidad, cancellationToken),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,

                RegionSeleccionadaId = regionId,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                Busqueda = busqueda,

                PaginaActual = paginaActual,
                CantidadPorPagina = cantidad
            });
        }


        private async Task<TableModel> LlenarTabla(string? busqueda, int? regionId, int? idAreaAcademica, int? idEntidadAcademica, int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                FiltrosUsuarioDTO filtros = new FiltrosUsuarioDTO
                {
                    Busqueda = busqueda,
                    RegionId = regionId,
                    IdAreaAcademica = idAreaAcademica,
                    IdEntidadAcademica = idEntidadAcademica,
                    Pagina = pagina,
                    Cantidad = cantidad
                };
                var usuarios = await _usuarioService.BuscarPorFiltroPaginadoAsync(filtros, cancellationToken);
                if (usuarios.Items.Count == 0)
                {
                    paginaActual = 1;
                    filtros.Pagina = paginaActual;
                    usuarios = await _usuarioService.BuscarPorFiltroPaginadoAsync(filtros, cancellationToken);
                }
                if (usuarios.Items.Count == 0)
                    return TablaFactory.GenerarTablaConMensaje(HEADERS_TABLA_INDEX, string.Format(Constantes.TABLA_VACIA, Constantes.USUARIOS));


                return new TableModel
                {
                    Headers = HEADERS_TABLA_INDEX,
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
        public async Task<IActionResult> CrearUsuario(int? regionId, int? idAreaAcademica, int? idEntidadAcademica, string? rol)
        {
            return View(await ObtenerModelo(regionId, idAreaAcademica, idEntidadAcademica, rol));
        }

        private async Task<CrearUsuarioViewModel> ObtenerModelo(int? regionId, int? idAreaAcademica, int? idEntidadAcademica, string? rol)
        {
            var rolesCombo = Constantes.ROLES
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == rol
                })
                .ToList();

            var regionesCombo = await ObtenerRegionesComboAsync(regionId);

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
                entidades = await ObtenerEntidadesNormalizadasAsync(idAreaAcademica, regionId);
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
        public async Task<IActionResult> ObtenerEntidades(int idAreaAcademica, int regionId)
        {
            var entidades = await ObtenerEntidadesNormalizadasAsync(idAreaAcademica, regionId);


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

                    if (!model.RegionId.HasValue)
                        ModelState.AddModelError("RegionId", "Seleccione una región");

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

            if (model.Rol.ToLower() == Constantes.COORDINADOR_EA.ToLower())
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

            model.Roles = Constantes.ROLES
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Rol
                }).ToList();


            model.Regiones = await ObtenerRegionesComboAsync(model.RegionId);


            var areas = await _areaAcademicaService.ObtenerTodasAsync();

            model.Areas = areas.Select(a => new OptionModel
            {
                Value = a.IdAreaAcademica.ToString(),
                Text = a.Nombre,
                Selected = model.IdAreaAcademica.HasValue &&
                           a.IdAreaAcademica == model.IdAreaAcademica.Value
            }).ToList();


            if (model.IdAreaAcademica.HasValue && model.RegionId.HasValue)
            {
                var entidades = await ObtenerEntidadesNormalizadasAsync(model.IdAreaAcademica, model.RegionId);

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

        private async Task<List<OptionModel>> ObtenerRegionesComboAsync(int? seleccionada)
        {
            var catalogos = await _catalogosMvcService.ObtenerAsync();
            return catalogos.Regiones.Select(r => new OptionModel
            {
                Value = r.Id.ToString(),
                Text = $"{r.Clave}-{r.Nombre}",
                Selected = seleccionada == r.Id
            }).ToList();
        }

        private async Task<List<OptionModel>> ObtenerRegionesFiltroComboAsync(int? seleccionada, CancellationToken cancellationToken = default)
        {
            var catalogos = await _catalogosMvcService.ObtenerAsync(cancellationToken);
            return catalogos.Regiones.Select(r => new OptionModel
            {
                Value = r.Id.ToString(),
                Text = $"{r.Clave}-{r.Nombre}",
                Selected = r.Id == seleccionada
            }).ToList();
        }

        private async Task<List<EntidadAcademica>> ObtenerEntidadesNormalizadasAsync(int? idAreaAcademica, int? regionId, CancellationToken cancellationToken = default)
        {
            var entidades = await _catalogosMvcService.ObtenerEntidadesAsync(
                campusId: null,
                areaAcademicaId: idAreaAcademica,
                cancellationToken: cancellationToken,
                regionId: regionId);
            return entidades.Select(x => new EntidadAcademica
            {
                IdEntidadAcademica = x.Id,
                Nombre = x.Nombre,
                IdAreaAcademica = idAreaAcademica ?? 0
            }).ToList();
        }

        /* * * * * * Editar Usuario * * * * * */

        public async Task<IActionResult> EditarUsuario(int id, string rol)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(new ReferenciaUsuarioDTO { IdUsuario = id, Rol = rol });
            var regionId = usuario.IdEntidadAcademica.HasValue
                ? await _catalogosMvcService.ObtenerRegionEntidadAsync(usuario.IdEntidadAcademica.Value)
                : null;

            var model = new CrearUsuarioViewModel
            {
                IdUsuario = id,
                Nombre = usuario.Nombre,
                Cargo = usuario.Cargo,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                IdAreaAcademica = usuario.IdAreaAcademica,
                IdEntidadAcademica = usuario.IdEntidadAcademica,
                RegionId = regionId
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

                    if (!model.RegionId.HasValue)
                        ModelState.AddModelError("RegionId", "Seleccione una región");

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

            if (model.Rol.ToLower() == Constantes.COORDINADOR_EA.ToLower())
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
