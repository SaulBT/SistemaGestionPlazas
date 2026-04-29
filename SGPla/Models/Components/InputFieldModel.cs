public class InputFieldModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Tipo { get; set; } = "text";
    public string Valor { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Disabled { get; set; }
    public bool Flexible { get; set; }
    public string? Error { get; set; }
}