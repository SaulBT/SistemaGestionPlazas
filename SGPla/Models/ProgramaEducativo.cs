using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class ProgramaEducativo
{
    public int IdProgramaEducativo { get; set; }

    public int IdEntidadAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string Campus { get; set; } = null!;

    public DateTime? FechaEliminacion { get; set; }

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();
}
