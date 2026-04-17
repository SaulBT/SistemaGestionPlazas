using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Aviso
{
    public int IdAviso { get; set; }

    public int IdEntidadAcademica { get; set; }

    public int IdPeriodo { get; set; }

    public int IdArticulo { get; set; }

    public string Folio { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaCreacion { get; set; }

    public string Requisitos { get; set; } = null!;

    public string? Lugar { get; set; }

    public string? Correo { get; set; }

    public string Modalidad { get; set; } = null!;

    public string? RutaDocumentoOriginal { get; set; }

    public string? RutaDocumentoFirmado { get; set; }

    public bool? Archivado { get; set; }

    public virtual ICollection<Acta> Acta { get; set; } = new List<Acta>();

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();

    public virtual Articulo IdArticuloNavigation { get; set; } = null!;

    public virtual EntidadAcademica IdEntidadAcademicaNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual ICollection<OfertaAviso> OfertaAviso { get; set; } = new List<OfertaAviso>();
}
