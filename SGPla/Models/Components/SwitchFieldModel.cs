namespace SGPla.Models.Components
{
    public class SwitchFieldModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;

        public bool Checked { get; set; }
        public bool Disabled { get; set; }

        public bool IsLabelVisible { get; set; } = true;
    }
}