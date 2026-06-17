namespace SGPla.Models.ViewModels.Docentes
{
    public class IndexViewModel
    {
        public TabIndexViewModel TabDocentes { get; set; } = new TabIndexViewModel();
        public TabIndexViewModel TabAspirantes { get; set; } = new TabIndexViewModel();
        public int TabActivada { get; set; }
    }
}
