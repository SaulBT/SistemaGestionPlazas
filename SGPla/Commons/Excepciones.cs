namespace SGPla.Commons
{
    public class ValidacionExcepction : Exception
    {
        public string Codigo { get; }

        public ValidacionExcepction(string mensaje, string codigo) : base(mensaje)
        {
            Codigo = codigo;
        }
    }
}
