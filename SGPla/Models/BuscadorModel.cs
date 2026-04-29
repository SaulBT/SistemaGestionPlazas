namespace SGPla.Models
{
    public class BuscadorModel
    {
        public string Name { get; set; } = "search";
        public string Placeholder { get; set; } = "Buscar...";
        public string Value { get; set; } = string.Empty;
        public string Action { get; set; } = "";
        public string Method { get; set; } = "get"; // solo "get" o "post"
        public string ButtonText { get; set; } = "";
    }
}
