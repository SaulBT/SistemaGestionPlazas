using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Horario
{
    public int idHorario { get; set; }

    public int? idOferta { get; set; }

    public int? idAviso { get; set; }

    public string dia { get; set; } = null!;

    public TimeOnly horaInicio { get; set; }

    public TimeOnly horaFin { get; set; }

    public string? salon { get; set; }

    public virtual Aviso? idAvisoNavigation { get; set; }

    public virtual Oferta? idOfertaNavigation { get; set; }
}
