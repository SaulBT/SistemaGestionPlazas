namespace SGPla.Commons
{
    public class Constantes
    {
        public const string CoordinadorEa = "Coordinador de Entidad Académica";
        public const string CoordinadorDgaa = "Coordinador de Área Académica";
        public static List<string> Roles = new List<string> { CoordinadorEa, CoordinadorDgaa };

        public const string IdEntidadAcademica = "IdEntidadAcademica";
        public const string IdAreaAcademica = "IdAreaAcademica";

        public const string RegionXalapa = "1-Xalapa";
        public const string RegionVeracruz = "2-Veracruz";
        public const string RegionOrizabaCordoba = "3-Orizaba-Córdoba";
        public const string RegionPozaRicaTuxpan = "4-Poza Rica-Túxpan";
        public const string RegionCotazacoalcosMinatitlan = "5-Coatzacoalcos-Minatitlán";
        public static List<string> Regiones = new List<string> { RegionXalapa, RegionVeracruz, RegionOrizabaCordoba, RegionPozaRicaTuxpan, RegionCotazacoalcosMinatitlan };
    }
}
