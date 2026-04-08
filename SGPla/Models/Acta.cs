using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Acta
{
    public int IdActa { get; set; }

    public int IdAviso { get; set; }

    public string Folio { get; set; } = null!;

    public string Lugar { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraConclusion { get; set; }

    public string AsuntosGenerales { get; set; } = null!;

    public string? RutaDocumentoOriginal { get; set; }

    public string? RutaDocumentoFirmado { get; set; }

    public bool? Archivado { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual ICollection<Dictamen> Dictamen { get; set; } = new List<Dictamen>();

    public virtual Aviso IdAvisoNavigation { get; set; } = null!;

    public virtual ICollection<Notificacion> Notificacion { get; set; } = new List<Notificacion>();
}
