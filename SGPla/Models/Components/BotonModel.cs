public class BotonModel
{
    public string Tipo { get; set; }      // Primario, Secundario, Terciario
    public string Accion { get; set; }    // guardar, cancelar, regresar, siguiente, buscar, agregar, editar, archivar, imprimir, confirmar, enviar, cargar, firmar
    public string Icono { get; set; }
    public string Texto { get; set; }
    public bool Disabled { get; set; }
    public bool Fondo { get; set; }
}