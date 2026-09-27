using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SGPla.Data.NewModel;
using SGPla.Data.NewModel.Entities;
using SGPla.Repositories.Implementations;
using SGPla.Services.Implementations;

namespace SGPla.Tests.Integration;

[Collection("SQL Server integration")]
public sealed class NormalizedDocumentoAvisoSqlServerTests
{
    private const string ConnectionEnvironmentVariable = "SGPLA_SQLSERVER_TEST_CONNECTION";

    [SqlServerFact]
    public async Task Carga_versiona_original_y_rechaza_entidad_ajena()
    {
        var options = new DbContextOptionsBuilder<SgplaDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)!)
            .Options;
        await using var db = new SgplaDbContext(options);
        Assert.Equal("GestionDePlazasBD", db.Database.GetDbConnection().Database);

        var token = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var campus = await db.Campuses.AsNoTracking().Where(x => x.FechaEliminacion == null).OrderBy(x => x.Id).FirstAsync();
        var areaId = await db.AreaAcademicas.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var municipioId = await db.Municipios.AsNoTracking().Select(x => x.Id).FirstAsync();
        var sistemaId = await db.SistemasEducativos.AsNoTracking().Where(x => x.FechaEliminacion == null).Select(x => x.Id).FirstAsync();
        var periodo = new PeriodoEscolar
        {
            Clave = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString(),
            FechaInicio = new DateOnly(2026, 1, 1), FechaFin = new DateOnly(2026, 12, 31)
        };
        db.Entry(periodo).State = EntityState.Added;
        await db.SaveChangesAsync();
        var periodoId = periodo.Id;
        var rolId = await db.Roles.AsNoTracking().Where(x => x.Nombre == "Entidad Académica").Select(x => x.Id).SingleAsync();

        var entidad = new EntidadAcademica
        {
            Clave = $"DOC{token[..10]}", Nombre = $"Entidad documentos {token}", Calle = "Calle de prueba",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234567", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        db.Entry(entidad).State = EntityState.Added;
        await db.SaveChangesAsync();

        var entidadAjena = new EntidadAcademica
        {
            Clave = $"OTR{token[..10]}", Nombre = $"Entidad ajena {token}", Calle = "Calle de prueba",
            Colonia = "Centro", CodigoPostal = "91000", Telefono = "2281234568", CampusId = campus.Id,
            AreaAcademicaId = areaId, MunicipioId = municipioId
        };
        var usuario = new Usuario
        {
            Correo = $"docs.{token[..12].ToLowerInvariant()}@example.test", Nombre = "Usuario integración", RolId = rolId
        };
        var usuarioDgaa = new Usuario
        {
            Correo = $"docs.dgaa.{token[..10].ToLowerInvariant()}@example.test", Nombre = "Usuario DGAA integración", RolId = 2
        };
        var usuarioAjeno = new Usuario
        {
            Correo = $"docs.ajeno.{token[..10].ToLowerInvariant()}@example.test", Nombre = "Usuario ajeno", RolId = rolId
        };
        var articulo = new Articulo { Numero = $"DOC-{token[..16]}", Descripcion = "Fixture de integración" };
        db.Entry(entidadAjena).State = EntityState.Added;
        db.Entry(usuario).State = EntityState.Added;
        db.Entry(usuarioDgaa).State = EntityState.Added;
        db.Entry(usuarioAjeno).State = EntityState.Added;
        db.Entry(articulo).State = EntityState.Added;
        await db.SaveChangesAsync();
        var perfil = new UsuarioEntidadAcademica { UsuarioId = usuario.Id, EntidadAcademicaId = entidad.Id };
        var perfilAjeno = new UsuarioEntidadAcademica { UsuarioId = usuarioAjeno.Id, EntidadAcademicaId = entidadAjena.Id };
        var perfilDgaa = new UsuarioDgaa { UsuarioId = usuarioDgaa.Id, AreaAcademicaId = areaId };
        var aviso = new Aviso
        {
            EntidadAcademicaId = entidad.Id, PeriodoEscolarId = periodoId, SistemaEducativoId = sistemaId,
            ArticuloId = articulo.Id, TipoComunicado = "AVISO", CreadoEn = DateTime.UtcNow,
            Estado = "CREADO"
        };
        db.Entry(perfil).State = EntityState.Added;
        db.Entry(perfilAjeno).State = EntityState.Added;
        db.Entry(perfilDgaa).State = EntityState.Added;
        db.Entry(aviso).State = EntityState.Added;
        await db.SaveChangesAsync();

        var root = Path.Combine(Path.GetTempPath(), "sgpla-doc-test-" + token.ToLowerInvariant());
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Documentos:Directorio"] = root
        }).Build();
        var storage = new AlmacenDocumentosLocal(configuration, new TestHostEnvironment());
        var service = new DocumentoAvisoMvcService(storage, new NormalizedDocumentoAvisoMvcRepository(db, TimeProvider.System));
        try
        {
            var bytes = "%PDF-1.7\nnormalized aviso fixture"u8.ToArray();
            await using var first = new MemoryStream(bytes);
            await service.GuardarOriginalAsync(aviso.Id, entidad.Id, usuario.Id, first, "original.pdf", "application/pdf", bytes.Length);
            await using var second = new MemoryStream(bytes);
            await service.GuardarOriginalAsync(aviso.Id, entidad.Id, usuario.Id, second, "original-v2.pdf", "application/pdf", bytes.Length);

            var versions = await db.DocumentoAvisos.AsNoTracking().Where(x => x.AvisoId == aviso.Id && x.Tipo == "ORIGINAL")
                .OrderBy(x => x.NumeroVersion).ToListAsync();
            Assert.Collection(versions,
                firstVersion => { Assert.Equal(1, firstVersion.NumeroVersion); Assert.False(firstVersion.EsVigente); },
                current => { Assert.Equal(2, current.NumeroVersion); Assert.True(current.EsVigente); });
            Assert.Equal(System.Security.Cryptography.SHA256.HashData(bytes), versions[1].ChecksumSha256);

            var download = await service.AbrirParaDescargaAsync(aviso.Id, versions[1].Id, usuario.Id);
            Assert.NotNull(download);
            await using (download.Contenido)
            using (var downloaded = new MemoryStream())
            {
                await download.Contenido.CopyToAsync(downloaded);
                Assert.Equal(bytes, downloaded.ToArray());
            }
            var historicalDownload = await service.AbrirParaDescargaAsync(aviso.Id, versions[0].Id, usuario.Id);
            Assert.NotNull(historicalDownload);
            Assert.Equal("original.pdf", historicalDownload.Nombre);
            await using (historicalDownload.Contenido)
            using (var historicalBytes = new MemoryStream())
            {
                await historicalDownload.Contenido.CopyToAsync(historicalBytes);
                Assert.Equal(bytes, historicalBytes.ToArray());
            }
            Assert.Null(await service.AbrirParaDescargaAsync(aviso.Id, versions[1].Id, usuarioAjeno.Id));

            await using var foreign = new MemoryStream(bytes);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GuardarOriginalAsync(aviso.Id, entidadAjena.Id, usuario.Id, foreign, "ajeno.pdf", "application/pdf", bytes.Length));
            Assert.Equal(2, await db.DocumentoAvisos.AsNoTracking().CountAsync(x => x.AvisoId == aviso.Id));
            Assert.Equal(2, Directory.GetFiles(Path.Combine(root, "avisos"), "*.pdf").Length);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.EliminarBorradorAsync(aviso.Id, entidadAjena.Id, usuario.Id));
            var revisionProtectora = new RevisionAviso
            {
                AvisoId = aviso.Id, NumeroRevision = 1, DocumentoOriginalId = versions[1].Id,
                EnviadoPorUsuarioId = usuario.Id, EnviadoEn = DateTime.UtcNow
            };
            db.Entry(revisionProtectora).State = EntityState.Added;
            await db.SaveChangesAsync();
            foreach (var estado in new[] { "EN_REVISION_DGAA", "DEVUELTO_DGAA", "AVALADO_DGAA", "FIRMADO", "PUBLICADO" })
            {
                aviso.Estado = estado;
                await db.SaveChangesAsync();
                var dgaaDownload = await service.AbrirParaDescargaAsync(aviso.Id, versions[1].Id, usuarioDgaa.Id);
                Assert.NotNull(dgaaDownload);
                await dgaaDownload.Contenido.DisposeAsync();
            }
            aviso.Estado = "CREADO";
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.EliminarBorradorAsync(aviso.Id, entidad.Id, usuario.Id));
            Assert.Equal(2, Directory.GetFiles(Path.Combine(root, "avisos"), "*.pdf").Length);
            await storage.EliminarAsync(versions[0].ClaveAlmacenamiento);
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                service.AbrirParaDescargaAsync(aviso.Id, versions[0].Id, usuario.Id));
            var rutaVigente = Path.Combine(root, versions[1].ClaveAlmacenamiento.Replace('/', Path.DirectorySeparatorChar));
            await File.WriteAllTextAsync(rutaVigente, "contenido alterado");
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                service.AbrirParaDescargaAsync(aviso.Id, versions[1].Id, usuario.Id));
            db.Entry(revisionProtectora).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            await service.EliminarBorradorAsync(aviso.Id, entidad.Id, usuario.Id);
            Assert.False(await db.Avisos.AsNoTracking().AnyAsync(x => x.Id == aviso.Id));
            Assert.False(await db.DocumentoAvisos.AsNoTracking().AnyAsync(x => x.AvisoId == aviso.Id));
            Assert.Empty(Directory.GetFiles(Path.Combine(root, "avisos"), "*.pdf"));
        }
        finally
        {
            try
            {
                db.RevisionAvisos.RemoveRange(await db.RevisionAvisos.AsTracking().Where(x => x.AvisoId == aviso.Id).ToListAsync());
                await db.SaveChangesAsync();
                db.DocumentoAvisos.RemoveRange(await db.DocumentoAvisos.AsTracking().Where(x => x.AvisoId == aviso.Id).ToListAsync());
                await db.SaveChangesAsync();
                var avisoRestante = await db.Avisos.AsTracking().SingleOrDefaultAsync(x => x.Id == aviso.Id);
                if (avisoRestante is not null) db.Avisos.Remove(avisoRestante);
                db.UsuariosEntidadAcademica.Remove(await db.UsuariosEntidadAcademica.AsTracking().SingleAsync(x => x.UsuarioId == usuario.Id));
                db.UsuariosEntidadAcademica.Remove(await db.UsuariosEntidadAcademica.AsTracking().SingleAsync(x => x.UsuarioId == usuarioAjeno.Id));
                db.UsuariosDgaa.Remove(await db.UsuariosDgaa.AsTracking().SingleAsync(x => x.UsuarioId == usuarioDgaa.Id));
                await db.SaveChangesAsync();
                db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuario.Id));
                db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuarioAjeno.Id));
                db.Usuarios.Remove(await db.Usuarios.AsTracking().SingleAsync(x => x.Id == usuarioDgaa.Id));
                db.Articulos.Remove(await db.Articulos.AsTracking().SingleAsync(x => x.Id == articulo.Id));
                db.PeriodosEscolares.Remove(await db.PeriodosEscolares.AsTracking().SingleAsync(x => x.Id == periodoId));
                db.EntidadAcademicas.RemoveRange(await db.EntidadAcademicas.AsTracking()
                    .Where(x => x.Id == entidad.Id || x.Id == entidadAjena.Id).ToListAsync());
                await db.SaveChangesAsync();
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
            }
        }
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable)))
                Skip = $"Configura {ConnectionEnvironmentVariable} para ejecutar la prueba SQL Server.";
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
