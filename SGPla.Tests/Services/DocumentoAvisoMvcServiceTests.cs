using Moq;
using SGPla.Models.DTOs.Avisos;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;

namespace SGPla.Tests.Services;

public sealed class DocumentoAvisoMvcServiceTests
{
    private const string Key = "avisos/0123456789abcdef0123456789abcdef.pdf";
    private static readonly DocumentoAlmacenado Stored = new("aviso.pdf", "application/pdf", 8, new byte[32], Key);

    [Fact]
    public async Task GuardarOriginal_PersisteMetadatosYLimpiaArchivoSiFallaSql()
    {
        var storage = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        storage.Setup(x => x.GuardarOriginalAsync(It.IsAny<Stream>(), "aviso.pdf", "application/pdf", 8,
            It.IsAny<CancellationToken>())).ReturnsAsync(Stored);
        storage.Setup(x => x.EliminarAsync(Key, CancellationToken.None)).Returns(Task.CompletedTask);
        var repo = new Mock<IDocumentoAvisoMvcRepository>(MockBehavior.Strict);
        repo.Setup(x => x.ValidarCargaOriginalAsync(10, 20, 30, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(x => x.GuardarOriginalAsync(It.IsAny<DocumentoAvisoOriginalMvc>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SQL rechazó los metadatos."));
        repo.Setup(x => x.ExisteClaveAlmacenamientoAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new DocumentoAvisoMvcService(storage.Object, repo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GuardarOriginalAsync(
            10, 20, 30, Stream.Null, "aviso.pdf", "application/pdf", 8));

        storage.Verify(x => x.EliminarAsync(Key, CancellationToken.None), Times.Once);
        repo.Verify(x => x.GuardarOriginalAsync(It.Is<DocumentoAvisoOriginalMvc>(dto =>
            dto.AvisoId == 10 && dto.EntidadAcademicaId == 20 && dto.UsuarioId == 30
            && dto.ClaveAlmacenamiento == Key && dto.ChecksumSha256.Length == 32), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GuardarOriginal_DevuelveErrorDeCompensacionSiNoPuedeBorrarArchivo()
    {
        var storage = new Mock<IAlmacenDocumentos>();
        storage.Setup(x => x.GuardarOriginalAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(Stored);
        storage.Setup(x => x.EliminarAsync(Key, CancellationToken.None)).ThrowsAsync(new IOException("Disco ocupado."));
        var repo = new Mock<IDocumentoAvisoMvcRepository>();
        repo.Setup(x => x.ValidarCargaOriginalAsync(10, 20, 30, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(x => x.GuardarOriginalAsync(It.IsAny<DocumentoAvisoOriginalMvc>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SQL no disponible."));
        repo.Setup(x => x.ExisteClaveAlmacenamientoAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new DocumentoAvisoMvcService(storage.Object, repo.Object);

        var error = await Assert.ThrowsAsync<AggregateException>(() => service.GuardarOriginalAsync(
            10, 20, 30, Stream.Null, "aviso.pdf", "application/pdf", 8));

        Assert.Equal(2, error.InnerExceptions.Count);
        Assert.Contains("compensación del archivo", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GuardarOriginal_ConCommitAmbiguoConservaElArchivoReferenciadoPorSql()
    {
        var storage = new Mock<IAlmacenDocumentos>();
        storage.Setup(x => x.GuardarOriginalAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(Stored);
        var repo = new Mock<IDocumentoAvisoMvcRepository>();
        repo.Setup(x => x.ValidarCargaOriginalAsync(10, 20, 30, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(x => x.GuardarOriginalAsync(It.IsAny<DocumentoAvisoOriginalMvc>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("La conexión se perdió al confirmar."));
        repo.Setup(x => x.ExisteClaveAlmacenamientoAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new DocumentoAvisoMvcService(storage.Object, repo.Object);

        var error = await Assert.ThrowsAsync<AggregateException>(() => service.GuardarOriginalAsync(
            10, 20, 30, Stream.Null, "aviso.pdf", "application/pdf", 8));

        Assert.Contains("resultado del guardado fue ambiguo", error.Message, StringComparison.OrdinalIgnoreCase);
        storage.Verify(x => x.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GuardarOriginal_NoInvocaAlmacenCuandoLosIdsNoSonValidos()
    {
        var storage = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        var repo = new Mock<IDocumentoAvisoMvcRepository>(MockBehavior.Strict);
        var service = new DocumentoAvisoMvcService(storage.Object, repo.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GuardarOriginalAsync(
            0, 20, 30, Stream.Null, "aviso.pdf", "application/pdf", 8));

        storage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GuardarOriginal_DevuelveIdCuandoMetadataSeConfirma()
    {
        var storage = new Mock<IAlmacenDocumentos>();
        storage.Setup(x => x.GuardarOriginalAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(Stored);
        var repo = new Mock<IDocumentoAvisoMvcRepository>();
        repo.Setup(x => x.ValidarCargaOriginalAsync(10, 20, 30, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repo.Setup(x => x.GuardarOriginalAsync(It.IsAny<DocumentoAvisoOriginalMvc>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(70);
        var service = new DocumentoAvisoMvcService(storage.Object, repo.Object);

        var id = await service.GuardarOriginalAsync(10, 20, 30, Stream.Null, "aviso.pdf", "application/pdf", 8);

        Assert.Equal(70, id);
        storage.Verify(x => x.EliminarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarBorrador_RestauraArchivosSiFallaLaTransaccionSql()
    {
        var ticket = new DocumentoEnCuarentena(Key, "0123456789abcdef0123456789abcdef");
        var storage = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        storage.Setup(x => x.PrepararEliminacionAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        storage.Setup(x => x.RestaurarEliminacionAsync(ticket, CancellationToken.None)).Returns(Task.CompletedTask);
        var repository = new Mock<IDocumentoAvisoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.ObtenerClavesBorradorAsync(10, 20, 30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Key });
        repository.Setup(x => x.EliminarBorradorAsync(10, 20, 30,
                It.Is<IReadOnlyList<string>>(keys => keys.SequenceEqual(new[] { Key })), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El borrador tiene referencias."));
        repository.Setup(x => x.ExisteAvisoAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new DocumentoAvisoMvcService(storage.Object, repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EliminarBorradorAsync(10, 20, 30));

        storage.Verify(x => x.RestaurarEliminacionAsync(ticket, CancellationToken.None), Times.Once);
        storage.Verify(x => x.CompletarEliminacionAsync(It.IsAny<DocumentoEnCuarentena>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarBorrador_CommitAmbiguoReconsultaSqlYCompletaCuarentenaSiYaNoExiste()
    {
        var ticket = new DocumentoEnCuarentena(Key, "0123456789abcdef0123456789abcdef");
        var storage = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        storage.Setup(x => x.PrepararEliminacionAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        storage.Setup(x => x.CompletarEliminacionAsync(ticket, CancellationToken.None)).Returns(Task.CompletedTask);
        var repository = new Mock<IDocumentoAvisoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.ObtenerClavesBorradorAsync(10, 20, 30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Key });
        repository.Setup(x => x.EliminarBorradorAsync(10, 20, 30,
                It.Is<IReadOnlyList<string>>(keys => keys.SequenceEqual(new[] { Key })), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Conexión perdida al confirmar."));
        repository.Setup(x => x.ExisteAvisoAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new DocumentoAvisoMvcService(storage.Object, repository.Object);

        var error = await Assert.ThrowsAsync<AggregateException>(() => service.EliminarBorradorAsync(10, 20, 30));

        Assert.Contains("SQL confirmó la eliminación", error.Message, StringComparison.OrdinalIgnoreCase);
        storage.Verify(x => x.CompletarEliminacionAsync(ticket, CancellationToken.None), Times.Once);
        storage.Verify(x => x.RestaurarEliminacionAsync(It.IsAny<DocumentoEnCuarentena>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EliminarBorrador_ConfirmaArchivosTrasCommitSql()
    {
        var ticket = new DocumentoEnCuarentena(Key, "0123456789abcdef0123456789abcdef");
        var storage = new Mock<IAlmacenDocumentos>(MockBehavior.Strict);
        storage.Setup(x => x.PrepararEliminacionAsync(Key, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        storage.Setup(x => x.CompletarEliminacionAsync(ticket, CancellationToken.None)).Returns(Task.CompletedTask);
        var repository = new Mock<IDocumentoAvisoMvcRepository>(MockBehavior.Strict);
        repository.Setup(x => x.ObtenerClavesBorradorAsync(10, 20, 30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Key });
        repository.Setup(x => x.EliminarBorradorAsync(10, 20, 30,
                It.Is<IReadOnlyList<string>>(keys => keys.SequenceEqual(new[] { Key })), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = new DocumentoAvisoMvcService(storage.Object, repository.Object);

        await service.EliminarBorradorAsync(10, 20, 30);

        storage.Verify(x => x.CompletarEliminacionAsync(ticket, CancellationToken.None), Times.Once);
        storage.Verify(x => x.RestaurarEliminacionAsync(It.IsAny<DocumentoEnCuarentena>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
