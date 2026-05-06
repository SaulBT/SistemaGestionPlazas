using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.PeriodosEscolares
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }

        public List<OptionModel> Periodos { get; set; }

        public int? Anio { get; set; }

        public string? Periodo { get; set; }


        public PeriodoEscolarFormularioViewModel Formulario { get; set; }


    }
}
