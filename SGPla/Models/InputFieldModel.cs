public class InputFieldModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Tipo { get; set; } = "text";
    public string Valor { get; set; }
    public string Placeholder { get; set; }
    public string Label { get; set; }
    public bool Disabled { get; set; }
}