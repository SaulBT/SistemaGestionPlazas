using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class ProgramaEducativo
{
    public int IdProgramaEducativo { get; set; }

    public int IdEntidadAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;

    public virtual ICollection<PlanEstudio> PlanEstudios { get; set; } = new List<PlanEstudio>();
}
