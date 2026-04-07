using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Dictamen
{
    public int IdDictamen { get; set; }

    public int IdActa { get; set; }

    public int IdDocente { get; set; }

    public string RutaDocumentoOriginal { get; set; } = null!;

    public string? RutaDocumentoFirmado { get; set; }

    public virtual Acta IdActaNavigation { get; set; } = null!;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;
}
