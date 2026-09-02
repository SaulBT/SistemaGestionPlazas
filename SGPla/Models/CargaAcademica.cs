using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class CargaAcademica
{
    public int IdCargaAcademica { get; set; }

    public int IdDocente { get; set; }

    public int IdPeriodo { get; set; }

    public int? IdExperienciaEducativa { get; set; }

    public string? Nrc { get; set; }

    public string? Plaza { get; set; }

    public string? TipoContratacion { get; set; }

    public int HorasPago { get; set; }

    public bool? Imparte { get; set; }

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual ExperienciaEducativa? IdExperienciaEducativaNavigation { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
}
