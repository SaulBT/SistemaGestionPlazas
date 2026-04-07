using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Docente
{
    public int IdDocente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? DescripcionPerfil { get; set; }

    public string? RutaDocumentosGenerales { get; set; }

    public string NumeroPersonal { get; set; } = null!;

    public string Puesto { get; set; } = null!;

    public virtual ICollection<Dictamen> Dictamen { get; set; } = new List<Dictamen>();

    public virtual Grado? Grado { get; set; }

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();
}
