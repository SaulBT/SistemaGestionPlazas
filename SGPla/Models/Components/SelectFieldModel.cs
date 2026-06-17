namespace SGPla.Models.Components
{
    public class SelectFieldModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Disabled { get; set; }
        public string? Error {  get; set; }
        public string? SelectedValue { get; set; }
        public string Placeholder { get; set; } = "Selecciona una opción";
        public bool Flexible { get; set; }

        public List<OptionModel> Options { get; set; } = new();
        public Dictionary<string, string>? HtmlAttributes { get; set; }
        public string? ContainerStyle { get; set; } = string.Empty;

    }

    public class OptionModel
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
