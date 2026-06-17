namespace SGPla.Models.DTOs.Oferta
{
    public class HorarioDia
    {
        public TimeSpan Inicio { get; init; }
        public TimeSpan Fin { get; init; }

        public override string ToString() =>
            $"{Inicio:hh\\:mm}-{Fin:hh\\:mm}";
    }

    public class OfertaDTO
    {
        public string Programa { get; set; } = "";
        public string NRC { get; set; } = "";
        public string ExperienciaEducativa { get; set; } = "";
        public int HorasPago { get; set; }
        public string Plaza { get; set; } = "";

        public string? TC { get; set; }

        public HorarioDia? Lunes { get; set; }
        public HorarioDia? Martes { get; set; }
        public HorarioDia? Miercoles { get; set; }
        public HorarioDia? Jueves { get; set; }
        public HorarioDia? Viernes { get; set; }
        public HorarioDia? Sabado { get; set; }

        public string? NP { get; set; }
        public string? NombreDocente { get; set; }
        public string? TipoIngreso { get; set; }


        public int Articulo { get; set; }

        public int Incluida { get; set; } = 0;

        public IEnumerable<(string Dia, HorarioDia Horario)> DiasConClase()
        {
            if (Lunes is not null) yield return ("Lunes", Lunes);
            if (Martes is not null) yield return ("Martes", Martes);
            if (Miercoles is not null) yield return ("Miércoles", Miercoles);
            if (Jueves is not null) yield return ("Jueves", Jueves);
            if (Viernes is not null) yield return ("Viernes", Viernes);
            if (Sabado is not null) yield return ("Sábado", Sabado);
        }

        public int IdPeriodo { get; set; }

        public int IdProgramaEducativo { get; set; }


    }
}
