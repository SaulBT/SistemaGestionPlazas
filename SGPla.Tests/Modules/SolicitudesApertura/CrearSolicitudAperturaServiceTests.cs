using Microsoft.Extensions.Logging;
using Moq;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Contracts;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Models;
using SGPla.Modules.SolicitudesApertura.Application.CrearSolicitudApertura.Ports;
using SGPla.Modules.SolicitudesApertura.Domain;

namespace SGPla.Tests.Modules.SolicitudesApertura;

public sealed class CrearSolicitudAperturaServiceTests
{
    [Fact]
    public async Task CrearAsync_CreaSolicitudSinJustificacion()
    {
        var repository = CrearRepositoryMock();
        var storage = CrearStorageMock();
        var service = CrearService(repository, storage);
        var solicitudCreada = default(SolicitudAperturaParaCrear);

        repository
            .Setup(repositorio => repositorio.CrearAsync(
                It.IsAny<SolicitudAperturaParaCrear>(),
                It.IsAny<ArchivoOficioGuardado>(),
                It.IsAny<CancellationToken>()))
            .Callback<SolicitudAperturaParaCrear, ArchivoOficioGuardado, CancellationToken>(
                (solicitud, _, _) => solicitudCreada = solicitud)
            .Returns(Task.CompletedTask);

        var resultado = await service.CrearAsync(CrearCommand(), CancellationToken.None);

        Assert.Equal(TipoResultadoCrearSolicitudApertura.Exito, resultado.Tipo);
        Assert.NotNull(resultado.Respuesta);
        Assert.Equal("Desarrollo de Software", resultado.Respuesta.NombreExperienciaEducativa);
        Assert.Equal("Matutino", resultado.Respuesta.Seccion);
        Assert.Equal("Presencial", resultado.Respuesta.Modalidad);
        Assert.NotNull(solicitudCreada);
        Assert.Null(solicitudCreada.Justificacion);
        Assert.Equal(SolicitudAperturaConstantes.ESTADO_PENDIENTE, solicitudCreada.Estado);
        storage.Verify(almacenamiento => almacenamiento.GuardarAsync(
            It.IsAny<ArchivoOficioParaGuardar>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_RechazaCantidadMenorAlMinimo()
    {
        var repository = CrearRepositoryMock();
        var storage = CrearStorageMock();
        var service = CrearService(repository, storage);
        var command = CrearCommand() with { CantidadSolicitantes = 4 };

        var resultado = await service.CrearAsync(command, CancellationToken.None);

        Assert.Equal(TipoResultadoCrearSolicitudApertura.Validacion, resultado.Tipo);
        Assert.Equal("CantidadSolicitantes", resultado.Campo);
        storage.Verify(almacenamiento => almacenamiento.GuardarAsync(
            It.IsAny<ArchivoOficioParaGuardar>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_RechazaExperienciaFueraDeLaEntidadDelCoordinador()
    {
        var repository = CrearRepositoryMock();
        repository
            .Setup(repositorio => repositorio.ObtenerExperienciaEducativaAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExperienciaEducativaContexto(
                1,
                "Desarrollo de Software",
                2,
                3,
                99,
                5,
                30));

        var storage = CrearStorageMock();
        var service = CrearService(repository, storage);

        var resultado = await service.CrearAsync(CrearCommand(), CancellationToken.None);

        Assert.Equal(TipoResultadoCrearSolicitudApertura.Prohibido, resultado.Tipo);
        storage.Verify(almacenamiento => almacenamiento.GuardarAsync(
            It.IsAny<ArchivoOficioParaGuardar>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_RechazaSolicitudDuplicada()
    {
        var repository = CrearRepositoryMock();
        repository
            .Setup(repositorio => repositorio.ExisteSolicitudActivaAsync(
                It.IsAny<SolicitudAperturaDuplicidadContexto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var storage = CrearStorageMock();
        var service = CrearService(repository, storage);

        var resultado = await service.CrearAsync(CrearCommand(), CancellationToken.None);

        Assert.Equal(TipoResultadoCrearSolicitudApertura.Conflicto, resultado.Tipo);
        storage.Verify(almacenamiento => almacenamiento.GuardarAsync(
            It.IsAny<ArchivoOficioParaGuardar>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearAsync_RechazaArchivoQueNoEsPdf()
    {
        var repository = CrearRepositoryMock();
        var storage = CrearStorageMock();
        var service = CrearService(repository, storage);
        var command = CrearCommand() with
        {
            ContenidoArchivo = "texto no pdf"u8.ToArray()
        };

        var resultado = await service.CrearAsync(command, CancellationToken.None);

        Assert.Equal(TipoResultadoCrearSolicitudApertura.Validacion, resultado.Tipo);
        Assert.Equal("ArchivoOficio", resultado.Campo);
        storage.Verify(almacenamiento => almacenamiento.GuardarAsync(
            It.IsAny<ArchivoOficioParaGuardar>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CrearSolicitudAperturaService CrearService(
        Mock<ISolicitudAperturaRepository> repository,
        Mock<IArchivoOficioStorage> storage)
    {
        var currentUserContext = new Mock<ICurrentUserContext>();
        currentUserContext.SetupGet(contexto => contexto.EstaAutenticado).Returns(true);
        currentUserContext.SetupGet(contexto => contexto.EsCoordinadorEa).Returns(true);
        currentUserContext.SetupGet(contexto => contexto.Correo)
            .Returns("coordinador@uv.mx");

        var fechaActualProvider = new Mock<IFechaActualProvider>();
        fechaActualProvider.Setup(provider => provider.ObtenerFechaActual())
            .Returns(new DateOnly(2026, 1, 15));

        storage
            .Setup(almacenamiento => almacenamiento.GuardarAsync(
                It.IsAny<ArchivoOficioParaGuardar>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArchivoOficioGuardado(
                "oficio.pdf",
                "solicitudes-apertura/archivo.pdf",
                SolicitudAperturaConstantes.TIPO_ARCHIVO_OFICIO,
                20));

        return new CrearSolicitudAperturaService(
            currentUserContext.Object,
            fechaActualProvider.Object,
            repository.Object,
            storage.Object,
            Mock.Of<ILogger<CrearSolicitudAperturaService>>());
    }

    private static Mock<ISolicitudAperturaRepository> CrearRepositoryMock()
    {
        var repository = new Mock<ISolicitudAperturaRepository>();
        repository
            .Setup(repositorio => repositorio.ObtenerCoordinadorEaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CoordinadorEaContexto(10));
        repository
            .Setup(repositorio => repositorio.ObtenerExperienciaEducativaAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExperienciaEducativaContexto(
                1,
                "Desarrollo de Software",
                2,
                3,
                10,
                5,
                30));
        repository
            .Setup(repositorio => repositorio.ObtenerModalidadAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModalidadContexto(4, "Presencial"));
        repository
            .Setup(repositorio => repositorio.ObtenerPeriodoSiguienteAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PeriodoContexto(
                5,
                "202601",
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 7, 31)));
        repository
            .Setup(repositorio => repositorio.ExisteSolicitudActivaAsync(
                It.IsAny<SolicitudAperturaDuplicidadContexto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        return repository;
    }

    private static Mock<IArchivoOficioStorage> CrearStorageMock()
    {
        return new Mock<IArchivoOficioStorage>();
    }

    private static CrearSolicitudAperturaCommand CrearCommand()
    {
        return new CrearSolicitudAperturaCommand(
            1,
            " Matutino ",
            4,
            20,
            null,
            "oficio.pdf",
            20,
            "%PDF-1.7\ncontenido"u8.ToArray());
    }
}
