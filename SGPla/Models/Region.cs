namespace SGPla.Models;

public partial class Region
{
    public int IdRegion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<EntidadAcademica> EntidadAcademica { get; set; } = new List<EntidadAcademica>();

    public virtual ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo { get; set; } = new List<ExperienciaEducativaPeriodo>();
}
