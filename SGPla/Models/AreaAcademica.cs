using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class AreaAcademica
{
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

    public virtual ICollection<CoordinadorDgaa> CoordinadorDgaas { get; set; } = new List<CoordinadorDgaa>();

    public virtual ICollection<EntidadAcademica> EntidadAcademicas { get; set; } = new List<EntidadAcademica>();
}
