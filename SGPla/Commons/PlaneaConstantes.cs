namespace SGPla.Commons
{
    public static class PlaneaConstantes
    {
        public const string ESTADO_EN_PROCESO = "EnProceso";
        public const string ESTADO_EXITOSA = "Exitosa";
        public const string ESTADO_SIN_DATOS = "SinDatos";
        public const string ESTADO_OMITIDA = "Omitida";
        public const string ESTADO_FALLIDA = "Fallida";
        public const string ESTADO_INTERRUMPIDA = "Interrumpida";
        public static readonly IReadOnlyList<string> ESTADOS = [ESTADO_EN_PROCESO, ESTADO_EXITOSA, ESTADO_SIN_DATOS, ESTADO_OMITIDA, ESTADO_FALLIDA, ESTADO_INTERRUMPIDA];
        public static readonly IReadOnlyList<string> DIAS = ["Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"];
        public const string APROBACION_PENDIENTE = "Pendiente";
        public const string APROBACION_APROBADA = "Aprobada";
        public const string APROBACION_DESCARTADA = "Descartada";
        public const int LONGITUD_NRC = 5;
        public const int LONGITUD_CODIGO_EE = 10;
        public const int LONGITUD_CODIGO_PLAN = 50;
        public const int LONGITUD_TITULO = 150;
    }
}
