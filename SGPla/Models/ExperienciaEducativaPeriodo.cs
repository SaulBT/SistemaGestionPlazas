namespace SGPla.Models;

public partial class ExperienciaEducativaPeriodo
{
    public int IdExperienciaEducativaPeriodo { get; set; }
    // Enlace con el catálogo; null mientras la EE o el plan de PLANEA no estén cargados.
    public int? IdExperienciaEducativa { get; set; }
    public int IdPeriodo { get; set; }
    public int? IdPlanEstudios { get; set; }
    public int? IdRegion { get; set; }
    public int IdSincronizacionPlanea { get; set; }
    public string Nrc { get; set; } = null!;
    public string CodigoExperiencia { get; set; } = null!;
    public string CodigoPlan { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string? Campus { get; set; }
    public string? Nivel { get; set; }
    public string? Area { get; set; }
    public DateTime FechaAlta { get; set; }
    public string EstadoAprobacion { get; set; } = null!;
    public int? IdOferta { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string? RevisadoPor { get; set; }

    public virtual ExperienciaEducativa? IdExperienciaEducativaNavigation { get; set; }
    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual Oferta? IdOfertaNavigation { get; set; }
    public virtual PlanEstudios? IdPlanEstudiosNavigation { get; set; }
    public virtual Region? IdRegionNavigation { get; set; }
    public virtual SincronizacionPlanea IdSincronizacionPlaneaNavigation { get; set; } = null!;
    public virtual ICollection<ExperienciaEducativaPeriodoHorario> Horarios { get; set; } = new List<ExperienciaEducativaPeriodoHorario>();
    public virtual ICollection<ExperienciaEducativaPeriodoDocente> Docentes { get; set; } = new List<ExperienciaEducativaPeriodoDocente>();
}
