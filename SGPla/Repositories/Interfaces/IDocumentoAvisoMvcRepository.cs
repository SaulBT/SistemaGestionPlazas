using SGPla.Models.DTOs.Avisos;

namespace SGPla.Repositories.Interfaces;

public interface IDocumentoAvisoMvcRepository
{
    Task ValidarCargaOriginalAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default);

    Task<int> GuardarOriginalAsync(DocumentoAvisoOriginalMvc documento,
        CancellationToken cancellationToken = default);

    Task ValidarCargaFirmadoAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default);

    Task<int> GuardarFirmadoAsync(DocumentoAvisoOriginalMvc documento,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerClavesBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default);

    Task EliminarBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        IReadOnlyList<string> clavesEsperadas, CancellationToken cancellationToken = default);

    Task<DocumentoAvisoDescargaMvc?> ObtenerParaDescargaAsync(int avisoId, int documentoId,
        int usuarioId, CancellationToken cancellationToken = default);

    Task<bool> ExisteClaveAlmacenamientoAsync(string claveRelativa, CancellationToken cancellationToken = default);

    Task<bool> ExisteAvisoAsync(int avisoId, CancellationToken cancellationToken = default);
}
