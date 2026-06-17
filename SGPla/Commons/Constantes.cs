namespace SGPla.Commons
{
    public class Constantes
    {
        //Metadatos
        public const string ID_ENTIDAD_ACADEMICA = "IdEntidadAcademica";
        public const string ID_AREA_ACADEMICA = "IdAreaAcademica";

        public const string MATERIA_EE = "MATERIA_EE";
        public const string CURSO_EE = "CURSO_EE";
        public const string DESC_EE = "DESC_EE";
        public const string PERFIL_DOC = "PERFIL_DOC";
        public const string HT_EE = "HT_EE";
        public const string HP_EE = "HP_EE";
        public const string CREDITOS_EE = "CREDITOS_EE";
        public static List<string> COLUMNAS_REQUERIDAS = [MATERIA_EE, CURSO_EE, DESC_EE, PERFIL_DOC, HT_EE, HP_EE, CREDITOS_EE];

        public const string SESSION_RUTA = "Ruta";
        public const string SESSION_NOMBRE_ARCHIVO = "NombreArchivo";


        // Mensajes tabla
        public const string ERROR_TABLA = "Error al generar la tabla de {0}.";
        public const string TABLA_VACIA = "No hay {0} para mostrar.";

        //Logs
        public const string LOGS_ESTRUCTURA = "{0}{1} {2}";
        public const string LOG_ERROR_INESPERADO = "Error inesperado.";
        public const string LOG_ERROR_VALIDACION = "Error de validación.";
        public const string LOG_ERROR_NULO = "El {0} es nulo";
        public const string LOG_ERROR_NULA = "La {0} es nula";

        //Toasts
        public const string TOAST_ERROR_GENERAL = "Ha ocurrido un error, inténtelo de nuevo más tarde.";
        public const string TOAST_ERROR_OBLIGATORIO = "El {0} es obligatorio.";
        public const string TOAST_ERROR_OBLIGATORIA = "La {0} es obligatoria.";
        public const string TOAST_ERROR_ELIMINACION_EL = "No se pudo eliminar el {0}, inténtelo nuevamente.";
        public const string TOAST_ERROR_ELIMINACION_LA = "No se pudo eliminar la {0}, inténtelo nuevamente.";
        public const string TOAST_ERROR_GUARDAR_EL = "No se pudo guardar el {0}, inténtelo nuevamente.";
        public const string TOAST_ERROR_GUARDAR_LA = "No se pudo guardar la {0}, inténtelo nuevamente.";

        public const string TOAST_ELIMINACION_EL = "El {0} ha sido eliminado con éxito.";
        public const string TOAST_ELIMINACION_LA = "La {0} ha sido eliminada con éxito.";
        public const string TOAST_GUARDADO_EL = "El {0} ha sido guardado con éxito.";
        public const string TOAST_GUARDADO_LA = "La {0} ha sido guardada con éxito.";

        //Elementos
        public const string PLAN_ESTUDIOS = "Plan de Estudios";
        public const string PLANES_ESTUDIOS = "Planes de Estudios";
        public const string EXPERIENCIA_EDUCATIVA = "Experiencia Educativa";
        public const string EXPERIENCIAS_EDUCATIVAS = "Experiencia Educativa";
        public const string ARTICULOS = "Artículos";
        public const string AREAS_ACADEMICAS = "Áreas Académicas";
        public const string ENTIDADES_ACADEMICAS = "Entidades Académicas";
        public const string PERIODOS_ESCOLARES = "Períodos Escolares";
        public const string PROGRAMAS_EDUCATIVOS = "Programas Educativos";
        public const string PERSONALES_ACADEMICOS = "Personales Académicos";
        public const string PERSONALES_EXTERNOS = "Personales Externos";

        public const string ARCHIVO = "Archivo";
        public const string RUTA_ARCHIVO = "Ruta del Archivo";
        public const string NOMBRE_ARCHIVO = "Nombre del Archivo";

        public const string USUARIOS = "Usuarios";
        public const string COORDINADOR_EA = "Coordinador de Entidad Académica";
        public const string COORDINADOR_DGAA = "Coordinador de Área Académica";
        public static List<string> ROLES = new List<string> { COORDINADOR_EA, COORDINADOR_DGAA };

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

        public const string DR = "Dr.";
        public const string DRA = "Dra.";
        public const string MTRO = "Mtro.";
        public const string MTRA = "Mtra.";
        public const string LIC = "Lic.";
        public static List<string> GRADOS = [LIC, MTRO, MTRA, DR, DRA];

        public const string LICENCIATURA = "Licenciatura";
        public const string DOCTORADO = "Doctorado";
        public const string MAESTRIA = "Maestria";
        public static List<string> GRADOS_DOCENTES = [LICENCIATURA, MAESTRIA, DOCTORADO];

        public const string INVESTIGADOR = "Investigador";
        public const string DOCENTE = "Docente";
        public const string TECNICO_ACADEMICO = "Técnico Académico";
        public const string DOCENTE_POR_ASIGNATURA = "Docente por Asignatura";
        public static List<string> PUESTOS = [INVESTIGADOR, DOCENTE, TECNICO_ACADEMICO, DOCENTE_POR_ASIGNATURA];
    }
}
