namespace SGPla.Repositories.Implementations
{
    internal static class SincronizacionPlaneaSql
    {
        public const string CrearTablasTemporales = """
            DROP TABLE IF EXISTS #CopiaPlanea;
            DROP TABLE IF EXISTS #HorarioPlanea;
            DROP TABLE IF EXISTS #DocentePlanea;

            CREATE TABLE #CopiaPlanea (
                nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
                codigoExperiencia varchar(10) COLLATE DATABASE_DEFAULT NOT NULL,
                codigoPlan varchar(50) COLLATE DATABASE_DEFAULT NOT NULL,
                titulo varchar(150) COLLATE DATABASE_DEFAULT NOT NULL,
                campus varchar(5) COLLATE DATABASE_DEFAULT NULL,
                nivel varchar(5) COLLATE DATABASE_DEFAULT NULL,
                region varchar(50) COLLATE DATABASE_DEFAULT NULL,
                area varchar(100) COLLATE DATABASE_DEFAULT NULL,
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
            CREATE TABLE #DocentePlanea (
                nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL,
                numeroPersonal varchar(15) COLLATE DATABASE_DEFAULT NULL,
                nombre varchar(150) COLLATE DATABASE_DEFAULT NOT NULL,
                imparte bit NULL
            );
            CREATE INDEX IX_DocentePlanea_nrc ON #DocentePlanea(nrc);
            """;

        // El enlace con la EE y el plan se resuelve después de registrar, con EnlacePlaneaSql.
        public const string ResolverReferencias = """
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
            -- Se registran todos los NRC con plan aunque su EE o su plan aún no estén en el catálogo:
            -- quedan pendientes y se enlazan en local al cargar el plan de estudios.
            INSERT INTO dbo.ExperienciaEducativaPeriodo
                (idExperienciaEducativa, idPeriodo, idPlanEstudios, idRegion, idSincronizacionPlanea,
                 nrc, codigoExperiencia, codigoPlan, titulo, campus, nivel, area, fechaAlta)
            OUTPUT inserted.idExperienciaEducativaPeriodo, inserted.nrc INTO @nuevas (idExperienciaEducativaPeriodo, nrc)
            SELECT NULL, @idPeriodo, NULL, c.idRegion, @idSincronizacion,
                   c.nrc, c.codigoExperiencia, c.codigoPlan, c.titulo, c.campus, c.nivel, c.area, @ahora
            FROM #CopiaPlanea AS c
            WHERE NOT EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t WITH (UPDLOCK, HOLDLOCK)
                              WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc);
            INSERT INTO dbo.ExperienciaEducativaPeriodoHorario
                (idExperienciaEducativaPeriodo, idHorarioPlanea, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin)
            SELECT n.idExperienciaEducativaPeriodo, h.idHorario, h.dia, h.horaInicio, h.horaFin,
                   h.edificio, h.aula, h.fechaInicio, h.fechaFin
            FROM #HorarioPlanea AS h
            INNER JOIN @nuevas AS n ON n.nrc = h.nrc;
            SET @horarios = @@ROWCOUNT;
            -- Los docentes cambian durante el periodo: se reemplazan en todos los NRC recibidos,
            -- también en los ya registrados.
            DELETE d
            FROM dbo.ExperienciaEducativaPeriodoDocente AS d
            INNER JOIN dbo.ExperienciaEducativaPeriodo AS t
                ON t.idExperienciaEducativaPeriodo = d.idExperienciaEducativaPeriodo
            WHERE t.idPeriodo = @idPeriodo
              AND EXISTS (SELECT 1 FROM #CopiaPlanea AS c WHERE c.nrc = t.nrc);
            INSERT INTO dbo.ExperienciaEducativaPeriodoDocente
                (idExperienciaEducativaPeriodo, numeroPersonal, nombre, imparte)
            SELECT t.idExperienciaEducativaPeriodo, d.numeroPersonal, d.nombre, d.imparte
            FROM #DocentePlanea AS d
            INNER JOIN dbo.ExperienciaEducativaPeriodo AS t
                ON t.idPeriodo = @idPeriodo AND t.nrc = d.nrc;
            """
            // Enlaza con el catálogo las copias pendientes del periodo, nuevas o de sincronizaciones anteriores.
            + EnlacePlaneaSql.EnlazarPendientes
            + """
            DECLARE @sinExperiencia int = (
                SELECT COUNT(*) FROM #CopiaPlanea AS c
                INNER JOIN dbo.ExperienciaEducativaPeriodo AS t ON t.idPeriodo = @idPeriodo AND t.nrc = c.nrc
                WHERE t.idExperienciaEducativa IS NULL);
            SELECT (SELECT COUNT(*) FROM @nuevas) AS nuevos,
                   @existentes AS existentes,
                   @sinExperiencia AS sinExperiencia,
                   @horarios AS horarios;
            """;
    }
}
