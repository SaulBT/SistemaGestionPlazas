using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Acta
{
    public int idActa { get; set; }

    public int idAviso { get; set; }

    public string folio { get; set; } = null!;

    public string lugar { get; set; } = null!;

    public DateOnly fecha { get; set; }

    public TimeOnly horaInicio { get; set; }

    public TimeOnly horaConclusion { get; set; }

    public string asuntosGenerales { get; set; } = null!;

    public string? rutaDocumentoOriginal { get; set; }

    public string? rutaDocumentoFirmado { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual ICollection<Dictamen> Dictamen { get; set; } = new List<Dictamen>();

    public virtual ICollection<Notificacion> Notificacion { get; set; } = new List<Notificacion>();

    public virtual Aviso idAvisoNavigation { get; set; } = null!;
}
