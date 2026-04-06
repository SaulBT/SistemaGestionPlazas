using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Oferta
{
    public int idOferta { get; set; }

    public int idDocente { get; set; }

    public int idExperienciaEducativa { get; set; }

    public int idArticulo { get; set; }

    public int idPeriodo { get; set; }

    public string? plaza { get; set; }

    public string tipoContratacion { get; set; } = null!;

    public string nrc { get; set; } = null!;

    public bool vacante { get; set; }

    public bool incluida { get; set; }

    public string justificacion { get; set; } = null!;

    public string? rutaArchivoSolicitudApertura { get; set; }

    public string estadoSolicitudApertura { get; set; } = null!;

    public int hsm { get; set; }

    public string? justificacionApertura { get; set; }

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();

    public virtual ICollection<Log> Log { get; set; } = new List<Log>();

    public virtual ICollection<OfertaAviso> OfertaAviso { get; set; } = new List<OfertaAviso>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();

    public virtual Articulo idArticuloNavigation { get; set; } = null!;

    public virtual Docente idDocenteNavigation { get; set; } = null!;

    public virtual ExperienciaEducativa idExperienciaEducativaNavigation { get; set; } = null!;

    public virtual Periodo idPeriodoNavigation { get; set; } = null!;
}
