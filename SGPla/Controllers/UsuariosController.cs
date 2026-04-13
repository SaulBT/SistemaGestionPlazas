using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGPla.Commons;
using SGPla.Models.DTOs.Usuarios;
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
        public async Task<IActionResult> Index(string rol = null, string busqueda = null)
        {
            try
            {
                var filtro = new FiltrosUsuarioDTO
                {
                    Rol = rol
                };

                var usuarios = await _usuarioService.ObtenerPorFiltroAsync(filtro);

                ViewBag.RolSeleccionado = rol;
                ViewBag.Busqueda = busqueda;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de usuarios");
                TempData["Error"] = "Error al cargar los usuarios";
                return View(new List<ListaUsuarioDTO>());
            }
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int id, string rol)
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