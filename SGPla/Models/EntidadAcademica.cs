using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class EntidadAcademica
{
    public int idEntidadAcademica { get; set; }

    public int idAreaAcademica { get; set; }

    public string nombre { get; set; } = null!;

    public string calleNumero { get; set; } = null!;

    public string colonia { get; set; } = null!;

    public string cp { get; set; } = null!;

    public string municipio { get; set; } = null!;

    public string telefono { get; set; } = null!;

    public string conmutador { get; set; } = null!;

    public string extension { get; set; } = null!;

    public string fax { get; set; } = null!;

    public string region { get; set; } = null!;

    public string campus { get; set; } = null!;

    public virtual ICollection<Aviso> Aviso { get; set; } = new List<Aviso>();

    public virtual ICollection<CoordinadorEA> CoordinadorEA { get; set; } = new List<CoordinadorEA>();

    public virtual ICollection<IntegranteCT> IntegranteCT { get; set; } = new List<IntegranteCT>();

    public virtual ICollection<ProgramaEducativo> ProgramaEducativo { get; set; } = new List<ProgramaEducativo>();

    public virtual AreaAcademica idAreaAcademicaNavigation { get; set; } = null!;
}
