namespace SGPla.Repositories.Implementations
{
    internal static class SincronizacionPlaneaSql
    {
        public const string CrearTablasTemporales = """
            DROP TABLE IF EXISTS #CopiaPlanea;
            DROP TABLE IF EXISTS #HorarioPlanea;

            CREATE TABLE #CopiaPlanea (
                nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
                codigoExperiencia varchar(10) COLLATE DATABASE_DEFAULT NOT NULL,
                codigoPlan varchar(50) COLLATE DATABASE_DEFAULT NOT NULL,
                titulo varchar(150) COLLATE DATABASE_DEFAULT NOT NULL,
                campus varchar(5) COLLATE DATABASE_DEFAULT NULL,
                nivel varchar(5) COLLATE DATABASE_DEFAULT NULL,
                region varchar(50) COLLATE DATABASE_DEFAULT NULL,
                area varchar(100) COLLATE DATABASE_DEFAULT NULL,
                idExperienciaEducativa int NULL,
                idPlanEstudios int NULL,
                idRegion int NULL
            );
            CREATE TABLE #HorarioPlanea (
                idHorario int NOT NULL,
                nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL,
                dia varchar(10) COLLATE DATABASE_DEFAULT NOT NULL,
                horaInicio time(0) NOT NULL,
                horaFin time(0) NOT NULL,
                edificio varchar(50) COLLATE DATABASE_DEFAULT NULL,
                aula varchar(100) COLLATE DATABASE_DEFAULT NULL,
                fechaInicio date NULL,
                fechaFin date NULL
            );
            CREATE INDEX IX_HorarioPlanea_nrc ON #HorarioPlanea(nrc);
            """;

        public const string ResolverReferencias = """
            UPDATE c SET idExperienciaEducativa = x.idExperienciaEducativa,
                         idPlanEstudios = x.idPlanEstudios
            FROM #CopiaPlanea AS c
            CROSS APPLY (
                SELECT TOP (1) ee.idExperienciaEducativa, ee.idPlanEstudios
                FROM dbo.ExperienciaEducativa AS ee
                INNER JOIN dbo.PlanEstudios AS pl ON pl.idPlanEstudios = ee.idPlanEstudios
                INNER JOIN dbo.ProgramaEducativo AS pe ON pe.idProgramaEducativo = pl.idProgramaEducativo
                WHERE pl.codigoPlan = c.codigoPlan
                  AND ee.codigo = c.codigoExperiencia
                  AND pe.fechaEliminacion IS NULL
                ORDER BY ee.idExperienciaEducativa
            ) AS x;

            UPDATE c SET idRegion = r.id
            FROM #CopiaPlanea AS c
            INNER JOIN dbo.Region AS r
                ON r.nombre COLLATE Latin1_General_CI_AI = c.region COLLATE Latin1_General_CI_AI;
            """;

        public const string RegistrarNuevas = """
            DECLARE @ahora datetime2(0) = SYSDATETIME();
            DECLARE @horarios int = 0;
            DECLARE @nuevas TABLE (
                idExperienciaEducativaPeriodo int NOT NULL,
                nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY
            );
            DECLARE @existentes int = (
                SELECT COUNT(*) FROM #CopiaPlanea AS c
                WHERE EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t
                              WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc));
            DECLARE @sinExperiencia int = (
                SELECT COUNT(*) FROM #CopiaPlanea AS c
                WHERE c.idExperienciaEducativa IS NULL
                  AND NOT EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t
                                  WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc));
            INSERT INTO dbo.ExperienciaEducativaPeriodo
                (idExperienciaEducativa, idPeriodo, idPlanEstudios, idRegion, idSincronizacionPlanea,
                 nrc, titulo, campus, nivel, area, fechaAlta)
            OUTPUT inserted.idExperienciaEducativaPeriodo, inserted.nrc INTO @nuevas (idExperienciaEducativaPeriodo, nrc)
            SELECT c.idExperienciaEducativa, @idPeriodo, c.idPlanEstudios, c.idRegion, @idSincronizacion,
                   c.nrc, c.titulo, c.campus, c.nivel, c.area, @ahora
            FROM #CopiaPlanea AS c
            WHERE c.idExperienciaEducativa IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t WITH (UPDLOCK, HOLDLOCK)
                              WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc);
            INSERT INTO dbo.ExperienciaEducativaPeriodoHorario
                (idExperienciaEducativaPeriodo, idHorarioPlanea, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin)
            SELECT n.idExperienciaEducativaPeriodo, h.idHorario, h.dia, h.horaInicio, h.horaFin,
                   h.edificio, h.aula, h.fechaInicio, h.fechaFin
            FROM #HorarioPlanea AS h
            INNER JOIN @nuevas AS n ON n.nrc = h.nrc;
            SET @horarios = @@ROWCOUNT;
            SELECT (SELECT COUNT(*) FROM @nuevas) AS nuevos,
                   @existentes AS existentes,
                   @sinExperiencia AS sinExperiencia,
                   @horarios AS horarios;
            """;
    }
}
