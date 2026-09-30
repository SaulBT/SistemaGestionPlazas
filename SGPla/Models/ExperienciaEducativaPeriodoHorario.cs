namespace SGPla.Models;

public partial class ExperienciaEducativaPeriodoHorario
{
    public int IdExperienciaEducativaPeriodoHorario { get; set; }
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdHorarioPlanea { get; set; }
    public string Dia { get; set; } = null!;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Edificio { get; set; }
    public string? Aula { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public virtual ExperienciaEducativaPeriodo IdExperienciaEducativaPeriodoNavigation { get; set; } = null!;
}
