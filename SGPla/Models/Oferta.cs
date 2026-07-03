using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Oferta
{
    public int IdOferta { get; set; }

    public int? IdDocente { get; set; }

    public int IdExperienciaEducativa { get; set; }

    public int IdProgramaEducativo { get; set; }

    public int IdArticulo { get; set; }

    public int IdPeriodo { get; set; }

    public string? Plaza { get; set; }

    public string? TipoContratacion { get; set; }

    public string Nrc { get; set; } = null!;

    public bool Incluida { get; set; }

    public string? Justificacion { get; set; }

    public int? IdArchivoApertura { get; set; }

    public string EstadoSolicitudApertura { get; set; } = null!;

    public int Hsm { get; set; }

    public string? JustificacionApertura { get; set; }

    public string? TipoPlaza { get; set; }

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();

    public virtual Archivo? IdArchivoAperturaNavigation { get; set; }

    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    public virtual Docente? IdDocenteNavigation { get; set; }

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual ProgramaEducativo IdProgramaEducativoNavigation { get; set; } = null!;

    public virtual ICollection<Log> Log { get; set; } = new List<Log>();

    public virtual ICollection<OfertaAviso> OfertaAviso { get; set; } = new List<OfertaAviso>();

    public virtual ICollection<Solicitud> Solicitud { get; set; } = new List<Solicitud>();
}
