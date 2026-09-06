namespace SGPla.Models;

public partial class Modalidad
{
    public int IdModalidad { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activa { get; set; }
}
