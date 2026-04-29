using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Archivo
{
    public int IdArchivo { get; set; }

    public string? Nombre { get; set; }

    public string? Ruta { get; set; }

    public string? Tipo { get; set; }

    public double? Tamanio { get; set; }

    public virtual ICollection<Acta> ActaIdArchivoFirmadoNavigation { get; set; } = new List<Acta>();

    public virtual ICollection<Acta> ActaIdArchivoOriginalNavigation { get; set; } = new List<Acta>();

    public virtual ICollection<Aviso> AvisoIdArchivoFirmadoNavigation { get; set; } = new List<Aviso>();

    public virtual ICollection<Aviso> AvisoIdArchivoOriginalNavigation { get; set; } = new List<Aviso>();

    public virtual ICollection<Dictamen> DictamenIdArchivoFirmadoNavigation { get; set; } = new List<Dictamen>();

    public virtual ICollection<Dictamen> DictamenIdArchivoOriginalNavigation { get; set; } = new List<Dictamen>();

    public virtual ICollection<Docente> Docente { get; set; } = new List<Docente>();

    public virtual ICollection<Notificacion> NotificacionIdArchivoFirmadoNavigation { get; set; } = new List<Notificacion>();

    public virtual ICollection<Notificacion> NotificacionIdArchivoOriginalNavigation { get; set; } = new List<Notificacion>();

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();
}
