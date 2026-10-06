namespace SGPla.Commons;

public static class EstadosAviso
{
    public static readonly string[] Informativos =
        [Constantes.CREADO, Constantes.EN_REVISION_POR_DGAA, Constantes.DEVUELTO_POR_DGAA];

    public static readonly string[] RevisadosDgaa =
    [
        Constantes.AVALADO_POR_DGAA, Constantes.DEVUELTO_POR_DGAA,
        Constantes.FIRMADO, Constantes.PUBLICADO, Constantes.ACTA_DE_CT_CREADA
    ];

    public static readonly string[] RecibidosDgaa =
        [Constantes.EN_REVISION_POR_DGAA, .. RevisadosDgaa];

    // Estados cuyo documento vigente es el PDF firmado que adjuntó la entidad académica.
    public static readonly string[] Firmados =
        [Constantes.FIRMADO, Constantes.PUBLICADO, Constantes.ACTA_DE_CT_CREADA];

    public static bool EsInformativo(string estado) => Informativos.Contains(estado);

    public static bool EsFirmado(string estado) => Firmados.Contains(estado);
}
