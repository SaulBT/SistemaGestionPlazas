using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Docente
{
    public int idDocente { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcionPerfil { get; set; }

    public string? rutaDocumentosGenerales { get; set; }

    public string numeroPersonal { get; set; } = null!;

    public string puesto { get; set; } = null!;

    public virtual ICollection<Dictamen> Dictamen { get; set; } = new List<Dictamen>();

    public virtual Grado? Grado { get; set; }

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();
}
