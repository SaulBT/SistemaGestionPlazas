using SGPla.Models.DTOs.Solicitudes;

namespace SGPla.Services.Interfaces;

public interface ISolicitudMvcService
{
    Task<SolicitudMvcListado?> ListarPorAvisoAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        CancellationToken cancellationToken = default);

    Task<int> RegistrarAsync(int usuarioId, int entidadAcademicaId, RegistrarSolicitudMvcDatos datos,
        CancellationToken cancellationToken = default);

    Task<SolicitudMvcDetalle?> ObtenerDetalleAsync(int usuarioId, int entidadAcademicaId, int avisoId,
        int solicitudId, CancellationToken cancellationToken = default);

    Task ActualizarPerfilAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        EditarPerfilSolicitudMvcDatos datos, CancellationToken cancellationToken = default);

    Task ResolverAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        ResolverSolicitudMvcDatos datos, CancellationToken cancellationToken = default);

    Task RetirarAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        string? motivo, CancellationToken cancellationToken = default);

    Task<int> AgregarDocumentoAsync(int usuarioId, int entidadAcademicaId, int avisoId, int solicitudId,
        int tipoDocumentoId, int? documentoAspiranteId, Stream contenido, string nombre, string mime, long tamanoDeclarado,
        CancellationToken cancellationToken = default);

    Task<DescargaDocumentoSolicitudMvc?> AbrirDocumentoAsync(int usuarioId, int entidadAcademicaId,
        int avisoId, int solicitudId, int versionDocumentoId, CancellationToken cancellationToken = default);
}
