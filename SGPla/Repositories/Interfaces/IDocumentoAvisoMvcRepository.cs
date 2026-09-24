using SGPla.Models.DTOs.Avisos;

namespace SGPla.Repositories.Interfaces;

public interface IDocumentoAvisoMvcRepository
{
    Task ValidarCargaOriginalAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default);

    Task<int> GuardarOriginalAsync(DocumentoAvisoOriginalMvc documento,
        CancellationToken cancellationToken = default);

    Task<DocumentoAvisoDescargaMvc?> ObtenerParaDescargaAsync(int avisoId, int documentoId,
        int usuarioId, CancellationToken cancellationToken = default);
}
