using System.ComponentModel.DataAnnotations;

namespace SGPla.Commons
{
    public sealed class PlaneaOpciones
    {
        public const string Seccion = "Planea";
        public bool Habilitada { get; set; }
        [Required, Url] public string UrlBase { get; set; } = "https://planea.uv.mx/planea/index.php/apiroladoovr/";
        public TimeSpan Intervalo { get; set; } = TimeSpan.FromDays(1);
        public TimeSpan RetrasoInicial { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan TiempoEspera { get; set; } = TimeSpan.FromMinutes(5);
        [Range(1, 5)] public int Intentos { get; set; } = 3;
        public TimeSpan EsperaEntreIntentos { get; set; } = TimeSpan.FromSeconds(30);
        [Range(0, 24)] public int VentanaAnticipacionMeses { get; set; } = 6;
        public TimeSpan UmbralInterrumpida { get; set; } = TimeSpan.FromHours(1);
    }
}
