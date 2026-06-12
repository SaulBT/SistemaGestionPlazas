using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class CargarProgramacionAcademica1ViewModel
    {
        public TableModel Table { get; set; }
        public List<OptionModel> Regiones { get; set; }

        public List<OptionModel> Periodos { get; set; }
        public List<OptionModel> Entidades { get; set; }
        public string? RegionSeleccionada { get; set; }


        public int? IdPeriodoSeleccionado { get; set; }
        public int? IdEntidadSeleccionada { get; set; }
        public string? Busqueda { get; set; }

        // Propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;

        public string? Region { get; set; }

        public int? IdEntidadAcademica { get; set; }

        public int? IdPeriodo { get; set; }

        public IFormFile? ArchivoVacantes { get; set; }
        public IFormFile? ArchivoDescargas { get; set; }

    }
}
