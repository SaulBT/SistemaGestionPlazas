using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Actum
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

    public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();

    public virtual ICollection<Dictaman> Dictamen { get; set; } = new List<Dictaman>();

    public virtual Aviso IdAvisoNavigation { get; set; } = null!;

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();
}
