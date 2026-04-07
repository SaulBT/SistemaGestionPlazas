using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdActa { get; set; }

    public string RutaDocumentoOriginal { get; set; } = null!;

    public string? RutaDocumentoFirmado { get; set; }

    public virtual Acta IdActaNavigation { get; set; } = null!;
}
