namespace SGPla.Models.DTOs.Avisos;

public sealed record DocumentoAvisoOriginalMvc(int AvisoId, int EntidadAcademicaId, int UsuarioId,
    string Nombre, string Mime, long Tamano, byte[] ChecksumSha256, string ClaveAlmacenamiento);
