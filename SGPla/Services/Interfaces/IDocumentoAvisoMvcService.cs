using SGPla.Models.DTOs.Avisos;

namespace SGPla.Services.Interfaces;

public interface IDocumentoAvisoMvcService
{
    Task<int> GuardarOriginalAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default);

    Task<int> GuardarFirmadoAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default);

    Task EliminarBorradorAsync(int avisoId, int entidadAcademicaId, int usuarioId,
        CancellationToken cancellationToken = default);

    Task<DescargaDocumentoAvisoMvc?> AbrirParaDescargaAsync(int avisoId, int documentoId,
        int usuarioId, CancellationToken cancellationToken = default);
}
