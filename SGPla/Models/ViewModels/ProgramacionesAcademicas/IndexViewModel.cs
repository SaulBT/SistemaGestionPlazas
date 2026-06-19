using SGPla.Models.Components;

namespace SGPla.Models.ViewModels.ProgramacionesAcademicas
{
    public class IndexViewModel
    {
        public List<OptionModel> Regiones { get; set; }

        public List<OptionModel> Entidades {  get; set; }


        public int PaginaActual { get; set; } = 1;

        public int CantidadPorPagina { get; set; } = 10;

        public TableModel Table { get; set; }

        public string? Region { get; set; }

        public List<OptionModel> Periodos { get; set; }



    }
}
