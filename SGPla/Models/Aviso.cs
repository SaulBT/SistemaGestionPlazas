using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Aviso
{
    public int idAviso { get; set; }

    public int idEntidadAcademica { get; set; }

    public int idPeriodo { get; set; }

    public int idArticulo { get; set; }

    public string folio { get; set; } = null!;

    public DateOnly fechaInicio { get; set; }

    public DateOnly fechaCreacion { get; set; }

    public string requisitos { get; set; } = null!;

    public string? lugar { get; set; }

    public string? correo { get; set; }

    public string modalidad { get; set; } = null!;

    public string? rutaDocumentoOriginal { get; set; }

    public string? rutaDocumentoFirmado { get; set; }

    public virtual ICollection<Acta> Acta { get; set; } = new List<Acta>();

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();

    public virtual ICollection<OfertaAviso> OfertaAviso { get; set; } = new List<OfertaAviso>();

    public virtual Articulo idArticuloNavigation { get; set; } = null!;

    public virtual EntidadAcademica idEntidadAcademicaNavigation { get; set; } = null!;

    public virtual Periodo idPeriodoNavigation { get; set; } = null!;
}
