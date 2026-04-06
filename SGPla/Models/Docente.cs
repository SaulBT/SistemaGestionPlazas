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

    public virtual ICollection<Dictaman> Dictamen { get; set; } = new List<Dictaman>();

    public virtual Grado? Grado { get; set; }

    public virtual ICollection<Ofertum> Oferta { get; set; } = new List<Ofertum>();

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();
}
