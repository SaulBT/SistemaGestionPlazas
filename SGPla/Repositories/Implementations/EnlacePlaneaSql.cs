namespace SGPla.Repositories.Implementations
{
    /// SQL para enlazar en local las copias PLANEA (NRC) con el catálogo, sin consultar PLANEA.
    /// Lo comparten la sincronización y la carga de planes de estudio.
    internal static class EnlacePlaneaSql
    {
        /// Enlaza las copias pendientes con la EE de su plan y región.
        /// @idPeriodo y @idPlanEstudios son opcionales y acotan las copias a revisar.
        public const string EnlazarPendientes = """
            UPDATE copia
            SET idExperienciaEducativa = x.idExperienciaEducativa,
                idPlanEstudios = x.idPlanEstudios
            FROM dbo.ExperienciaEducativaPeriodo AS copia
            CROSS APPLY (
                SELECT TOP (1) ee.idExperienciaEducativa, ee.idPlanEstudios
                FROM dbo.ExperienciaEducativa AS ee
                INNER JOIN dbo.PlanEstudios AS pl ON pl.idPlanEstudios = ee.idPlanEstudios
                INNER JOIN dbo.ProgramaEducativo AS pe ON pe.idProgramaEducativo = pl.idProgramaEducativo
                INNER JOIN dbo.EntidadAcademica AS ea ON ea.idEntidadAcademica = pe.idEntidadAcademica
                -- PLANEA repite el código de plan en varias regiones: la copia solo se enlaza
                -- con el plan de una entidad de su misma región.
                WHERE pl.codigoPlan = copia.codigoPlan
                  AND ee.codigo = copia.codigoExperiencia
                  AND ea.idRegion = copia.idRegion
                  AND pe.fechaEliminacion IS NULL
                  AND ea.fechaEliminacion IS NULL
                  AND (@idPlanEstudios IS NULL OR pl.idPlanEstudios = @idPlanEstudios)
                ORDER BY ee.idExperienciaEducativa
            ) AS x
            WHERE copia.idExperienciaEducativa IS NULL
              AND (@idPeriodo IS NULL OR copia.idPeriodo = @idPeriodo)
              AND (@idPlanEstudios IS NULL OR copia.codigoPlan =
                   (SELECT codigoPlan FROM dbo.PlanEstudios WHERE idPlanEstudios = @idPlanEstudios));
            """;

        /// Deja pendientes las copias de un plan cuya EE ya no existe o cambió de código o de plan.
        public const string DesenlazarDesactualizadas = """
            UPDATE copia
            SET idExperienciaEducativa = NULL,
                idPlanEstudios = NULL
            FROM dbo.ExperienciaEducativaPeriodo AS copia
            LEFT JOIN dbo.ExperienciaEducativa AS ee ON ee.idExperienciaEducativa = copia.idExperienciaEducativa
            LEFT JOIN dbo.PlanEstudios AS pl ON pl.idPlanEstudios = ee.idPlanEstudios
            WHERE copia.idPlanEstudios = @idPlanEstudios
              AND (ee.idExperienciaEducativa IS NULL
                   OR ee.idPlanEstudios <> copia.idPlanEstudios
                   OR ee.codigo <> copia.codigoExperiencia
                   OR pl.codigoPlan IS NULL
                   OR pl.codigoPlan <> copia.codigoPlan);
            """;
    }
}
