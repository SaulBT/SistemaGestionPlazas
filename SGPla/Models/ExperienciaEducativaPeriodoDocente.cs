namespace SGPla.Models;

public partial class ExperienciaEducativaPeriodoDocente
{
    public int IdExperienciaEducativaPeriodoDocente { get; set; }
    public int IdExperienciaEducativaPeriodo { get; set; }
    public string? NumeroPersonal { get; set; }
    public string Nombre { get; set; } = null!;
    public bool? Imparte { get; set; }

    public virtual ExperienciaEducativaPeriodo IdExperienciaEducativaPeriodoNavigation { get; set; } = null!;
}
