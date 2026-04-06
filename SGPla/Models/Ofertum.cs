using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Ofertum
{
    public int IdOferta { get; set; }

    public int IdDocente { get; set; }

    public int IdExperienciaEducativa { get; set; }

    public int IdArticulo { get; set; }

    public int IdPeriodo { get; set; }

    public string? Plaza { get; set; }

    public string TipoContratacion { get; set; } = null!;

    public string Nrc { get; set; } = null!;

    public bool Vacante { get; set; }

    public bool Incluida { get; set; }

    public string Justificacion { get; set; } = null!;

    public string? RutaArchivoSolicitudApertura { get; set; }

    public string EstadoSolicitudApertura { get; set; } = null!;

    public int Hsm { get; set; }

    public string? JustificacionApertura { get; set; }

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<OfertaAviso> OfertaAvisos { get; set; } = new List<OfertaAviso>();

    public virtual ICollection<Solicitud> Solicituds { get; set; } = new List<Solicitud>();
}
