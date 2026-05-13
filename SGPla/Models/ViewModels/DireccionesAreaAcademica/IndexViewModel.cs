namespace SGPla.Models.ViewModels.DireccionesAreaAcademica
{
    public class IndexViewModel
    {
        public TableModel Table { get; set; }
        public string? Busqueda { get; set; }

        //paginación
        public int PaginaActual { get; set; } = 1;
        public int CantidadPorPagina { get; set; } = 10;
    }
}
