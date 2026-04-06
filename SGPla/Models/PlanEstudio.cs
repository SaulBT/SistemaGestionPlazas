using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class PlanEstudio
{
    public int IdPlanEstudios { get; set; }

    public int IdProgramaEducativo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Modalidad { get; set; } = null!;

    public virtual ICollection<ExperienciaEducativa> ExperienciaEducativas { get; set; } = new List<ExperienciaEducativa>();

    public virtual ProgramaEducativo IdProgramaEducativoNavigation { get; set; } = null!;
}
