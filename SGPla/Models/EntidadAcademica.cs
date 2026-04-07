using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class EntidadAcademica
{
    public int IdEntidadAcademica { get; set; }

    public int IdAreaAcademica { get; set; }

    public string Nombre { get; set; } = null!;

    public string CalleNumero { get; set; } = null!;

    public string Colonia { get; set; } = null!;

    public string Cp { get; set; } = null!;

    public string Municipio { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Conmutador { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public string Fax { get; set; } = null!;

    public string Region { get; set; } = null!;

    public string Campus { get; set; } = null!;

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<CoordinadorEa> CoordinadorEa { get; set; } = new List<CoordinadorEa>();

    public virtual AreaAcademica IdAreaAcademicaNavigation { get; set; } = null!;

    public virtual ICollection<IntegranteCt> IntegranteCt { get; set; } = new List<IntegranteCt>();

    public virtual ICollection<ProgramaEducativo> ProgramaEducativo { get; set; } = new List<ProgramaEducativo>();
}
