namespace SGPla.Models;

public partial class SincronizacionPlanea
{
    public int IdSincronizacionPlanea { get; set; }
    public int IdPeriodo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = null!;
    public int? RegistrosRecibidos { get; set; }
    public int? NrcRecibidos { get; set; }
    public int? NrcSinPlan { get; set; }
    public int? NrcSinExperiencia { get; set; }
    public int? NrcExistentes { get; set; }
    public int? NrcNuevos { get; set; }
    public int? HorariosInsertados { get; set; }
    public string? Advertencias { get; set; }
    public string? MensajeError { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo { get; set; } = new List<ExperienciaEducativaPeriodo>();
}
