using Moq;
using SGPla.Models.DTOs.Solicitudes;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public sealed class SolicitudMvcServiceTests
{
    [Fact]
    public async Task AgregarDocumento_AnteCommitAmbiguoConservaArchivoReferenciadoEnSql()
    {
        const string clave = "aspirantes/0123456789abcdef0123456789abcdef.pdf";
        var almacen = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        almacen.Setup(x => x.GuardarAspiranteAsync(It.IsAny<Stream>(), "cv.pdf", "application/pdf", 8,
            It.IsAny<CancellationToken>())).ReturnsAsync(new DocumentoAlmacenado("cv.pdf", "application/pdf", 8,
            new byte[32], clave));
        var repositorio = new Mock<ISolicitudMvcRepository>(MockBehavior.Strict);
        repositorio.Setup(x => x.AgregarDocumentoAsync(7, 12, 33, 55,
            It.Is<DocumentoAspiranteMvcDatos>(d => d.TipoDocumentoId == 4 && d.ClaveRelativa == clave),
            It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("Conexión perdida al confirmar."));
        repositorio.Setup(x => x.ExisteClaveDocumentoAsync(clave, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new SolicitudMvcService(repositorio.Object, almacen.Object);

        var error = await Assert.ThrowsAsync<AggregateException>(() => service.AgregarDocumentoAsync(7, 12, 33, 55,
            4, null, Stream.Null, "cv.pdf", "application/pdf", 8));

        Assert.Contains("resultado fue ambiguo", error.Message, StringComparison.OrdinalIgnoreCase);
        almacen.Verify(x => x.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Registrar_NormalizaDatosYDelegaConPerfilYFormacion()
    {
        var repository = new Mock<ISolicitudMvcRepository>();
        repository.Setup(x => x.RegistrarAsync(7, 12, It.IsAny<RegistrarSolicitudMvcDatos>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(92);
        var service = new SolicitudMvcService(repository.Object);

        var id = await service.RegistrarAsync(7, 12, new RegistrarSolicitudMvcDatos(33,
            "  Ada Ejemplo  ", " Ada@Example.org ", "  Profesora  ", "  Perfil  ",
            [new FormacionSolicitudMvcDatos(4, "  Doctorado, Universidad  ")], "  Solicito participar  "));

        Assert.Equal(92, id);
        repository.Verify(x => x.RegistrarAsync(7, 12,
            It.Is<RegistrarSolicitudMvcDatos>(d => d.Nombre == "Ada Ejemplo"
                && d.Correo == "ada@example.org" && d.PuestoActual == "Profesora"
                && d.DescripcionPerfil == "Perfil" && d.Observaciones == "Solicito participar"
                && d.Formaciones.Single().Descripcion == "Doctorado, Universidad"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("ada@example.org extra")]
    public async Task Registrar_RechazaCorreoNoValidoAntesDePersistir(string correo)
    {
        var repository = new Mock<ISolicitudMvcRepository>();
        var service = new SolicitudMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RegistrarAsync(7, 12,
            new RegistrarSolicitudMvcDatos(33, "Ada", correo, null, "Perfil",
                [new FormacionSolicitudMvcDatos(4, "Doctorado")], null)));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Registrar_ExigeAlMenosUnaFormacion()
    {
        var repository = new Mock<ISolicitudMvcRepository>();
        var service = new SolicitudMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RegistrarAsync(7, 12,
            new RegistrarSolicitudMvcDatos(33, "Ada", "ada@example.org", null, "Perfil", [], null)));

        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task Resolver_RechazoExigeMotivoAntesDePersistir(string? motivo)
    {
        var repository = new Mock<ISolicitudMvcRepository>();
        var service = new SolicitudMvcService(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ResolverAsync(7, 12, 33, 55,
            new ResolverSolicitudMvcDatos(false, motivo)));

        repository.VerifyNoOtherCalls();
    }
}
