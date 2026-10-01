namespace SGPla.Models;

public partial class ExperienciaEducativaPeriodo
{
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdExperienciaEducativa { get; set; }
    public int IdPeriodo { get; set; }
    public int IdPlanEstudios { get; set; }
    public int? IdRegion { get; set; }
    public int IdSincronizacionPlanea { get; set; }
    public string Nrc { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string? Campus { get; set; }
    public string? Nivel { get; set; }
    public string? Area { get; set; }
    public DateTime FechaAlta { get; set; }

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;
    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
    public virtual Region? IdRegionNavigation { get; set; }
    public virtual SincronizacionPlanea IdSincronizacionPlaneaNavigation { get; set; } = null!;
    public virtual ICollection<ExperienciaEducativaPeriodoHorario> Horarios { get; set; } = new List<ExperienciaEducativaPeriodoHorario>();
    public virtual ICollection<ExperienciaEducativaPeriodoDocente> Docentes { get; set; } = new List<ExperienciaEducativaPeriodoDocente>();
}
