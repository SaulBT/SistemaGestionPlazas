using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Solicitud
{
    public int IdSolicitud { get; set; }

    public int IdDocente { get; set; }

    public int IdOferta { get; set; }

    public bool Designado { get; set; }

    public string Modalidad { get; set; } = null!;

    public string Justificacion { get; set; } = null!;

    public string RutaDocumentosSolicitud { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual Oferta IdOfertaNavigation { get; set; } = null!;
}
