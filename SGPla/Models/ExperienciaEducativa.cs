using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class ExperienciaEducativa
{
    public int idExperienciaEducativa { get; set; }

    public int idPlanEstudios { get; set; }

    public string codigo { get; set; } = null!;

    public string nombre { get; set; } = null!;

    public string perfilDocente { get; set; } = null!;

    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();

    public virtual PlanEstudios idPlanEstudiosNavigation { get; set; } = null!;
}
