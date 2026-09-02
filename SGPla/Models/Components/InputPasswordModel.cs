public class InputPasswordModel
{
    public string Id { get; set; } = "password";
    public string Name { get; set; } = "password";
    public string? Label { get; set; }
    public string? Valor { get; set; }
    public string Placeholder { get; set; } = "";
    public string? Error { get; set; }
    public bool Disabled { get; set; }
    public bool Flexible { get; set; }
    public int? MaximoCaracteres { get; set; }
}