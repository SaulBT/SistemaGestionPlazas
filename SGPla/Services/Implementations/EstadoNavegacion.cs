using SGPla.Services.Interfaces;
using System.Text.Json;

namespace SGPla.Services.Implementations
{
    public static class ClavesEstado
    {
        public static class ProgramacionAcademica
        {
            public const string Region = "ProgAcademica.Region";
            public const string IdPeriodo = "ProgAcademica.IdPeriodo";
            public const string NombrePeriodo = "ProgAcademica.NombrePeriodo";
            public const string IdEntidadAcademica = "ProgAcademica.IdEntidadAcademica";
            public const string NombreEntidadAcademica = "ProgAcademica.NombreEntidadAcademica";
            public const string IdOfertaAsignar = "ProgAcademica.IdOfertaAsignar";
            public const string Ofertas = "ProgAcademica.Ofertas";
            public const string Cargas = "ProgAcademica.Cargas";
            public const string ResumenOferta = "ProgAcademica.ResumenOferta";
            public const string FiltroOfertaActual = "FiltroOfertaActual";
            public const string FiltroCargaActual = "FiltroCargaActual";
        }
    }

    public class EstadoNavegacion : IEstadoNavegacion
    {
        private const string CLAVE_PILA_RETORNO = "__pilaRetorno";

        private readonly IHttpContextAccessor _accessor;

        public EstadoNavegacion(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ISession Session =>
            _accessor.HttpContext?.Session
            ?? throw new InvalidOperationException("No hay HttpContext/Session disponible.");

        public T? Obtener<T>(string clave)
        {
            var json = Session.GetString(clave);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }

        public void Guardar<T>(string clave, T valor)
        {
            Session.SetString(clave, JsonSerializer.Serialize(valor));
        }

        public void Eliminar(string clave)
        {
            Session.Remove(clave);
        }

        public void PushRetorno(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            var pila = Obtener<List<string>>(CLAVE_PILA_RETORNO) ?? new List<string>();
            pila.Add(url);
            Guardar(CLAVE_PILA_RETORNO, pila);
        }

        public string PopRetorno(string fallback)
        {
            var pila = Obtener<List<string>>(CLAVE_PILA_RETORNO) ?? new List<string>();
            if (pila.Count == 0) return fallback;

            var url = pila[^1];
            pila.RemoveAt(pila.Count - 1);
            Guardar(CLAVE_PILA_RETORNO, pila);

            return string.IsNullOrWhiteSpace(url) ? fallback : url;
        }

        public string PeekRetorno(string fallback)
        {
            var pila = Obtener<List<string>>(CLAVE_PILA_RETORNO) ?? new List<string>();
            if (pila.Count == 0) return fallback;

            var url = pila[^1];
            return string.IsNullOrWhiteSpace(url) ? fallback : url;
        }
    }
}
