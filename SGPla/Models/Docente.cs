using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Docente
{
    public int IdDocente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? DescripcionPerfil { get; set; }

    public int? IdArchivosGenerales { get; set; }

    public string? NumeroPersonal { get; set; }

    public string? Puesto { get; set; }

    public virtual ICollection<Dictamen> Dictamen { get; set; } = new List<Dictamen>();

    public virtual ICollection<Grado> Grado { get; set; } = new List<Grado>();

    public virtual Archivo? IdArchivosGeneralesNavigation { get; set; }

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();
}
