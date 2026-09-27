using SGPla.Models.DTOs.Solicitudes;

namespace SGPla.Repositories.Interfaces;

public interface ISolicitudMvcRepository
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
        DocumentoAspiranteMvcDatos documento, CancellationToken cancellationToken = default);

    Task<(string Nombre, string Mime, string ClaveRelativa, byte[] Checksum)?> ObtenerDocumentoAsync(int usuarioId,
        int entidadAcademicaId, int avisoId, int solicitudId, int versionDocumentoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteClaveDocumentoAsync(string claveRelativa, CancellationToken cancellationToken = default);
}
