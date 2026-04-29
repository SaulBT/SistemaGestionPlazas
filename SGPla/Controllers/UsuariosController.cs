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

        public UsuariosController(
            IUsuarioService usuarioService,
            ILogger<UsuariosController> logger,
            IAreaAcademicaRepository areaAcademicaRepository,
            IEntidadAcademicaRepository entidadAcademicaRepository)
        {
            _usuarioService = usuarioService;
            _logger = logger;
            _areaAcademicaRepository = areaAcademicaRepository;
            _entidadAcademicaRepository = entidadAcademicaRepository;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica)
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

            return View(new IndexViewModel
            {
                Table = await LlenarTabla(busqueda, region, idAreaAcademica, idEntidadAcademica),
                Regiones = regionesCombo,
                Areas = areasCombo,
                Entidades = entidadesCombo,

                RegionSeleccionada = region,
                IdAreaSeleccionada = idAreaAcademica,
                IdEntidadSeleccionada = idEntidadAcademica,
                Busqueda = busqueda
            });
        }

        private async Task<TableModel> LlenarTabla( string? busqueda, string? region, int? idAreaAcademica, int? idEntidadAcademica)
        {
            try
            {
                FiltrosUsuarioDTO filtros = new FiltrosUsuarioDTO
                {
                    Busqueda = busqueda,
                    Region = region,
                    IdAreaAcademica = idAreaAcademica,
                    IdEntidadAcademica = idEntidadAcademica
                };
                var usuarios = await _usuarioService.BuscarConFiltrosAsync(filtros);

                return new TableModel
                {
                    Headers = new List<string>
                        {
                            "Nombre", "Correo", "Cargo", "Rol", "Entidad/Área", "Región", "Acciones"
                        },
                    Rows = usuarios.Select(usuario => new TableRowModel
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
                                Url = Url.Action("Delete", "Usuarios", new { id = usuario.IdUsuario, rol = usuario.Rol })
                            }
                        }
                    }
                }
                    }).ToList()
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

            try
            {
                await _usuarioService.CrearAsync(dto);

                TempData["Success"] = "Usuario creado correctamente";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Correo", ex.Message);
                await CargarCombos(model);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError("", "Error al crear el usuario");
                await CargarCombos(model);
                return View(model);
            }
        }

        private async Task CargarCombos(CrearUsuarioViewModel model)
        {
            // 🔹 Roles
            model.Roles = Constantes.Roles
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Rol
                }).ToList();

            // 🔹 Regiones
            model.Regiones = Constantes.Regiones
                .Select(r => new OptionModel
                {
                    Value = r,
                    Text = r,
                    Selected = r == model.Region
                }).ToList();

            // 🔹 Áreas
            var areas = await _areaAcademicaRepository.ObtenerTodosAsync();

            model.Areas = areas.Select(a => new OptionModel
            {
                Value = a.IdAreaAcademica.ToString(),
                Text = a.Nombre,
                Selected = model.IdAreaAcademica.HasValue &&
                           a.IdAreaAcademica == model.IdAreaAcademica.Value
            }).ToList();

            // 🔹 Entidades (IMPORTANTE)
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
            var usuario = await _usuarioService.ObtenerPorIdAsync(new ReferenciaUsuarioDTO { IdUsuario = id, Rol = rol});

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
                ModelState.AddModelError("Rol", ex.Message);
                await CargarCombos(model);
                return View(model);
            }
        }


















        // GET: Usuarios/Create - CORREGIDO: ahora es async y usa await
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = GetRolesList();
            ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
            ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await

            return View(new CrearUsuarioDTO());
        }

        

        // POST: Usuarios/Create - CORREGIDO: usa await en las listas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearUsuarioDTO model)
        {
            if (model.Rol == "EA")
            {
                model.Rol = Constantes.CoordinadorEa;
            }
            else if (model.Rol == "DGAA")
            {
                model.Rol = Constantes.CoordinadorDgaa;
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = GetRolesList();
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }

            try
            {
                var idUsuario = await _usuarioService.CrearAsync(model);
                TempData["Success"] = $"Usuario creado exitosamente con ID: {idUsuario}";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Roles = GetRolesList();
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError("", "Error al crear el usuario");
                ViewBag.Roles = GetRolesList();
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }
        }

        // GET: Usuarios/Edit/5 - CORREGIDO: usa await en las listas
        public async Task<IActionResult> Edit(int id, string rol)
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

                var model = new EditarUsuarioDTO
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    //Correo = usuario.Correo, // Descomentado: debe incluir el correo
                    Cargo = usuario.Cargo,
                    Rol = usuario.Rol,
                    IdAreaAcademica = usuario.IdAreaAcademica,
                    IdEntidadAcademica = usuario.IdEntidadAcademica
                };

                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario para editar {Id}", id);
                TempData["Error"] = "Error al cargar el usuario para editar";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: Usuarios/Edit/5 - CORREGIDO: usa await en las listas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditarUsuarioDTO model)
        {
            if (id != model.IdUsuario)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }

            try
            {
                await _usuarioService.EditarAsync(model);
                TempData["Success"] = "Usuario actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
                ModelState.AddModelError("", "Error al actualizar el usuario");
                ViewBag.AreasAcademicas = await GetAreasAcademicasList(); // Añadido await
                ViewBag.EntidadesAcademicas = await GetEntidadesAcademicasList(); // Añadido await
                return View(model);
            }
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int id, string rol)
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
                _logger.LogError(ex, "Error al cargar usuario para eliminar {Id}", id);
                TempData["Error"] = "Error al cargar el usuario para eliminar";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string rol)
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

        // POST: Usuarios/Filtrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Filtrar(FiltrosUsuarioDTO filtro)
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerPorFiltroAsync(filtro);
                return PartialView("_ListaUsuariosPartial", usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar usuarios");
                return StatusCode(500, "Error al aplicar el filtro");
            }
        }

        // Métodos auxiliares
        private SelectList GetRolesList()
        {
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = Constantes.CoordinadorEa, Text = "Coordinador de Entidad Académica" },
                new SelectListItem { Value = Constantes.CoordinadorDgaa, Text = "Coordinador DGAA" }
            };
            return new SelectList(roles, "Value", "Text");
        }

        private async Task<SelectList> GetAreasAcademicasList()
        {
            var areas = await _areaAcademicaRepository.ObtenerTodosAsync();
            return new SelectList(areas, "IdAreaAcademica", "Nombre");
        }

        private async Task<SelectList> GetEntidadesAcademicasList()
        {
            var entidades = await _entidadAcademicaRepository.ObtenerTodosAsync();
            return new SelectList(entidades, "IdEntidadAcademica", "Nombre"); // Corregido: "Nombre" en lugar de "nombre"
        }
    }
}