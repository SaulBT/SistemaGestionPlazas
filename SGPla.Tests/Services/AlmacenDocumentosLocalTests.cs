using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Services;

public sealed class AlmacenDocumentosLocalTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "sgpla-docs-" + Guid.NewGuid().ToString("N"));
    private readonly AlmacenDocumentosLocal _storage;

    public AlmacenDocumentosLocalTests()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Documentos:Directorio"] = _root
        }).Build();
        _storage = new AlmacenDocumentosLocal(configuration, new TestHostEnvironment());
    }

    [Fact]
    public async Task Guardar_UsaClaveAleatoriaYCalculaChecksumEnStreaming()
    {
        var bytes = "%PDF-1.7\nfixture"u8.ToArray();
        await using var input = new MemoryStream(bytes);

        var saved = await _storage.GuardarOriginalAsync(input, "../aviso.pdf", "application/pdf", bytes.Length);

        Assert.Equal("aviso.pdf", saved.Nombre);
        Assert.Equal("application/pdf", saved.Mime);
        Assert.Equal(bytes.Length, saved.Tamano);
        Assert.Equal(System.Security.Cryptography.SHA256.HashData(bytes), saved.ChecksumSha256);
        Assert.StartsWith("avisos/", saved.ClaveRelativa, StringComparison.Ordinal);
        Assert.DoesNotContain("..", saved.ClaveRelativa, StringComparison.Ordinal);
        await using var stored = await _storage.AbrirLecturaAsync(saved.ClaveRelativa);
        Assert.NotNull(stored);
        using var copy = new MemoryStream();
        await stored!.CopyToAsync(copy);
        Assert.Equal(bytes, copy.ToArray());

        await _storage.EliminarAsync(saved.ClaveRelativa);
        Assert.Null(await _storage.AbrirLecturaAsync(saved.ClaveRelativa));
    }

    [Theory]
    [InlineData("text/html", "%PDF-x")]
    [InlineData("application/pdf", "not a pdf")]
    public async Task Guardar_RechazaMimeOFirmaNoPermitidos(string mime, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        await using var input = new MemoryStream(bytes);

        if (mime != "application/pdf")
            await Assert.ThrowsAsync<ArgumentException>(() => _storage.GuardarOriginalAsync(input, "aviso.pdf", mime, bytes.Length));
        else
            await Assert.ThrowsAsync<InvalidDataException>(() => _storage.GuardarOriginalAsync(input, "aviso.pdf", mime, bytes.Length));
    }

    [Fact]
    public async Task Guardar_RechazaDiscrepanciaDeTamanoYNoDejaTemporales()
    {
        var bytes = "%PDF-1.7"u8.ToArray();
        await using var input = new MemoryStream(bytes);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            _storage.GuardarOriginalAsync(input, "aviso.pdf", "application/pdf", bytes.Length + 1));
        Assert.Empty(Directory.Exists(Path.Combine(_root, "avisos"))
            ? Directory.GetFiles(Path.Combine(_root, "avisos"))
            : []);
    }

    [Fact]
    public async Task Guardar_RechazaTamanoMayorAlLimiteAntesDeEscribir()
    {
        await using var input = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.GuardarOriginalAsync(
            input, "aviso.pdf", "application/pdf", AlmacenDocumentosLocal.TamanoMaximo + 1));
        Assert.False(Directory.Exists(Path.Combine(_root, "avisos")));
    }

    [Theory]
    [InlineData("../aviso.pdf")]
    [InlineData("avisos/../../archivo.pdf")]
    [InlineData("avisos/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.txt")]
    public async Task Abrir_RechazaClavesNoGeneradasPorElAlmacen(string key)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.AbrirLecturaAsync(key));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
