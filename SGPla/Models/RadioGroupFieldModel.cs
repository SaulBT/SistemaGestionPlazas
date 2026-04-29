namespace SGPla.Models
{
    public class RadioGroupFieldModel
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public string SelectedValue { get; set; } = string.Empty;

        public bool Disabled { get; set; }

        public List<RadioOptionModel> Options { get; set; } = new();
    }

    public class RadioOptionModel
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}