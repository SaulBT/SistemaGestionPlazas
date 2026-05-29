using SGPla.Models.Components;
using System.ComponentModel.DataAnnotations;

namespace SGPla.Models.ViewModels.PeriodosEscolares
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }

        public List<OptionModel> Periodos { get; set; }



        public string? AnioFiltro { get; set; }

        public string? Periodo { get; set; }


        public PeriodoEscolarFormularioViewModel Formulario { get; set; }

        // Propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
