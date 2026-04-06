using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Dictamen
{
    public int idDictamen { get; set; }

    public int idActa { get; set; }

    public int idDocente { get; set; }

    public string rutaDocumentoOriginal { get; set; } = null!;

    public string? rutaDocumentoFirmado { get; set; }

    public virtual Acta idActaNavigation { get; set; } = null!;

    public virtual Docente idDocenteNavigation { get; set; } = null!;
}
