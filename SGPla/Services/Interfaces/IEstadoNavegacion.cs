namespace SGPla.Services.Interfaces
{
    public interface IEstadoNavegacion
    {
        T? Obtener<T>(string clave);
        void Guardar<T>(string clave, T valor);
        void Eliminar(string clave);

        void PushRetorno(string url);
        string PopRetorno(string fallback);
        string PeekRetorno(string fallback);
    }
}
