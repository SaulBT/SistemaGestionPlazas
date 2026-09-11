namespace SGPla.Commons;

public static class EstadosAviso
{
    public static readonly string[] RevisadosDgaa =
    [
        Constantes.AVALADO_POR_DGAA, Constantes.DEVUELTO_POR_DGAA,
        Constantes.FIRMADO, Constantes.PUBLICADO, Constantes.ACTA_DE_CT_CREADA
    ];

    public static readonly string[] RecibidosDgaa =
        [Constantes.EN_REVISION_POR_DGAA, .. RevisadosDgaa];
}
