namespace SGPla.Models
{
    public class TextAreaFieldModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;

        public int Rows { get; set; } = 4;

        public bool Disabled { get; set; }
    }
}