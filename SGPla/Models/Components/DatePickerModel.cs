namespace SGPla.Models.Components
{
    public class DatePickerModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty; // formato yyyy-MM-dd
        public string? Error { get; set; } = string.Empty;
        public string Min { get; set; } = string.Empty;
        public string Max { get; set; } = string.Empty;
        public bool Flexible { get; set; }
            
        public bool Disabled { get; set; }
    }
}