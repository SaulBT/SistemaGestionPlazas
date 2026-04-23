namespace SGPla.Models
{
    public class TimeFieldModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty; // HH:mm
        public string Min { get; set; } = string.Empty;
        public string Max { get; set; } = string.Empty;

        public bool Disabled { get; set; }
    }
}