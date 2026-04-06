using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class ProgramaEducativo
{
    public int idProgramaEducativo { get; set; }

    public int idEntidadAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();

    public virtual EntidadAcademica idEntidadAcademicaNavigation { get; set; } = null!;
}
