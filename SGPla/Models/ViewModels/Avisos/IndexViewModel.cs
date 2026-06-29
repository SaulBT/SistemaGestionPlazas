using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.Avisos
{
    public class IndexViewModel
    {
        public TableModel Table { get; set;  }
        public List<OptionModel> Periodos { get; set;  }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin {  get; set; }

        public int? IdPeriodoSeleccionado { get; set; }
        public string? Busqueda { get; set; }


        // Propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
