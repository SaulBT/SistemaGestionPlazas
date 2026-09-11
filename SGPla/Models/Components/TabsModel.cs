namespace SGPla.Models.Components
{
    public class TabsModel
    {
        public string Id { get; set; }

        public List<TabItemModel> Tabs { get; set; } = new();

        public int TabActivaPorDefecto { get; set; } = 0;
    }

    public class TabItemModel
    {
        public string Titulo { get; set; }

        public string? EstadoClase { get; set; }
        public int? Cantidad { get; set; }

        public string? IconClass { get; set; }

        public string PartialView { get; set; }

        public object? PartialModel { get; set; }
    }
}