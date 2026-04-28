using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdActa { get; set; }

    public int IdArchivoOriginal { get; set; }

    public int? IdArchivoFirmado { get; set; }

    public virtual Acta IdActaNavigation { get; set; } = null!;

    public virtual Archivo? IdArchivoFirmadoNavigation { get; set; }

    public virtual Archivo IdArchivoOriginalNavigation { get; set; } = null!;
}
