namespace SGPla.Helpers
{
    public class AccionesDisponibles
    {
        private readonly HashSet<string> _acciones;

        public AccionesDisponibles(HashSet<string> acciones)
        {
            _acciones = acciones;
        }

        public AccionesDisponibles()
        {
            _acciones = [];
        }

        public bool Puede(string accion) => _acciones.Contains(accion);
    }
}
