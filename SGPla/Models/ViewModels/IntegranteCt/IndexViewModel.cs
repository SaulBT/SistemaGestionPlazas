

namespace SGPla.Models.ViewModels.IntegranteCt
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }

        public FormularioIntegranteViewModel Formulario { get; set; }

        public string? Busqueda { get; set; }

        public int PaginaActual { get; set; }

        public int CantidadPorPagina { get; set; }
    }
}
