namespace SGPla.Commons
{
    public class Constantes
    {
        public const string COORDINADOR_EA = "Coordinador de Entidad Académica";
        public const string COORDINADOR_DGAA = "Coordinador de Área Académica";
        public static List<string> ROLES = new List<string> { COORDINADOR_EA, COORDINADOR_DGAA };

        public const string ID_ENTIDAD_ACADEMICA = "IdEntidadAcademica";
        public const string ID_AREA_ACADEMICA = "IdAreaAcademica";

        public const string REGION_XALAPA = "1-Xalapa";
        public const string REGION_VERACRUZ = "2-Veracruz";
        public const string REGION_ORIZABA = "3-Orizaba-Córdoba";
        public const string REGION_POZARICA_TUXPAN = "4-Poza Rica-Túxpan";
        public const string REGION_COATZACOALCOS_MINATITLAN = "5-Coatzacoalcos-Minatitlán";
        public static List<string> REGIONES = new List<string> { REGION_XALAPA, REGION_VERACRUZ, REGION_ORIZABA, REGION_POZARICA_TUXPAN, REGION_COATZACOALCOS_MINATITLAN };

        public const string MODALIDAD_ESCOLARIZADA = "Escolarizado";
        public const string MODALIDAD_ABIERTA = "Abierto";
        public const string MODALIDAD_VIRTUAL = "Virtual";
        public const string MODALIDAD_MIXTA = "Mixta";
        public const string MODALIDAD_SEMIESCOLARIZADA = "Semi escolarizado";
        public const string MODALIDAD_DISTANCIA = "A distancia";
        public static List<string> MODALIDADES = new List<string> { MODALIDAD_ESCOLARIZADA, MODALIDAD_ABIERTA, MODALIDAD_VIRTUAL, MODALIDAD_MIXTA, MODALIDAD_SEMIESCOLARIZADA, MODALIDAD_DISTANCIA };

        // Mensajes tabla
        public static string ERROR_TABLA = "Error al generar la tabla {tabla}.";
        public static string TABLA_VACIA = "No hay {elemento} para mostrar.";

        //Logs
        public static string LOGS_ESTRUCTURA = "{Ubicacion}{Metodo} {Log}";
        public static string LOG_ERROR_INESPERADO = "Error inesperado.";
        public static string LOG_ERROR_VALIDACION = "Error de validación.";

        //Toasts
        public static string TOAST_ERROR_GENERAL = "Ha ocurrido un error, inténtelo de nuevo más tarde.";
        public static string TOAST_ERROR_OBLIGATORIO = "El {elemento} es obligatorio.";
        public static string TOAST_ERROR_OBLIGATORIA = "La {elemento} es obligatoria.";
        public static string TOAST_ERROR_ELIMINACION_EL = "No se pudo eliminar el {elemento}, inténtelo nuevamente.";
        public static string TOAST_ERROR_ELIMINACION_LA = "No se pudo eliminar la {elemento}, inténtelo nuevamente.";
        public static string TOAST_ERROR_GUARDAR_EL = "No se pudo guardar el {elemento}, inténtelo nuevamente.";
        public static string TOAST_ERROR_GUARDAR_LA = "No se pudo guardar la {elemento}, inténtelo nuevamente.";

        public static string TOAST_ELIMINACION_EL = "El {elemento} ha sido eliminado con éxito.";
        public static string TOAST_ELIMINACION_LA = "La {elemento} ha sido eliminada con éxito.";
        public static string TOAST_GUARDADO_EL = "El {elemento} ha sido guardado con éxito.";
        public static string TOAST_GUARDADO_LA = "La {elemento} ha sido guardada con éxito.";

        //Elementos
        public static string PLAN_ESTUDIOS = "Plan de Estudios";
        public static string EXPERIENCIA_EDUCATIVA = "Experiencia Educativa";
    }
}
