using System;
using System.Collections.Generic;

namespace SGPla.Models;

public partial class Horario
{
    public int IdHorario { get; set; }

    public int? IdOferta { get; set; }

    public int? IdAviso { get; set; }

    public string Dia { get; set; } = null!;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string? Salon { get; set; }

    public virtual Aviso? IdAvisoNavigation { get; set; }

    public virtual Ofertum? IdOfertaNavigation { get; set; }
}
