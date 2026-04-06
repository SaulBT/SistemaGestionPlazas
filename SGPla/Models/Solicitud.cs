using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Solicitud
{
    public int idSolicitud { get; set; }

    public int idDocente { get; set; }

    public int idOferta { get; set; }

    public bool designado { get; set; }

    public string modalidad { get; set; } = null!;

    public string justificacion { get; set; } = null!;

    public string rutaDocumentosSolicitud { get; set; } = null!;

    public virtual Docente idDocenteNavigation { get; set; } = null!;

    public virtual Oferta idOfertaNavigation { get; set; } = null!;
}
