using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class PlanEstudios
{
    public int idPlanEstudios { get; set; }

    public int idProgramaEducativo { get; set; }

    public string nombre { get; set; } = null!;

    public string modalidad { get; set; } = null!;

    public virtual ICollection<ExperienciaEducativa> ExperienciaEducativa { get; set; } = new List<ExperienciaEducativa>();

    public virtual ProgramaEducativo idProgramaEducativoNavigation { get; set; } = null!;
}
