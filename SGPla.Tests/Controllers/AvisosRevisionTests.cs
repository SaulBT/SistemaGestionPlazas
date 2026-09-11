using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using SGPla.Commons;
using SGPla.Controllers;
using SGPla.Models;
using SGPla.Models.DTOs.Aviso;
using SGPla.Models.DTOs.Archivo;
using SGPla.Models.DTOs.EntidadAcademica;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Controllers;

public class AvisosRevisionTests
{
    private readonly Mock<IArchivoService> archivos = new();
    private readonly Mock<IAvisoService> servicio = new();
    private readonly Mock<IPeriodoEscolarService> periodos = new();
    private readonly Mock<IEntidadAcademicaService> entidades = new();
    private readonly Mock<IAvisoRepository> avisos = new();
    private readonly Mock<ICoordinadorDgaaRepository> coordinadores = new();

    private AvisosController Crear(string rol)
    {
        var controller = new AvisosController(servicio.Object, periodos.Object,
            Mock.Of<IArticuloService>(), entidades.Object, archivos.Object,
            Mock.Of<IPlantillaService>(), Mock.Of<ILogger<AvisosController>>(), avisos.Object, coordinadores.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Role, rol), new Claim("EntidadAcademicaId", "7"),
            new Claim(ClaimTypes.Email, "coordinador@uv.mx") }, "test"));
        coordinadores.Setup(r => r.ObtenerPorCorreoAsync("coordinador@uv.mx"))
            .ReturnsAsync(new CoordinadorDgaa { IdAreaAcademica = 3 });
        return controller;
    }

    [Theory]
    [InlineData(Constantes.COORDINADOR_EA)]
    [InlineData(Constantes.COORDINADOR_DGAA)]
    public async Task Listado_UsaAlcanceDelUsuarioYFiltrosDeFecha(string rol)
    {
        var controller = Crear(rol);
        periodos.Setup(p => p.ObtenerTodosAsync()).ReturnsAsync([]);
        entidades.Setup(e => e.ObtenerCatalogoAsync(It.IsAny<FiltroEntidadAcademicaDTO>())).ReturnsAsync([]);
        servicio.Setup(s => s.ObtenerTodosAvisosAsync(It.IsAny<FiltroAvisosDTO>()))
            .ReturnsAsync((new List<ListaAvisosDTO>(), 0));
        var inicio = new DateOnly(2026, 9, 1);
        var fin = new DateOnly(2026, 9, 8);
        Assert.IsType<ViewResult>(await controller.Index(null, null, 99, inicio, fin));
        servicio.Verify(s => s.ObtenerTodosAvisosAsync(It.Is<FiltroAvisosDTO>(f =>
            f.FechaInicio == inicio && f.FechaFin == fin &&
            (rol == Constantes.COORDINADOR_EA
                ? f.IdEntidadAcademica == 7 && !f.SoloEnviadosDgaa && f.IdAreaAcademica == null
                : f.IdAreaAcademica == 3 && f.SoloEnviadosDgaa))), Times.Once);
    }

    private void Aviso(string estado, int entidad = 7, int area = 3, bool archivado = false)
        => avisos.Setup(r => r.ObtenerPorIDAsync(15)).ReturnsAsync(new Aviso {
            IdAviso = 15, IdEntidadAcademica = entidad, Estado = estado, Archivado = archivado,
            IdEntidadAcademicaNavigation = new EntidadAcademica { IdAreaAcademica = area } });

    [Theory]
    [InlineData(Constantes.CREADO)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA)]
    public async Task Entidad_PuedeEnviarCreadoODevuelto(string estado)
    {
        Aviso(estado);
        Assert.IsType<ViewResult>(await Crear(Constantes.COORDINADOR_EA).EnviarARevisionAsync(15));
    }

    [Fact]
    public async Task Entidad_NoPuedeEnviarAvisoAjeno()
    {
        Aviso(Constantes.CREADO, entidad: 8);
        Assert.IsType<ForbidResult>(await Crear(Constantes.COORDINADOR_EA).EnviarARevisionAsync(15));
    }

    [Theory]
    [InlineData(Constantes.EN_REVISION_POR_DGAA, false)]
    [InlineData(Constantes.AVALADO_POR_DGAA, false)]
    [InlineData(Constantes.CREADO, true)]
    public async Task Entidad_NoPuedeEnviarEstadoInvalido(string estado, bool archivado)
    {
        Aviso(estado, archivado: archivado);
        Assert.IsType<ConflictObjectResult>(await Crear(Constantes.COORDINADOR_EA).EnviarARevisionAsync(15));
    }

    [Theory]
    [InlineData(Constantes.CREADO, 3, false)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA, 3, false)]
    [InlineData(Constantes.EN_REVISION_POR_DGAA, 4, false)]
    [InlineData(Constantes.EN_REVISION_POR_DGAA, 3, true)]
    public async Task Dgaa_NoPuedeRevisarBorradoresAvisosAjenosNiArchivados(string estado, int area, bool archivado)
    {
        Aviso(estado, area: area, archivado: archivado);
        Assert.IsType<ForbidResult>(await Crear(Constantes.COORDINADOR_DGAA).RevisarAvisoAsync(15));
    }

    [Fact]
    public async Task Dgaa_PuedeRevisarPendienteDeSuArea()
    {
        Aviso(Constantes.EN_REVISION_POR_DGAA);
        Assert.IsType<ViewResult>(await Crear(Constantes.COORDINADOR_DGAA).RevisarAvisoAsync(15));
    }

    [Theory]
    [InlineData(Constantes.AVALADO_POR_DGAA)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA)]
    [InlineData(Constantes.FIRMADO)]
    [InlineData(Constantes.PUBLICADO)]
    [InlineData(Constantes.ACTA_DE_CT_CREADA)]
    public async Task Dgaa_ConservaAccesoAlDocumentoTrasRevision(string estado)
    {
        Aviso(estado);
        servicio.Setup(s => s.ObtenerAvisoPorIDAsync(15)).ReturnsAsync(new DatosAvisoDTO
            { IdAviso = 15, IdEntidadAcademica = 7, IdArchivoOriginal = 4 });
        var controller = Crear(Constantes.COORDINADOR_DGAA);
        controller.Url = Mock.Of<IUrlHelper>();
        Assert.IsType<ViewResult>(await controller.VistaPreviaAvisoAsync(15));
    }

    [Theory]
    [InlineData(Constantes.AVALADO_POR_DGAA)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA)]
    [InlineData(Constantes.FIRMADO)]
    [InlineData(Constantes.PUBLICADO)]
    [InlineData(Constantes.ACTA_DE_CT_CREADA)]
    public async Task Dgaa_EditaComentariosSinCambiarDecision(string estado)
    {
        Aviso(estado);
        var controller = Crear(Constantes.COORDINADOR_DGAA);
        var vista = Assert.IsType<ViewResult>(await controller.EditarRevisionAsync(15));
        Assert.Equal(true, vista.ViewData["EditarComentarios"]);
        Assert.IsType<RedirectToActionResult>(await controller.GuardarComentariosRevisionAsync(15, "Comentarios corregidos"));
        servicio.Verify(s => s.EditarComentariosRevisionAsync(15, "Comentarios corregidos"), Times.Once);
        servicio.Verify(s => s.RevisarAvisoAsync(It.IsAny<RevisionDTO>()), Times.Never);
        servicio.Verify(s => s.EnviarARevisionAsync(It.IsAny<RevisionDTO>()), Times.Never);
    }

    [Theory]
    [InlineData(Constantes.CREADO, 3)]
    [InlineData(Constantes.EN_REVISION_POR_DGAA, 3)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA, 4)]
    public async Task Dgaa_NoEditaComentariosSinRevisionODeOtraArea(string estado, int area)
    {
        Aviso(estado, area: area);
        var controller = Crear(Constantes.COORDINADOR_DGAA);
        Assert.IsType<ForbidResult>(await controller.EditarRevisionAsync(15));
        Assert.IsType<ForbidResult>(await controller.GuardarComentariosRevisionAsync(15, "Intento"));
        servicio.Verify(s => s.EditarComentariosRevisionAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Dgaa_ErrorAlEditarConservaTextoYModoEdicion()
    {
        Aviso(Constantes.DEVUELTO_POR_DGAA);
        servicio.Setup(s => s.EditarComentariosRevisionAsync(15, "Corrección"))
            .ThrowsAsync(new ValidacionExcepction("El aviso fue reenviado", "409"));
        var vista = Assert.IsType<ViewResult>(await Crear(Constantes.COORDINADOR_DGAA)
            .GuardarComentariosRevisionAsync(15, "Corrección"));
        Assert.Equal(true, vista.ViewData["EditarComentarios"]);
        Assert.Equal("Corrección", vista.ViewData["ComentariosIngresados"]);
        Assert.False(vista.ViewData.ModelState.IsValid);
    }

    [Theory]
    [InlineData(nameof(AvisosController.CrearAviso), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(AvisosController.EditarAviso), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(AvisosController.EnviarARevisionAsync), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(AvisosController.ConfirmarEnviarARevisionAsync), PoliticasAutorizacion.EntidadAcademica)]
    [InlineData(nameof(AvisosController.EditarRevisionAsync), PoliticasAutorizacion.Dgaa)]
    [InlineData(nameof(AvisosController.GuardarComentariosRevisionAsync), PoliticasAutorizacion.Dgaa)]
    public void Acciones_ExigenRolCorrespondiente(string accion, string politica)
    {
        var metodos = typeof(AvisosController).GetMethods().Where(m => m.Name == accion);
        Assert.NotEmpty(metodos);
        Assert.All(metodos, metodo => Assert.Contains(metodo.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>(), a => a.Policy == politica));
    }

    [Theory]
    [InlineData(nameof(AvisosController.FirmarAvisoAsync))]
    [InlineData(nameof(AvisosController.PublicarAvisoAsync))]
    public void FirmaYPublicacion_SoloCeaYConAntifalsificacion(string accion)
    {
        var metodo = typeof(AvisosController).GetMethod(accion)!;
        Assert.Contains(metodo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>(),
            a => a.Policy == PoliticasAutorizacion.EntidadAcademica);
        Assert.NotEmpty(metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
    }

    private static FormFile Pdf(string contenido = "%PDF-1.7 ejemplo", string nombre = "firmado.pdf")
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(contenido);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "archivo", nombre);
    }

    [Fact]
    public async Task Cea_FirmaAvaladoYDevuelveConfirmacion()
    {
        Aviso(Constantes.AVALADO_POR_DGAA);
        archivos.Setup(a => a.GuardarTemporalmenteAsync(It.IsAny<IFormFile>())).ReturnsAsync(("firmado.pdf", ""));
        Assert.IsType<OkObjectResult>(await Crear(Constantes.COORDINADOR_EA).FirmarAvisoAsync(15, Pdf()));
        servicio.Verify(s => s.FirmarAvisoAsync(15, It.IsAny<CargarArchivoDTO>()), Times.Once);
    }

    [Theory]
    [InlineData(Constantes.CREADO)]
    [InlineData(Constantes.EN_REVISION_POR_DGAA)]
    [InlineData(Constantes.DEVUELTO_POR_DGAA)]
    [InlineData(Constantes.FIRMADO)]
    public async Task Cea_NoFirmaSinAval(string estado)
    {
        Aviso(estado);
        Assert.IsType<ConflictObjectResult>(await Crear(Constantes.COORDINADOR_EA).FirmarAvisoAsync(15, Pdf()));
        archivos.Verify(a => a.GuardarTemporalmenteAsync(It.IsAny<IFormFile>()), Times.Never);
    }

    [Theory]
    [InlineData("texto", "firmado.pdf")]
    [InlineData("%PDF-1.7", "archivo.txt")]
    [InlineData("", "firmado.pdf")]
    public async Task Cea_FirmaRechazaArchivoInvalido(string contenido, string nombre)
    {
        Aviso(Constantes.AVALADO_POR_DGAA);
        Assert.IsType<BadRequestObjectResult>(await Crear(Constantes.COORDINADOR_EA).FirmarAvisoAsync(15, Pdf(contenido, nombre)));
        servicio.Verify(s => s.FirmarAvisoAsync(It.IsAny<int>(), It.IsAny<CargarArchivoDTO>()), Times.Never);
    }

    [Fact]
    public async Task Cea_NoFirmaNiPublicaAvisosDeOtraEntidad()
    {
        Aviso(Constantes.AVALADO_POR_DGAA, entidad: 8);
        var controller = Crear(Constantes.COORDINADOR_EA);
        Assert.IsType<ForbidResult>(await controller.FirmarAvisoAsync(15, Pdf()));
        Assert.IsType<ForbidResult>(await controller.PublicarAvisoAsync(15, "https://uv.mx/aviso"));
    }

    [Fact]
    public async Task Cea_PublicaSoloConArchivoFirmado()
    {
        Aviso(Constantes.FIRMADO);
        var controller = Crear(Constantes.COORDINADOR_EA);
        Assert.IsType<ConflictObjectResult>(await controller.PublicarAvisoAsync(15, "https://uv.mx/aviso"));
        var aviso = await avisos.Object.ObtenerPorIDAsync(15);
        aviso!.IdArchivoFirmado = 20;
        Assert.IsType<OkObjectResult>(await controller.PublicarAvisoAsync(15, "https://uv.mx/aviso"));
        servicio.Verify(s => s.PublicarAvisoAsync(15, "https://uv.mx/aviso"), Times.Once);
    }

    [Fact]
    public async Task Firma_ErrorDevuelveRespuestaFallida()
    {
        Aviso(Constantes.AVALADO_POR_DGAA);
        archivos.Setup(a => a.GuardarTemporalmenteAsync(It.IsAny<IFormFile>())).ThrowsAsync(new IOException());
        var resultado = Assert.IsType<ObjectResult>(await Crear(Constantes.COORDINADOR_EA).FirmarAvisoAsync(15, Pdf()));
        Assert.Equal(500, resultado.StatusCode);
    }

    [Fact]
    public async Task Descargar_UsaArchivoFirmadoCuandoExiste()
    {
        Aviso(Constantes.FIRMADO);
        servicio.Setup(s => s.ObtenerAvisoPorIDAsync(15)).ReturnsAsync(new DatosAvisoDTO
            { IdArchivoOriginal = 4, IdArchivoFirmado = 20 });
        archivos.Setup(a => a.DescargarAsync(20)).ReturnsAsync(new ArchivoDescargadoDTO
            { Ruta = "C:/archivos/firmado.pdf", Tipo = "application/pdf", Nombre = "firmado.pdf" });
        Assert.IsType<PhysicalFileResult>(await Crear(Constantes.COORDINADOR_EA).DescargarAvisoAsync(15));
        archivos.Verify(a => a.DescargarAsync(20), Times.Once);
    }
}
