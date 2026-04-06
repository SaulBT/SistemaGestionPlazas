using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class ExperienciaEducativa
{
    public int IdExperienciaEducativa { get; set; }

    public int IdPlanEstudios { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string PerfilDocente { get; set; } = null!;

    public virtual PlanEstudio IdPlanEstudiosNavigation { get; set; } = null!;

    public virtual ICollection<Ofertum> Oferta { get; set; } = new List<Ofertum>();
}
