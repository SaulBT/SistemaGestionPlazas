namespace SGPla.Models.InterfacesDTOs
{
    public interface IProgramaEducativoDTO
    {
        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public int IdEntidadAcademica { get; set; }

        public string Campus { get; set; }
    }
}
