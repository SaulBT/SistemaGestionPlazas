# PLAN_PLANEA — Sincronización automática de EEs/NRC desde PLANEA

> **Para el agente implementador.** Este documento es autocontenido. Síguelo en orden, fase por fase.
> Al terminar cada fase, ejecuta su **checkpoint**. No avances a la siguiente fase si el checkpoint falla.
> Las decisiones de diseño de la §1 ya están tomadas con el usuario: **no las cambies**.
> Si algo del código real no coincide con lo que dice este plan (un nombre, una línea), **investígalo en el código**, adáptate y anótalo en tu resumen final.

---

## 0. Reglas de trabajo

- **Raíz del repo:** `/Users/kaleb/repos/sgpla/SistemaGestionPlazas`. Rama base: `develop`.
  - Crea la rama `feature/sincronizacion-planea` antes del primer commit.
- **No hagas push ni abras PR** sin que el usuario lo pida.
  - Commits en español, estilo convencional: `feat(db): …`, `feat(planea): …`, `test(planea): …`.
  - Termina cada mensaje de commit con la línea `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.
- **Stack:**
  - .NET 10 (SDK 10.0.400) y ASP.NET Core MVC.
  - EF Core 10 Database First sobre SQL Server. Las migraciones son SQL y las aplica **DbUp** (`SGPla.DbMigrator`).
  - Pruebas: xUnit 2.9 + Moq 4.20.
  - `Nullable` está habilitado en ambos proyectos.
- **No agregues paquetes NuGet.** Todo lo necesario ya está disponible:
  - `Microsoft.Data.SqlClient`, transitivo de `Microsoft.EntityFrameworkCore.SqlServer`.
  - `AddHttpClient`, `IOptions` y `ValidateDataAnnotations`, del framework compartido de ASP.NET Core.
  - `System.Text.Json`.
- **Convenciones del código** (imítalas):
  - Nombres en español y clases `sealed`.
  - `CancellationToken cancellationToken` en todos los métodos async.
  - Módulos con la estructura de `SGPla/Modules/PeriodosEscolares/` (`Api/`, `Application/`, `Domain/`, `Infra/`, `XxxModuleExtensions.cs`).
  - Namespaces `SGPla.Modules.<Modulo>.<Capa>...`.
- **Comandos:**
  - Compilar: `dotnet build SGPla/SGPla.csproj`
  - Probar: `dotnet test SGPla.Tests/SGPla.Tests.csproj`
  - Compilar el migrador: `dotnet build SGPla.DbMigrator/SGPla.DbMigrator.csproj`
- **Archivo de datos real** para análisis y fixtures: `/Users/kaleb/repos/sgpla/Endpoint.json` (65 MB, fuera del repo). **No lo copies al repo.**

---

## 1. Contexto y decisiones (NO modificar)

PLANEA expone `GET https://planea.uv.mx/planea/index.php/apiroladoovr/periodo/{codigoPeriodo}` (ej. `202701`). La respuesta pesa unos 65 MB y tiene unos 1.9 M de líneas. Queremos guardar su contenido en nuestra BD para no consultar el servicio en cada uso, leerlo de forma eficiente y **sin duplicados**.

| Tema | Decisión |
|---|---|
| EE base | `ExperienciaEducativa` es la EE base del catálogo y **NO lleva NRC** |
| NRC | Nueva tabla ternaria **EE × Periodo × PlanEstudios**: `ExperienciaEducativaPeriodo`, con clave única `(idPeriodo, nrc)` |
| Plan del NRC | FK **opcional** a `PlanEstudios` (por `codigoPlan`) + el código crudo de PLANEA en `codigoPlanPlanea` |
| EE que no existe en el catálogo | Se crea con datos mínimos (código y nombre), `idPlanEstudios = NULL` y `origen = 'PLANEA'`. `idPlanEstudios`, `perfilDocente`, `creditos` y `horas` pasan a ser opcionales |
| Alcance | NRC + docentes asignados por NRC + horarios por NRC |
| Re-sincronización | Upsert. Los NRC que ya no vienen reciben **baja lógica** (`fechaBaja`); si reaparecen, se reactivan. Los docentes y horarios de cada NRC se reemplazan por lo que trae PLANEA (se insertan, actualizan o borran) |
| Disparo | **Automático programado** (`BackgroundService`), sin pantalla ni endpoint |
| Qué periodos | **Vigentes por fecha**: `fechaFin >= hoy` y `fechaInicio <= hoy + VentanaAnticipacionMeses` |
| Fechas de `Periodo` | Están mal calculadas (ver §10), pero **su corrección está fuera de alcance** |

---

## 2. Perfil de la respuesta de PLANEA (medido sobre `Endpoint.json`)

- **Raíz**, en este orden: `periodo` (string), `total` (**número JSON**), `resultado` (array de 18,203) y `horarios` (array de 22,623). El resto de los valores son **strings** o `null`.
- **`resultado`**
  - Cada fila es la asignación de un docente a un NRC, con `radoc_id` único.
  - Hay **18,022 NRC distintos**; 152 NRC tienen de 2 a 4 filas (varios docentes).
  - **5,619 filas no tienen docente**: `ID_TITULAR` es null y `radoc_nombre` está en blanco.
  - Los campos de sección nunca difieren entre filas del mismo NRC: `Region`, `Area`, `sec_programa`, `sec_campus`, `nivel`, `sec_titulo`, `radoc_materia` y `radoc_curso`.
- **Código de EE:** `radoc_materia + " " + radoc_curso`, por ejemplo `CVCB 18003`.
  - Coincide con el formato del catálogo `^[A-Z]{4} [0-9]{5}$`.
  - Excepción: `MVZ 80001` y `MVZ 80002`, con materia de 3 letras.
  - Hay 4,476 códigos distintos. En 343 el título viene truncado en algunas filas (`DERECHO CONSTITUCIONAL MEX` vs `DERECHO CONSTITUCIONAL MEXICANO`).
- **`sec_programa`** (ej. `CIVI-20-E-CR`, máximo 12 caracteres) es `PlanEstudios.codigoPlan`. Viene `null` en 900 NRC.
- **`horarios`**
  - Cada fila es un bloque con `rhs_id` único y las columnas `LUN_INI`, `LUN_FIN`, `MAR_*`, `MIE_*`, `JUE_*`, `VIE_*`, `SAB_*` en formato `"HHmm"`. No hay domingo.
  - Todas las horas son válidas y siempre `FIN > INI`.
  - 1,144 bloques no tienen ningún día. 3 NRC de `horarios` no están en `resultado` (`25108`, `49268`, `93562`).
  - Expandidos por día producen unas 38,800 filas.
- **Longitudes máximas:**
  - `sec_titulo` 108, `Area` 53, `Region` 24, `radoc_nombre` 91.
  - `radoc_textopuesto` 31, `radoc_textocontratacion` 31.
  - `EDIFICIO` 7, `AULA` 10, `ID_TITULAR` 9, `radoc_plaza` 5, `radoc_puesto` 4.
  - `sec_campus` 2 y `nivel` 2.
- **Formatos:**
  - `radoc_nhoras` es decimal (`"4.5"`).
  - `ID_TITULAR` = `E00029548`, mientras que `Docente.numeroPersonal` es numérico (`29548`; ver `SGPla/Parsers/CargasParser.cs:82`).
  - `radoc_plaza` puede ser `""`. `radoc_tipocontratacion` puede ser `"-"` y `radoc_textocontratacion` puede ser `"---"`.
- **Regiones:** llegan como `XALAPA`, `VERACRUZ`, `ORIZABA-CÓRDOBA`, `POZA RICA-TUXPAN` y `COATZACOALCOS-MINATITLÁN`. `dbo.Region.nombre` las guarda como `Xalapa`, `Poza Rica-Túxpan`, etc., así que se comparan con `COLLATE Latin1_General_CI_AI`.
- **Días en BD:** usa los mismos nombres que `dbo.Horario`: `"Lunes"`, `"Martes"`, `"Miercoles"`, `"Jueves"`, `"Viernes"` y `"Sabado"`, **sin acentos** (ver `SGPla/Repositories/Implementations/ProgramacionAcademicaRepository.cs:241-246`).

---

## 3. Mapa de archivos

### Crear
```
SGPla.DbMigrator/database/migrations/0012_sincronizacion_planea.sql

SGPla/Models/ExperienciaEducativaPeriodo.cs
SGPla/Models/ExperienciaEducativaPeriodoDocente.cs
SGPla/Models/ExperienciaEducativaPeriodoHorario.cs
SGPla/Models/SincronizacionPlanea.cs

SGPla/Modules/SincronizacionPlanea/SincronizacionPlaneaModuleExtensions.cs
SGPla/Modules/SincronizacionPlanea/Application/PlaneaOpciones.cs
SGPla/Modules/SincronizacionPlanea/Application/Models/PlaneaRespuesta.cs          (DTOs JSON)
SGPla/Modules/SincronizacionPlanea/Application/Models/PlaneaModelos.cs            (records normalizados)
SGPla/Modules/SincronizacionPlanea/Application/Contracts/ResumenAplicacionPlanea.cs
SGPla/Modules/SincronizacionPlanea/Application/Contracts/ResultadoSincronizacionPlanea.cs
SGPla/Modules/SincronizacionPlanea/Application/Ports/IPlaneaCliente.cs
SGPla/Modules/SincronizacionPlanea/Application/Ports/ISincronizacionPlaneaRepository.cs
SGPla/Modules/SincronizacionPlanea/Application/SincronizarPeriodo/SincronizarPeriodoPlaneaService.cs
SGPla/Modules/SincronizacionPlanea/Application/SincronizarPeriodo/Ports/ISincronizarPeriodoPlaneaService.cs
SGPla/Modules/SincronizacionPlanea/Application/SincronizarPeriodosVigentes/SincronizarPeriodosVigentesService.cs
SGPla/Modules/SincronizacionPlanea/Application/SincronizarPeriodosVigentes/Ports/ISincronizarPeriodosVigentesService.cs
SGPla/Modules/SincronizacionPlanea/Domain/PlaneaConstantes.cs
SGPla/Modules/SincronizacionPlanea/Domain/PlaneaExcepciones.cs
SGPla/Modules/SincronizacionPlanea/Domain/PlaneaNormalizador.cs
SGPla/Modules/SincronizacionPlanea/Infra/PlaneaCliente.cs
SGPla/Modules/SincronizacionPlanea/Infra/SincronizacionPlaneaRepository.cs
SGPla/Modules/SincronizacionPlanea/Infra/SincronizacionPlaneaSql.cs               (constantes SQL)
SGPla/Modules/SincronizacionPlanea/Infra/SincronizacionPlaneaWorker.cs

SGPla.Tests/Modules/SincronizacionPlanea/Fixtures/planea_muestra.json
SGPla.Tests/Modules/SincronizacionPlanea/PlaneaNormalizadorTests.cs
SGPla.Tests/Modules/SincronizacionPlanea/PlaneaClienteTests.cs
SGPla.Tests/Modules/SincronizacionPlanea/SincronizarPeriodoPlaneaServiceTests.cs
SGPla.Tests/Modules/SincronizacionPlanea/SincronizarPeriodosVigentesServiceTests.cs
```

### Modificar
```
SGPla/Models/ExperienciaEducativa.cs            (nulabilidad + Origen + colección)
SGPla/Models/Periodo.cs, PlanEstudios.cs, Region.cs, Docente.cs   (colecciones inversas)
SGPla/Data/GestionDePlazasDbContext.cs          (DbSets + mapeos)
SGPla/Program.cs                                (registrar módulo)
SGPla/appsettings.json, SGPla/appsettings.Development.json, docker-compose.yml
SGPla.Tests/SGPla.Tests.csproj                  (copiar fixture al output)
+ los sitios afectados por la nulabilidad (§5.4)
```

---

## FASE 1 — Migración de base de datos

### 1.1 Crear `SGPla.DbMigrator/database/migrations/0012_sincronizacion_planea.sql`

- Guárdalo en **UTF-8**. El `.csproj` del migrador ya incluye `database\migrations\**\*.sql`, así que no hay que tocarlo.
- DbUp ejecuta el script en una transacción y separa lotes por `GO`.
- Debe ser **idempotente**, igual que `0009`/`0011`.
- Usa **exactamente** este contenido. Revisa la sintaxis, pero no cambies nombres de tablas ni de columnas.

```sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Sincronización con PLANEA.
    1) La EE base puede existir sin plan de estudios ni datos académicos
       (las crea la sincronización con datos mínimos).
    2) Tabla ternaria EE x Periodo x PlanEstudios identificada por NRC.
    3) Docentes y horarios por NRC.
    4) Bitácora de ejecuciones.
*/

/* 1. ExperienciaEducativa: idPlanEstudios opcional.
      La FK y el índice dependen de la columna, se recrean alrededor del ALTER. */
IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ExperienciaEducativa')
      AND name = N'idPlanEstudios'
      AND is_nullable = 0
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExperienciaEducativa_PlanEstudios')
        ALTER TABLE [dbo].[ExperienciaEducativa] DROP CONSTRAINT [FK_ExperienciaEducativa_PlanEstudios];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativa_idPlanEstudios' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativa'))
        DROP INDEX [IX_ExperienciaEducativa_idPlanEstudios] ON [dbo].[ExperienciaEducativa];

    ALTER TABLE [dbo].[ExperienciaEducativa] ALTER COLUMN [idPlanEstudios] [int] NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativa_idPlanEstudios' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativa'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativa_idPlanEstudios]
        ON [dbo].[ExperienciaEducativa] ([idPlanEstudios] ASC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExperienciaEducativa_PlanEstudios')
    ALTER TABLE [dbo].[ExperienciaEducativa] WITH CHECK
        ADD CONSTRAINT [FK_ExperienciaEducativa_PlanEstudios]
        FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]);
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExperienciaEducativa') AND name = N'perfilDocente' AND is_nullable = 0)
    ALTER TABLE [dbo].[ExperienciaEducativa] ALTER COLUMN [perfilDocente] [varchar](max) NULL;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExperienciaEducativa') AND name = N'creditos' AND is_nullable = 0)
    ALTER TABLE [dbo].[ExperienciaEducativa] ALTER COLUMN [creditos] [varchar](max) NULL;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExperienciaEducativa') AND name = N'horas' AND is_nullable = 0)
    ALTER TABLE [dbo].[ExperienciaEducativa] ALTER COLUMN [horas] [varchar](max) NULL;
GO

IF COL_LENGTH(N'dbo.ExperienciaEducativa', N'origen') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativa]
        ADD [origen] [varchar](10) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativa_origen] DEFAULT ('SGPLA');
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativa_codigo' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativa'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativa_codigo]
        ON [dbo].[ExperienciaEducativa] ([codigo] ASC)
        INCLUDE ([idPlanEstudios]);
GO

/* 2. NRC: relación ternaria EE x Periodo x PlanEstudios. */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodo]
    (
        [idExperienciaEducativaPeriodo] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativa]        [int] NOT NULL,
        [idPeriodo]                     [int] NOT NULL,
        [idPlanEstudios]                [int] NULL,
        [idRegion]                      [int] NULL,
        [nrc]                           [varchar](5)   NOT NULL,
        [codigoPlanPlanea]              [varchar](50)  NULL,
        [titulo]                        [varchar](150) NOT NULL,
        [campus]                        [varchar](5)   NULL,
        [nivel]                         [varchar](5)   NULL,
        [area]                          [varchar](100) NULL,
        [fechaAlta]                     [datetime2](0) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_fechaAlta] DEFAULT (SYSDATETIME()),
        [fechaActualizacion]            [datetime2](0) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_fechaActualizacion] DEFAULT (SYSDATETIME()),
        [fechaBaja]                     [datetime2](0) NULL,
        CONSTRAINT [PK_ExperienciaEducativaPeriodo] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodo] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodo_periodo_nrc] UNIQUE NONCLUSTERED ([idPeriodo] ASC, [nrc] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_ExperienciaEducativa]
            FOREIGN KEY ([idExperienciaEducativa]) REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Periodo]
            FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_PlanEstudios]
            FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Region]
            FOREIGN KEY ([idRegion]) REFERENCES [dbo].[Region] ([id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idExperienciaEducativa' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idExperienciaEducativa]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativa] ASC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idPlanEstudios' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idPlanEstudios]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idPlanEstudios] ASC);
GO

/* 3. Docentes asignados a cada NRC. */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoDocente', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodoDocente]
    (
        [idExperienciaEducativaPeriodoDocente] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativaPeriodo]        [int] NOT NULL,
        [idRadocPlanea]                        [int] NOT NULL,
        [idDocente]                            [int] NULL,
        [numeroPersonal]                       [varchar](10)  NOT NULL,
        [nombreDocente]                        [varchar](300) NOT NULL,
        [plaza]                                [varchar](5)   NULL,
        [puesto]                               [varchar](10)  NULL,
        [textoPuesto]                          [varchar](100) NULL,
        [tipoContratacion]                     [varchar](5)   NULL,
        [textoContratacion]                    [varchar](100) NULL,
        [horas]                                [decimal](5,2) NOT NULL,
        [imparte]                              [bit] NOT NULL,
        CONSTRAINT [PK_ExperienciaEducativaPeriodoDocente] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodoDocente] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodoDocente_nrc_radoc]
            UNIQUE NONCLUSTERED ([idExperienciaEducativaPeriodo] ASC, [idRadocPlanea] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodoDocente_ExperienciaEducativaPeriodo]
            FOREIGN KEY ([idExperienciaEducativaPeriodo])
            REFERENCES [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativaPeriodo]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExperienciaEducativaPeriodoDocente_Docente]
            FOREIGN KEY ([idDocente]) REFERENCES [dbo].[Docente] ([idDocente])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodoDocente_idDocente' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoDocente'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodoDocente_idDocente]
        ON [dbo].[ExperienciaEducativaPeriodoDocente] ([idDocente] ASC);
GO

/* 4. Horarios de cada NRC: una fila por bloque de PLANEA y día. */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoHorario', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodoHorario]
    (
        [idExperienciaEducativaPeriodoHorario] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativaPeriodo]        [int] NOT NULL,
        [idHorarioPlanea]                      [int] NOT NULL,
        [dia]                                  [varchar](10)  NOT NULL,
        [horaInicio]                           [time](0) NOT NULL,
        [horaFin]                              [time](0) NOT NULL,
        [edificio]                             [varchar](50)  NULL,
        [aula]                                 [varchar](100) NULL,
        [fechaInicio]                          [date] NULL,
        [fechaFin]                             [date] NULL,
        [numeroPersonalDocente]                [varchar](10)  NULL,
        [principal]                            [bit] NOT NULL,
        CONSTRAINT [PK_ExperienciaEducativaPeriodoHorario] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodoHorario] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodoHorario_nrc_bloque_dia]
            UNIQUE NONCLUSTERED ([idExperienciaEducativaPeriodo] ASC, [idHorarioPlanea] ASC, [dia] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodoHorario_ExperienciaEducativaPeriodo]
            FOREIGN KEY ([idExperienciaEducativaPeriodo])
            REFERENCES [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativaPeriodo]) ON DELETE CASCADE
    );
END
GO

/* 5. Bitácora de ejecuciones. */
IF OBJECT_ID(N'dbo.SincronizacionPlanea', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SincronizacionPlanea]
    (
        [idSincronizacionPlanea] [int] IDENTITY(1,1) NOT NULL,
        [idPeriodo]              [int] NOT NULL,
        [fechaInicio]            [datetime2](0) NOT NULL,
        [fechaFin]               [datetime2](0) NULL,
        [estado]                 [varchar](15) NOT NULL,
        [registrosRecibidos]     [int] NULL,
        [eesCreadas]             [int] NULL,
        [nrcNuevos]              [int] NULL,
        [nrcActualizados]        [int] NULL,
        [nrcReactivados]         [int] NULL,
        [nrcBaja]                [int] NULL,
        [docentesInsertados]     [int] NULL,
        [docentesActualizados]   [int] NULL,
        [docentesEliminados]     [int] NULL,
        [horariosInsertados]     [int] NULL,
        [horariosActualizados]   [int] NULL,
        [horariosEliminados]     [int] NULL,
        [advertencias]           [nvarchar](max) NULL,
        [mensajeError]           [nvarchar](max) NULL,
        CONSTRAINT [PK_SincronizacionPlanea] PRIMARY KEY CLUSTERED ([idSincronizacionPlanea] ASC),
        CONSTRAINT [FK_SincronizacionPlanea_Periodo]
            FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]),
        CONSTRAINT [CK_SincronizacionPlanea_estado] CHECK ([estado] IN
            ('EnProceso', 'Exitosa', 'SinDatos', 'Omitida', 'Fallida', 'Interrumpida'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SincronizacionPlanea_periodo_fecha' AND object_id = OBJECT_ID(N'dbo.SincronizacionPlanea'))
    CREATE NONCLUSTERED INDEX [IX_SincronizacionPlanea_periodo_fecha]
        ON [dbo].[SincronizacionPlanea] ([idPeriodo] ASC, [fechaInicio] DESC);
GO
```

### 1.2 Checkpoint Fase 1
1. `dotnet build SGPla.DbMigrator/SGPla.DbMigrator.csproj` compila.
2. Si hay Docker disponible, ejecuta:
   ```bash
   docker compose up -d db
   docker compose run --rm db-init
   ```
   Luego vuelve a correr `db-init` una segunda vez: debe terminar sin errores (idempotencia).
3. Si Docker **no** está disponible, anótalo en el resumen final. No lo simules.

Commit: `feat(db): agrega tablas de sincronización con PLANEA`. Si compila, el commit puede incluir también la Fase 2.

---

## FASE 2 — Modelos EF, DbContext y ajustes de nulabilidad

### 2.1 `SGPla/Models/ExperienciaEducativa.cs`
Cambia estas propiedades y deja el resto igual:
```csharp
public int? IdPlanEstudios { get; set; }
public string? PerfilDocente { get; set; }
public string? Creditos { get; set; }
public string? Horas { get; set; }
public string Origen { get; set; } = "SGPLA";

public virtual PlanEstudios? IdPlanEstudiosNavigation { get; set; }
public virtual ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo { get; set; } = new List<ExperienciaEducativaPeriodo>();
```

### 2.2 Modelos nuevos
Usa el estilo de `SGPla/Models/CargaAcademica.cs`: `namespace SGPla.Models;` y `public partial class`.

```csharp
// ExperienciaEducativaPeriodo.cs
public partial class ExperienciaEducativaPeriodo
{
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdExperienciaEducativa { get; set; }
    public int IdPeriodo { get; set; }
    public int? IdPlanEstudios { get; set; }
    public int? IdRegion { get; set; }
    public string Nrc { get; set; } = null!;
    public string? CodigoPlanPlanea { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Campus { get; set; }
    public string? Nivel { get; set; }
    public string? Area { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaBaja { get; set; }

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;
    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual PlanEstudios? IdPlanEstudiosNavigation { get; set; }
    public virtual Region? IdRegionNavigation { get; set; }
    public virtual ICollection<ExperienciaEducativaPeriodoDocente> Docentes { get; set; } = new List<ExperienciaEducativaPeriodoDocente>();
    public virtual ICollection<ExperienciaEducativaPeriodoHorario> Horarios { get; set; } = new List<ExperienciaEducativaPeriodoHorario>();
}

// ExperienciaEducativaPeriodoDocente.cs
public partial class ExperienciaEducativaPeriodoDocente
{
    public int IdExperienciaEducativaPeriodoDocente { get; set; }
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdRadocPlanea { get; set; }
    public int? IdDocente { get; set; }
    public string NumeroPersonal { get; set; } = null!;
    public string NombreDocente { get; set; } = null!;
    public string? Plaza { get; set; }
    public string? Puesto { get; set; }
    public string? TextoPuesto { get; set; }
    public string? TipoContratacion { get; set; }
    public string? TextoContratacion { get; set; }
    public decimal Horas { get; set; }
    public bool Imparte { get; set; }

    public virtual ExperienciaEducativaPeriodo IdExperienciaEducativaPeriodoNavigation { get; set; } = null!;
    public virtual Docente? IdDocenteNavigation { get; set; }
}

// ExperienciaEducativaPeriodoHorario.cs
public partial class ExperienciaEducativaPeriodoHorario
{
    public int IdExperienciaEducativaPeriodoHorario { get; set; }
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdHorarioPlanea { get; set; }
    public string Dia { get; set; } = null!;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string? Edificio { get; set; }
    public string? Aula { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string? NumeroPersonalDocente { get; set; }
    public bool Principal { get; set; }

    public virtual ExperienciaEducativaPeriodo IdExperienciaEducativaPeriodoNavigation { get; set; } = null!;
}

// SincronizacionPlanea.cs
public partial class SincronizacionPlanea
{
    public int IdSincronizacionPlanea { get; set; }
    public int IdPeriodo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = null!;
    public int? RegistrosRecibidos { get; set; }
    public int? EesCreadas { get; set; }
    public int? NrcNuevos { get; set; }
    public int? NrcActualizados { get; set; }
    public int? NrcReactivados { get; set; }
    public int? NrcBaja { get; set; }
    public int? DocentesInsertados { get; set; }
    public int? DocentesActualizados { get; set; }
    public int? DocentesEliminados { get; set; }
    public int? HorariosInsertados { get; set; }
    public int? HorariosActualizados { get; set; }
    public int? HorariosEliminados { get; set; }
    public string? Advertencias { get; set; }
    public string? MensajeError { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
}
```

Agrega las colecciones inversas:
- `Periodo`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo` y `ICollection<SincronizacionPlanea> SincronizacionPlanea`.
- `PlanEstudios`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo`.
- `Region`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo`.
- `Docente`: `ICollection<ExperienciaEducativaPeriodoDocente> ExperienciaEducativaPeriodoDocente`.

### 2.3 `SGPla/Data/GestionDePlazasDbContext.cs`
- **DbSets:** agrega `ExperienciaEducativaPeriodo`, `ExperienciaEducativaPeriodoDocente`, `ExperienciaEducativaPeriodoHorario` y `SincronizacionPlanea`, siguiendo el patrón `public virtual DbSet<X> X { get; set; }`.
- **Bloque existente de `ExperienciaEducativa`** (línea ~509):
  - Agrega `entity.HasIndex(e => e.Codigo, "IX_ExperienciaEducativa_codigo");`.
  - Agrega `entity.Property(e => e.Origen).HasMaxLength(10).IsUnicode(false).HasDefaultValue("SGPLA").HasColumnName("origen");`.
  - La relación con `PlanEstudios` se queda igual; EF la vuelve opcional porque la FK ahora es `int?`.
- **Mapeos nuevos:** agrégalos **al final de `OnModelCreating`**, antes del cierre del método (el archivo no tiene `OnModelCreatingPartial`). Sigue el estilo de `CargaAcademica`:
  - `HasKey`, y `HasColumnName` en **camelCase** exactamente como en el SQL.
  - `HasMaxLength` e `IsUnicode(false)` en los `varchar`; `HasColumnType("decimal(5, 2)")` en `horas`; `HasColumnType("time(0)")` y `HasColumnType("datetime2(0)")` donde corresponda.
  - `ToTable("…")` con el nombre de cada tabla.
  - Índices únicos:
    - `HasIndex(e => new { e.IdPeriodo, e.Nrc }, "UX_ExperienciaEducativaPeriodo_periodo_nrc").IsUnique()`.
    - `HasIndex(e => new { e.IdExperienciaEducativaPeriodo, e.IdRadocPlanea }, "UX_ExperienciaEducativaPeriodoDocente_nrc_radoc").IsUnique()`.
    - `HasIndex(e => new { e.IdExperienciaEducativaPeriodo, e.IdHorarioPlanea, e.Dia }, "UX_ExperienciaEducativaPeriodoHorario_nrc_bloque_dia").IsUnique()`.
  - Relaciones:
    - De los hijos (docentes y horarios) hacia el NRC: `.OnDelete(DeleteBehavior.Cascade)`.
    - Todas las demás: `.OnDelete(DeleteBehavior.ClientSetNull)`.
    - Usa `.HasConstraintName("FK_…")` con los mismos nombres del SQL.
  - Para `ExperienciaEducativaPeriodo.IdRegionNavigation` usa `.HasForeignKey(d => d.IdRegion)` contra `Region`, cuya PK se mapea a la columna `id`.
  - En `fechaAlta` y `fechaActualizacion` de `ExperienciaEducativaPeriodo`, agrega `.HasDefaultValueSql("(sysdatetime())")`.

### 2.4 Ajustes por la nulabilidad
Compila (`dotnet build SGPla/SGPla.csproj`) y corrige **todos los errores**. Revisa también los warnings **nuevos** de nulabilidad (CS8601, CS8602, CS8604) que aparezcan en estos archivos. Los sitios conocidos son:

| Archivo | Cambio |
|---|---|
| `SGPla/Modules/ProgramasEducativos/Infra/ProgramaEducativoRepository.cs:~377` | `.Where(experiencia => idsPlanesEstudio.Contains(experiencia.IdPlanEstudios))` → `.Where(experiencia => experiencia.IdPlanEstudios.HasValue && idsPlanesEstudio.Contains(experiencia.IdPlanEstudios.Value))` |
| `SGPla/Repositories/Implementations/OfertaRepository.cs:~69-78` | El `GroupBy` usa `IdPlanEstudios`, que ahora es `int?`. Si el consumidor espera `int`, usa `IdPlanEstudios ?? 0`. Las ofertas siempre apuntan a EEs con plan |
| `SGPla/Repositories/Implementations/OfertaRepository.cs:~111` | `PerfilDocente = o.IdExperienciaEducativaNavigation.PerfilDocente ?? string.Empty` |
| `SGPla/Repositories/Implementations/ProgramacionAcademicaRepository.cs:~205` | `…IdPlanEstudiosNavigation!.Modalidad` |
| `SGPla/Services/Implementations/AvisoService.cs` (~295, 306, 553) | Usa `?? string.Empty` o el fallback `"Perfil del docente"` que ya existe |
| `SGPla/Services/Implementations/PlantillaService.cs` (~217, 296) | `?? string.Empty` |
| `SGPla/Repositories/Implementations/ExperienciaEducativaRepository.cs` (`ObtenerIdsPorNombreAsync`, ~línea 95) | Agrega `.Where(e => e.IdPlanEstudios != null)` antes del `Select`. **Es obligatorio**: sin este filtro, la importación por Excel podría asociar ofertas a EEs creadas por PLANEA que no tienen plan |
| Otras lecturas de `PerfilDocente`/`Creditos`/`Horas` hacia DTOs `string` no nulos (busca con `grep -rn "PerfilDocente\|\.Creditos\|\.Horas" SGPla --include='*.cs'`) | `?? string.Empty` |

**No cambies** los validadores de planes de estudio (`PlanEstudiosValidator`, `PlanEstudiosReglas`). Siguen exigiendo perfil, créditos y horas para las EEs que se capturan en SGPLA, y eso es correcto.

### 2.5 Checkpoint Fase 2
- `dotnet build SGPla/SGPla.csproj` compila con 0 errores.
- `dotnet test SGPla.Tests/SGPla.Tests.csproj`: todas las pruebas que ya existían siguen pasando.
- Commit: `feat(db): agrega tablas y modelos para la sincronización con PLANEA`.

---

## FASE 3 — Contratos, normalizador y cliente HTTP

Todos los archivos van bajo `SGPla/Modules/SincronizacionPlanea/`, con el namespace de su carpeta.

### 3.1 `Application/PlaneaOpciones.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace SGPla.Modules.SincronizacionPlanea.Application;

public sealed class PlaneaOpciones
{
    public const string Seccion = "Planea";

    public bool Habilitada { get; set; }

    [Required, Url]
    public string UrlBase { get; set; } = "https://planea.uv.mx/planea/index.php/apiroladoovr/";

    public TimeSpan Intervalo { get; set; } = TimeSpan.FromDays(1);
    public TimeSpan RetrasoInicial { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan TiempoEspera { get; set; } = TimeSpan.FromMinutes(5);

    [Range(1, 5)]
    public int Intentos { get; set; } = 3;

    public TimeSpan EsperaEntreIntentos { get; set; } = TimeSpan.FromSeconds(30);

    [Range(0, 24)]
    public int VentanaAnticipacionMeses { get; set; } = 6;

    [Range(1, 100)]
    public int PorcentajeMaximoBajas { get; set; } = 30;

    public TimeSpan UmbralInterrumpida { get; set; } = TimeSpan.FromHours(1);
}
```

### 3.2 `Application/Models/PlaneaRespuesta.cs` (DTOs JSON ligeros)
Mapea **solo** estas propiedades. System.Text.Json ignora las demás al leer, sin crear strings para ellas.
```csharp
using System.Text.Json.Serialization;

namespace SGPla.Modules.SincronizacionPlanea.Application.Models;

public sealed class PlaneaRespuesta
{
    [JsonPropertyName("periodo")] public string? Periodo { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("resultado")] public List<PlaneaSeccion>? Resultado { get; set; }
    [JsonPropertyName("horarios")] public List<PlaneaHorario>? Horarios { get; set; }
}

public sealed class PlaneaSeccion
{
    [JsonPropertyName("Region")] public string? Region { get; set; }
    [JsonPropertyName("Area")] public string? Area { get; set; }
    [JsonPropertyName("sec_programa")] public string? Programa { get; set; }
    [JsonPropertyName("sec_campus")] public string? Campus { get; set; }
    [JsonPropertyName("nivel")] public string? Nivel { get; set; }
    [JsonPropertyName("sec_titulo")] public string? Titulo { get; set; }
    [JsonPropertyName("radoc_nrc")] public string? Nrc { get; set; }
    [JsonPropertyName("radoc_materia")] public string? Materia { get; set; }
    [JsonPropertyName("radoc_curso")] public string? Curso { get; set; }
    [JsonPropertyName("ID_TITULAR")] public string? IdTitular { get; set; }
    [JsonPropertyName("radoc_nombre")] public string? NombreDocente { get; set; }
    [JsonPropertyName("radoc_plaza")] public string? Plaza { get; set; }
    [JsonPropertyName("radoc_nhoras")] public string? Horas { get; set; }
    [JsonPropertyName("radoc_id")] public string? IdRadoc { get; set; }
    [JsonPropertyName("radoc_imparte")] public string? Imparte { get; set; }
    [JsonPropertyName("radoc_puesto")] public string? Puesto { get; set; }
    [JsonPropertyName("radoc_textopuesto")] public string? TextoPuesto { get; set; }
    [JsonPropertyName("radoc_tipocontratacion")] public string? TipoContratacion { get; set; }
    [JsonPropertyName("radoc_textocontratacion")] public string? TextoContratacion { get; set; }
}

public sealed class PlaneaHorario
{
    [JsonPropertyName("NRC")] public string? Nrc { get; set; }
    [JsonPropertyName("rhs_id")] public string? IdHorario { get; set; }
    [JsonPropertyName("ID_DOCENTE")] public string? IdDocente { get; set; }
    [JsonPropertyName("IND_PRINCIPAL")] public string? Principal { get; set; }
    [JsonPropertyName("EDIFICIO")] public string? Edificio { get; set; }
    [JsonPropertyName("AULA")] public string? Aula { get; set; }
    [JsonPropertyName("FECHA_INICIO")] public string? FechaInicio { get; set; }
    [JsonPropertyName("FECHA_FIN")] public string? FechaFin { get; set; }
    [JsonPropertyName("LUN_INI")] public string? LunIni { get; set; }
    [JsonPropertyName("LUN_FIN")] public string? LunFin { get; set; }
    [JsonPropertyName("MAR_INI")] public string? MarIni { get; set; }
    [JsonPropertyName("MAR_FIN")] public string? MarFin { get; set; }
    [JsonPropertyName("MIE_INI")] public string? MieIni { get; set; }
    [JsonPropertyName("MIE_FIN")] public string? MieFin { get; set; }
    [JsonPropertyName("JUE_INI")] public string? JueIni { get; set; }
    [JsonPropertyName("JUE_FIN")] public string? JueFin { get; set; }
    [JsonPropertyName("VIE_INI")] public string? VieIni { get; set; }
    [JsonPropertyName("VIE_FIN")] public string? VieFin { get; set; }
    [JsonPropertyName("SAB_INI")] public string? SabIni { get; set; }
    [JsonPropertyName("SAB_FIN")] public string? SabFin { get; set; }
}
```

### 3.3 `Application/Models/PlaneaModelos.cs` (datos ya normalizados)
```csharp
namespace SGPla.Modules.SincronizacionPlanea.Application.Models;

public sealed record PeriodoPorSincronizar(int IdPeriodo, string Codigo);

public sealed record ExperienciaEducativaPlanea(string Codigo, string Nombre);

public sealed record NrcPlanea(
    string Nrc, string CodigoExperiencia, string Titulo, string? CodigoPlan,
    string? Campus, string? Nivel, string? Region, string? Area);

public sealed record DocenteNrcPlanea(
    int IdRadoc, string Nrc, string NumeroPersonal, string? NumeroPersonalNormalizado,
    string Nombre, string? Plaza, string? Puesto, string? TextoPuesto,
    string? TipoContratacion, string? TextoContratacion, decimal Horas, bool Imparte);

public sealed record HorarioNrcPlanea(
    int IdHorario, string Nrc, string Dia, TimeOnly HoraInicio, TimeOnly HoraFin,
    string? Edificio, string? Aula, DateOnly? FechaInicio, DateOnly? FechaFin,
    string? NumeroPersonalDocente, bool Principal);

public sealed record DatosPeriodoPlanea(
    IReadOnlyList<ExperienciaEducativaPlanea> Experiencias,
    IReadOnlyList<NrcPlanea> Nrcs,
    IReadOnlyList<DocenteNrcPlanea> Docentes,
    IReadOnlyList<HorarioNrcPlanea> Horarios,
    IReadOnlyList<string> Advertencias);
```

### 3.4 `Application/Contracts/`
```csharp
// ResumenAplicacionPlanea.cs
public sealed record ResumenAplicacionPlanea(
    int EesCreadas,
    int NrcNuevos, int NrcActualizados, int NrcReactivados, int NrcBaja,
    int DocentesInsertados, int DocentesActualizados, int DocentesEliminados,
    int HorariosInsertados, int HorariosActualizados, int HorariosEliminados);

// ResultadoSincronizacionPlanea.cs
public sealed record ResultadoSincronizacionPlanea(
    string CodigoPeriodo, string Estado, ResumenAplicacionPlanea? Resumen, string? Mensaje);
```

### 3.5 `Domain/PlaneaConstantes.cs` y `Domain/PlaneaExcepciones.cs`
```csharp
public static class PlaneaConstantes
{
    public const string ORIGEN_PLANEA = "PLANEA";

    public const string ESTADO_EN_PROCESO = "EnProceso";
    public const string ESTADO_EXITOSA = "Exitosa";
    public const string ESTADO_SIN_DATOS = "SinDatos";
    public const string ESTADO_OMITIDA = "Omitida";
    public const string ESTADO_FALLIDA = "Fallida";
    public const string ESTADO_INTERRUMPIDA = "Interrumpida";

    public const int LONGITUD_NRC = 5;
    public const int LONGITUD_CODIGO_EE = 10;
    public const int LONGITUD_TITULO = 150;
}

public sealed class PlaneaRespuestaInvalidaException(string mensaje) : Exception(mensaje);
public sealed class SincronizacionEnCursoException(string codigoPeriodo)
    : Exception($"Ya hay una sincronización en curso para el periodo {codigoPeriodo}.");
public sealed class BajasMasivasException(int bajas, int activos, int porcentajeMaximo)
    : Exception($"La sincronización daría de baja {bajas} de {activos} NRC activos (máximo permitido {porcentajeMaximo} %). Se canceló para proteger los datos.");
```

### 3.6 `Domain/PlaneaNormalizador.cs`
Clase **estática y pura**: sin I/O ni dependencias. Recorre los datos una sola vez usando diccionarios (O(n)). Implementa exactamente estas reglas:

```csharp
using System.Globalization;
using System.Text.RegularExpressions;
using SGPla.Modules.SincronizacionPlanea.Application.Models;

namespace SGPla.Modules.SincronizacionPlanea.Domain;

public static partial class PlaneaNormalizador
{
    public static DatosPeriodoPlanea Normalizar(PlaneaRespuesta respuesta)
    {
        var advertencias = new AcumuladorAdvertencias();
        var nrcs = new Dictionary<string, NrcPlanea>(StringComparer.Ordinal);
        var titulos = new Dictionary<string, string>(StringComparer.Ordinal);   // codigoEE -> título más largo
        var docentes = new Dictionary<int, DocenteNrcPlanea>();

        foreach (var fila in respuesta.Resultado ?? [])
        {
            var nrc = Limpiar(fila.Nrc);
            var materia = Limpiar(fila.Materia);
            var curso = Limpiar(fila.Curso);
            var titulo = NormalizarEspacios(fila.Titulo);

            if (nrc is null || nrc.Length > PlaneaConstantes.LONGITUD_NRC
                || materia is null || curso is null || titulo is null)
            {
                advertencias.Agregar("Filas sin NRC, materia, curso o título válidos", nrc ?? fila.IdRadoc);
                continue;
            }

            var codigo = $"{materia} {curso}";
            if (codigo.Length > PlaneaConstantes.LONGITUD_CODIGO_EE)
            {
                advertencias.Agregar("Códigos de EE de más de 10 caracteres", codigo);
                continue;
            }

            titulo = Truncar(titulo, PlaneaConstantes.LONGITUD_TITULO);

            // Datos de sección: idénticos en todas las filas del NRC; se toma la primera.
            nrcs.TryAdd(nrc, new NrcPlanea(
                nrc, codigo, titulo,
                Truncar(Limpiar(fila.Programa), 50),
                Truncar(Limpiar(fila.Campus), 5),
                Truncar(Limpiar(fila.Nivel), 5),
                Truncar(Limpiar(fila.Region), 50),
                Truncar(Limpiar(fila.Area), 100)));

            // Nombre de la EE nueva: el título más largo (PLANEA trunca algunos).
            if (!titulos.TryGetValue(codigo, out var actual) || titulo.Length > actual.Length)
                titulos[codigo] = titulo;

            // Docente asignado (el NRC ya quedó registrado aunque no haya docente).
            var numeroPersonal = Limpiar(fila.IdTitular);
            if (numeroPersonal is null)
            {
                advertencias.Agregar("Filas de asignación sin docente", nrc);
                continue;
            }
            if (!int.TryParse(fila.IdRadoc, NumberStyles.None, CultureInfo.InvariantCulture, out var idRadoc))
            {
                advertencias.Agregar("Filas con radoc_id inválido", nrc);
                continue;
            }
            if (!docentes.TryAdd(idRadoc, CrearDocente(idRadoc, nrc, numeroPersonal, fila, advertencias)))
                advertencias.Agregar("radoc_id repetido", idRadoc.ToString(CultureInfo.InvariantCulture));
        }

        var horarios = new Dictionary<(int, string), HorarioNrcPlanea>();
        foreach (var bloque in respuesta.Horarios ?? [])
        {
            var nrc = Limpiar(bloque.Nrc);
            if (nrc is null || !nrcs.ContainsKey(nrc))
            {
                advertencias.Agregar("Horarios de NRC que no vienen en resultado", nrc);
                continue;
            }
            if (!int.TryParse(bloque.IdHorario, NumberStyles.None, CultureInfo.InvariantCulture, out var idHorario))
            {
                advertencias.Agregar("Horarios con rhs_id inválido", nrc);
                continue;
            }

            var tieneDias = false;
            foreach (var (dia, inicio, fin) in DiasDe(bloque))
            {
                if (Limpiar(inicio) is null && Limpiar(fin) is null) continue;
                tieneDias = true;

                if (!TryParsearHora(inicio, out var horaInicio) || !TryParsearHora(fin, out var horaFin)
                    || horaFin <= horaInicio)
                {
                    advertencias.Agregar("Horas de horario inválidas", $"{nrc}/{dia}");
                    continue;
                }

                var horario = new HorarioNrcPlanea(
                    idHorario, nrc, dia, horaInicio, horaFin,
                    Truncar(Limpiar(bloque.Edificio), 50),
                    Truncar(Limpiar(bloque.Aula), 100),
                    ParsearFecha(bloque.FechaInicio),
                    ParsearFecha(bloque.FechaFin),
                    Truncar(Limpiar(bloque.IdDocente), 10),
                    EsSi(bloque.Principal));

                if (!horarios.TryAdd((idHorario, dia), horario))
                    advertencias.Agregar("Bloques de horario repetidos", $"{idHorario}/{dia}");
            }

            if (!tieneDias)
                advertencias.Agregar("Bloques de horario sin días", nrc);
        }

        return new DatosPeriodoPlanea(
            titulos.Select(par => new ExperienciaEducativaPlanea(par.Key, par.Value)).ToList(),
            nrcs.Values.ToList(),
            docentes.Values.ToList(),
            horarios.Values.ToList(),
            advertencias.Resumir());
    }

    // ---- helpers públicos (se prueban directamente) ----

    /// Trim; "", "-", "---" => null.
    public static string? Limpiar(string? valor) { ... }

    /// Limpiar + colapsar espacios internos múltiples (Regex \s+ -> " ").
    public static string? NormalizarEspacios(string? valor) { ... }

    /// "E00029548" -> "29548". Devuelve null si no tenía prefijo/ceros (ya coincide tal cual) o si queda vacío.
    public static string? NormalizarNumeroPersonal(string numeroPersonal) { ... }

    /// "1400" -> 14:00. Exactamente 4 dígitos, HH <= 23, mm <= 59.
    public static bool TryParsearHora(string? valor, out TimeOnly hora) { ... }

    /// "yyyy-MM-dd" (InvariantCulture) o null.
    public static DateOnly? ParsearFecha(string? valor) { ... }

    /// "SI" (ignorando mayúsculas y espacios) => true.
    public static bool EsSi(string? valor) { ... }

    public static string? Truncar(string? valor, int longitud) { ... }

    // ---- privados ----

    private static DocenteNrcPlanea CrearDocente(int idRadoc, string nrc, string numeroPersonal,
        PlaneaSeccion fila, AcumuladorAdvertencias advertencias)
    {
        // horas: decimal.TryParse(fila.Horas, NumberStyles.Number, CultureInfo.InvariantCulture, …);
        //        si falla => 0 y advertencias.Agregar("Horas de docente inválidas", nrc).
        // nombre: NormalizarEspacios(fila.NombreDocente) ?? string.Empty, truncado a 300.
        // numeroPersonal truncado a 10; NumeroPersonalNormalizado = NormalizarNumeroPersonal(numeroPersonal).
        // Plaza(5), Puesto(10), TextoPuesto(100), TipoContratacion(5), TextoContratacion(100): Truncar(Limpiar(...)).
        // Imparte = EsSi(fila.Imparte).
    }

    private static IEnumerable<(string Dia, string? Inicio, string? Fin)> DiasDe(PlaneaHorario h) =>
    [
        ("Lunes", h.LunIni, h.LunFin),
        ("Martes", h.MarIni, h.MarFin),
        ("Miercoles", h.MieIni, h.MieFin),
        ("Jueves", h.JueIni, h.JueFin),
        ("Viernes", h.VieIni, h.VieFin),
        ("Sabado", h.SabIni, h.SabFin),
    ];

    /// Agrupa advertencias por categoría: "5619 × Filas de asignación sin docente (ej. 10677, 10678, 10679, 10680, 10681)".
    /// Guarda como máximo 5 ejemplos por categoría. Resumir() devuelve una línea por categoría, ordenadas por conteo descendente.
    private sealed class AcumuladorAdvertencias { ... }
}
```
**No** guardes `FIN` sumándole un minuto. `"1459"` se guarda literal como `14:59`.

### 3.7 `Application/Ports/IPlaneaCliente.cs` e `Infra/PlaneaCliente.cs`
```csharp
public interface IPlaneaCliente
{
    Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken);
}
```
```csharp
public sealed class PlaneaCliente : IPlaneaCliente
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;

    public PlaneaCliente(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken)
    {
        // ResponseHeadersRead: NO bufferiza los ~65 MB; se deserializa directo desde el stream de red.
        using var respuesta = await _httpClient.GetAsync(
            $"periodo/{Uri.EscapeDataString(codigoPeriodo.Trim())}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        respuesta.EnsureSuccessStatusCode();

        await using var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            return await JsonSerializer.DeserializeAsync<PlaneaRespuesta>(stream, OpcionesJson, cancellationToken)
                ?? throw new PlaneaRespuestaInvalidaException("PLANEA devolvió una respuesta vacía.");
        }
        catch (JsonException ex)
        {
            throw new PlaneaRespuestaInvalidaException($"PLANEA devolvió un JSON inválido: {ex.Message}");
        }
    }
}
```
**Importante:** la `BaseAddress` termina en `/` y la ruta relativa **no** empieza con `/`. Si no, se pierde el segmento `apiroladoovr/`.

### 3.8 Pruebas de la Fase 3

**Fixture `SGPla.Tests/Modules/SincronizacionPlanea/Fixtures/planea_muestra.json`**

Genéralo con este script, **sin editarlo a mano**. Así conserva la forma real del JSON, con todos sus campos.
```bash
python3 - <<'EOF'
import json
d = json.load(open('/Users/kaleb/repos/sgpla/Endpoint.json'))
nrcs = {'10676','10677','10709','10795','59262','10812','10695','25637','38444'}
res = [x for x in d['resultado'] if x['radoc_nrc'] in nrcs]
hor = [x for x in d['horarios'] if x['NRC'] in nrcs or x['NRC'] == '25108']
out = {'periodo': d['periodo'], 'total': len(res), 'resultado': res, 'horarios': hor}
json.dump(out, open('SGPla.Tests/Modules/SincronizacionPlanea/Fixtures/planea_muestra.json','w'), ensure_ascii=False, indent=4)
print(len(res), len(hor))
EOF
```
Qué cubre cada NRC:

| NRC | Caso |
|---|---|
| `10676` | 1 docente; horario de lunes a viernes, 14:00–14:59, edificio `I-INEB`, aula `I-02` |
| `10677` | Sin docente |
| `10709` | 2 docentes (`radoc_id` 249794 y 249795) |
| `10795` | Varios bloques de horario; al menos uno sin días |
| `59262` | EE `MVZ 80001` |
| `10812` | `sec_programa` nulo |
| `10695` y `25637` | Mismo código `DECA 28003`, título truncado y completo |
| `38444` | `radoc_nhoras` decimal |
| `25108` | Horario huérfano (solo aparece en `horarios`) |

En `SGPla.Tests/SGPla.Tests.csproj` agrega:
```xml
<ItemGroup>
  <None Update="Modules\SincronizacionPlanea\Fixtures\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```
Lee el fixture con `Path.Combine(AppContext.BaseDirectory, "Modules", "SincronizacionPlanea", "Fixtures", "planea_muestra.json")`.

**`PlaneaNormalizadorTests`** (xUnit, clase `sealed`, `[Fact]`/`[Theory]`, nombres `Metodo_Condicion`). Carga el fixture con `JsonSerializer.Deserialize<PlaneaRespuesta>`, **con las mismas opciones que usa el cliente**. Prueba:
- Hay un único NRC por cada `radoc_nrc`.
- El NRC `10709` tiene 2 docentes.
- `10677` existe como NRC y no tiene docente; la advertencia "sin docente" aparece.
- El código `CVCB 18003` para `10676`, y `MVZ 80001`.
- La EE `DECA 28003` se llama `DERECHO CONSTITUCIONAL MEXICANO`.
- `10676` genera 5 horarios: días `Lunes`…`Viernes`, `14:00`–`14:59`.
- No hay horarios del NRC `25108`, y aparece la advertencia de horarios huérfanos.
- El docente de `38444` tiene horas con decimal (`4.5m` u otro valor fraccional; léelo del fixture).
- `10812` tiene `CodigoPlan == null`.
- `[Theory]` para los helpers:
  - `Limpiar`: `""`, `"  "`, `"-"`, `"---"` → null; `" X "` → `"X"`.
  - `NormalizarNumeroPersonal`: `"E00029548"` → `"29548"`.
  - `TryParsearHora`: `"0700"` → 07:00; `"2460"`, `"123"`, `"ab12"` y null → false.
  - `EsSi`.
  - `ParsearFecha`: `"2026-08-17"` → fecha; `"x"` → null.

**`PlaneaClienteTests`.** Crea un `HttpMessageHandler` falso (subclase privada que sobrescribe `SendAsync`). Úsalo con `new HttpClient(handler) { BaseAddress = new Uri("https://planea.test/planea/index.php/apiroladoovr/") }`. Prueba:
- Con el fixture en un `StreamContent`, deserializa y `Total == Resultado.Count`.
- El handler recibió la URL `…/apiroladoovr/periodo/202701`.
- Una respuesta 500 produce `HttpRequestException`.
- Un contenido `"{no json"` produce `PlaneaRespuestaInvalidaException`.
- Un contenido `"null"` produce `PlaneaRespuestaInvalidaException`.

### 3.9 Checkpoint Fase 3
- `dotnet build` y `dotnet test`: todo en verde.
- Commits: `feat(planea): agrega cliente y normalizador de PLANEA` y `test(planea): …`.

---

## FASE 4 — Repositorio set-based (SqlBulkCopy + MERGE)

### 4.1 `Application/Ports/ISincronizacionPlaneaRepository.cs`
```csharp
public interface ISincronizacionPlaneaRepository
{
    Task<IReadOnlyList<PeriodoPorSincronizar>> ObtenerPeriodosVigentesAsync(
        DateOnly hoy, int ventanaAnticipacionMeses, CancellationToken cancellationToken);

    Task<int> ContarPeriodosSinFechasAsync(CancellationToken cancellationToken);

    Task<int> IniciarBitacoraAsync(int idPeriodo, CancellationToken cancellationToken);

    Task CerrarBitacoraAsync(
        int idSincronizacion, string estado, int? registrosRecibidos,
        ResumenAplicacionPlanea? resumen, string? advertencias, string? mensajeError,
        CancellationToken cancellationToken);

    Task<int> MarcarInterrumpidasAsync(TimeSpan umbral, CancellationToken cancellationToken);

    Task<ResumenAplicacionPlanea> AplicarAsync(
        int idPeriodo, string codigoPeriodo, DatosPeriodoPlanea datos,
        int porcentajeMaximoBajas, CancellationToken cancellationToken);
}
```

### 4.2 `Infra/SincronizacionPlaneaRepository.cs`: métodos simples (EF)
- **`ObtenerPeriodosVigentesAsync`:** `_context.Periodo.AsNoTracking()`, con este filtro:
  ```csharp
  var limite = hoy.AddMonths(ventanaAnticipacionMeses);
  .Where(p => p.FechaInicio != null && p.FechaFin != null && p.FechaFin >= hoy && p.FechaInicio <= limite)
  .OrderBy(p => p.Codigo)
  .Select(p => new PeriodoPorSincronizar(p.IdPeriodo, p.Codigo.Trim()))
  ```
  `Codigo` es `char(6)`: aplica `Trim` siempre.
- **`ContarPeriodosSinFechasAsync`:** cuenta los periodos con `FechaInicio == null || FechaFin == null`.
- **`IniciarBitacoraAsync`:**
  1. `Add(new SincronizacionPlanea { IdPeriodo, FechaInicio = DateTime.Now, Estado = ESTADO_EN_PROCESO })`.
  2. `SaveChangesAsync`.
  3. Devuelve el id y luego hace `_context.ChangeTracker.Clear()`.
- **`CerrarBitacoraAsync`:** `_context.SincronizacionPlanea.Where(s => s.IdSincronizacionPlanea == id).ExecuteUpdateAsync(set => set.SetProperty(...)…)`. Asigna `FechaFin = DateTime.Now`, el estado, los contadores del resumen (o null), las advertencias y el mensaje de error.
- **`MarcarInterrumpidasAsync`:** `ExecuteUpdateAsync` sobre los registros con `Estado == EnProceso && FechaInicio < DateTime.Now - umbral`. Asigna `Estado = Interrumpida`, `FechaFin = DateTime.Now` y `MensajeError = "La ejecución no terminó (reinicio o caída de la aplicación)."`.

### 4.3 `AplicarAsync`: algoritmo
Los ~70k registros **no** pasan por el ChangeTracker de EF. Se usa ADO.NET sobre la misma conexión del DbContext, en **una sola transacción**:

```csharp
public async Task<ResumenAplicacionPlanea> AplicarAsync(int idPeriodo, string codigoPeriodo,
    DatosPeriodoPlanea datos, int porcentajeMaximoBajas, CancellationToken cancellationToken)
{
    await _context.Database.OpenConnectionAsync(cancellationToken);
    try
    {
        await using var transaccionEf = await _context.Database.BeginTransactionAsync(cancellationToken);
        var conexion = (SqlConnection)_context.Database.GetDbConnection();
        var transaccion = (SqlTransaction)transaccionEf.GetDbTransaction();

        await AdquirirBloqueoAsync(conexion, transaccion, codigoPeriodo, cancellationToken);   // paso 1
        await EjecutarAsync(conexion, transaccion, SincronizacionPlaneaSql.CrearTablasTemporales, idPeriodo, cancellationToken); // paso 2
        await CopiarAsync(conexion, transaccion, "#EePlanea", CrearTablaExperiencias(datos.Experiencias), cancellationToken);
        await CopiarAsync(conexion, transaccion, "#NrcPlanea", CrearTablaNrcs(datos.Nrcs), cancellationToken);
        await CopiarAsync(conexion, transaccion, "#DocentePlanea", CrearTablaDocentes(datos.Docentes), cancellationToken);
        await CopiarAsync(conexion, transaccion, "#HorarioPlanea", CrearTablaHorarios(datos.Horarios), cancellationToken);

        await EjecutarAsync(..., SincronizacionPlaneaSql.ResolverReferencias, ...);             // paso 3
        var eesCreadas = await EjecutarEscalarAsync(..., SincronizacionPlaneaSql.CrearExperiencias, ...); // paso 4
        await EjecutarAsync(..., SincronizacionPlaneaSql.ResolverExperiencias, ...);            // paso 5

        var (activos, bajas) = await ContarBajasAsync(...);                                     // paso 6
        if (activos > 0 && bajas * 100 > activos * porcentajeMaximoBajas)
            throw new BajasMasivasException(bajas, activos, porcentajeMaximoBajas);

        var nrc = await LeerContadoresAsync(..., SincronizacionPlaneaSql.MergeNrc, ...);        // paso 7
        await EjecutarAsync(..., SincronizacionPlaneaSql.ResolverNrcHijos, ...);
        var docentes = await LeerContadoresAsync(..., SincronizacionPlaneaSql.MergeDocentes, ...); // paso 8
        var horarios = await LeerContadoresAsync(..., SincronizacionPlaneaSql.MergeHorarios, ...); // paso 9

        await transaccionEf.CommitAsync(cancellationToken);                                     // paso 10
        return new ResumenAplicacionPlanea(eesCreadas, …);
    }
    finally
    {
        await _context.Database.CloseConnectionAsync();
    }
}
```

**Helpers ADO.NET:**
- Cada `SqlCommand` lleva `Connection = conexion`, `Transaction = transaccion`, `CommandTimeout = 300` y el parámetro `@idPeriodo` (`SqlDbType.Int`).
- **`AdquirirBloqueoAsync`:**
  - Ejecuta `DECLARE @r int; EXEC @r = sp_getapplock @Resource = @recurso, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 0; SELECT @r;` con `@recurso = "SincronizacionPlanea:" + codigoPeriodo`.
  - Si el resultado es `< 0`, lanza `SincronizacionEnCursoException(codigoPeriodo)`.
- **`CopiarAsync`:**
  - `using var bulk = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, transaccion) { DestinationTableName = tabla, BatchSize = 5000, BulkCopyTimeout = 300 };`
  - Agrega un `ColumnMappings.Add(col.ColumnName, col.ColumnName)` por cada columna del `DataTable`.
  - Termina con `await bulk.WriteToServerAsync(dataTable, cancellationToken)`.
- **`CrearTabla…`:**
  - Crea `DataTable`s cuyos nombres de columna coinciden exactamente con los de las tablas temporales, **sin** las columnas calculadas (`id…`) que se llenan en SQL.
  - Los `null` se escriben como `DBNull.Value`.
  - Tipos: `TimeOnly` → columna `TimeSpan` (`hora.ToTimeSpan()`), `DateOnly?` → `DateTime` (`fecha.ToDateTime(TimeOnly.MinValue)`), `decimal` → `decimal` y `bool` → `bool`.
- **`LeerContadoresAsync`:** ejecuta el MERGE y lee **la última fila de resultados** (el `SELECT` final de conteos) con `SqlDataReader`. Si una columna llega `NULL` (el MERGE no afectó filas), cuenta como 0.

### 4.4 `Infra/SincronizacionPlaneaSql.cs`: SQL exacto
`internal static class` con constantes `const string` en raw string literals (`"""…"""`).

> **Collation:** las tablas temporales viven en `tempdb`, que puede tener otra collation. **Todas** las columnas de texto de las `#tablas` llevan `COLLATE DATABASE_DEFAULT`; si falta, SQL Server lanza "Cannot resolve the collation conflict".

```sql
-- CrearTablasTemporales
DROP TABLE IF EXISTS #EePlanea;
DROP TABLE IF EXISTS #NrcPlanea;
DROP TABLE IF EXISTS #DocentePlanea;
DROP TABLE IF EXISTS #HorarioPlanea;

CREATE TABLE #EePlanea (
    codigo varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    nombre varchar(150) COLLATE DATABASE_DEFAULT NOT NULL
);

CREATE TABLE #NrcPlanea (
    nrc               varchar(5)   COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    codigoExperiencia varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL,
    titulo            varchar(150) COLLATE DATABASE_DEFAULT NOT NULL,
    codigoPlan        varchar(50)  COLLATE DATABASE_DEFAULT NULL,
    campus            varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    nivel             varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    region            varchar(50)  COLLATE DATABASE_DEFAULT NULL,
    area              varchar(100) COLLATE DATABASE_DEFAULT NULL,
    idExperienciaEducativa int NULL,
    idPlanEstudios         int NULL,
    idRegion               int NULL
);

CREATE TABLE #DocentePlanea (
    idRadoc                   int NOT NULL PRIMARY KEY,
    nrc                       varchar(5)   COLLATE DATABASE_DEFAULT NOT NULL,
    numeroPersonal            varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL,
    numeroPersonalNormalizado varchar(10)  COLLATE DATABASE_DEFAULT NULL,
    nombre                    varchar(300) COLLATE DATABASE_DEFAULT NOT NULL,
    plaza                     varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    puesto                    varchar(10)  COLLATE DATABASE_DEFAULT NULL,
    textoPuesto               varchar(100) COLLATE DATABASE_DEFAULT NULL,
    tipoContratacion          varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    textoContratacion         varchar(100) COLLATE DATABASE_DEFAULT NULL,
    horas                     decimal(5,2) NOT NULL,
    imparte                   bit NOT NULL,
    idDocente                 int NULL,
    idExperienciaEducativaPeriodo int NULL
);

CREATE TABLE #HorarioPlanea (
    idHorario             int NOT NULL,
    dia                   varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL,
    nrc                   varchar(5)   COLLATE DATABASE_DEFAULT NOT NULL,
    horaInicio            time(0) NOT NULL,
    horaFin               time(0) NOT NULL,
    edificio              varchar(50)  COLLATE DATABASE_DEFAULT NULL,
    aula                  varchar(100) COLLATE DATABASE_DEFAULT NULL,
    fechaInicio           date NULL,
    fechaFin              date NULL,
    numeroPersonalDocente varchar(10)  COLLATE DATABASE_DEFAULT NULL,
    principal             bit NOT NULL,
    idExperienciaEducativaPeriodo int NULL,
    PRIMARY KEY (idHorario, dia)
);
```
Las columnas de los `DataTable` son:
- `#EePlanea`: `codigo`, `nombre`.
- `#NrcPlanea`: `nrc`, `codigoExperiencia`, `titulo`, `codigoPlan`, `campus`, `nivel`, `region`, `area`.
- `#DocentePlanea`: `idRadoc`, `nrc`, `numeroPersonal`, `numeroPersonalNormalizado`, `nombre`, `plaza`, `puesto`, `textoPuesto`, `tipoContratacion`, `textoContratacion`, `horas`, `imparte`.
- `#HorarioPlanea`: `idHorario`, `dia`, `nrc`, `horaInicio`, `horaFin`, `edificio`, `aula`, `fechaInicio`, `fechaFin`, `numeroPersonalDocente`, `principal`.

```sql
-- ResolverReferencias
UPDATE n SET idPlanEstudios = p.idPlanEstudios
FROM #NrcPlanea AS n
INNER JOIN dbo.PlanEstudios AS p ON p.codigoPlan = n.codigoPlan;

UPDATE n SET idRegion = r.id
FROM #NrcPlanea AS n
INNER JOIN dbo.Region AS r
    ON r.nombre COLLATE Latin1_General_CI_AI = n.region COLLATE Latin1_General_CI_AI;

UPDATE d SET idDocente = x.idDocente
FROM #DocentePlanea AS d
CROSS APPLY (
    SELECT TOP (1) doc.idDocente
    FROM dbo.Docente AS doc
    WHERE doc.numeroPersonal = d.numeroPersonal
       OR doc.numeroPersonal = d.numeroPersonalNormalizado
    ORDER BY CASE WHEN doc.numeroPersonal = d.numeroPersonal THEN 0 ELSE 1 END
) AS x;
```
```sql
-- CrearExperiencias  (devuelve el número de EEs creadas)
INSERT INTO dbo.ExperienciaEducativa (idPlanEstudios, codigo, nombre, perfilDocente, creditos, horas, origen)
SELECT NULL, e.codigo, e.nombre, NULL, NULL, NULL, 'PLANEA'
FROM #EePlanea AS e
WHERE NOT EXISTS (SELECT 1 FROM dbo.ExperienciaEducativa AS x WHERE x.codigo = e.codigo);

SELECT @@ROWCOUNT;
```
```sql
-- ResolverExperiencias: primero la EE del mismo plan; si no hay, la de menor id con ese código
UPDATE n SET idExperienciaEducativa = COALESCE(
    (SELECT MIN(x.idExperienciaEducativa) FROM dbo.ExperienciaEducativa AS x
     WHERE x.codigo = n.codigoExperiencia AND x.idPlanEstudios = n.idPlanEstudios),
    (SELECT MIN(x.idExperienciaEducativa) FROM dbo.ExperienciaEducativa AS x
     WHERE x.codigo = n.codigoExperiencia))
FROM #NrcPlanea AS n;

IF EXISTS (SELECT 1 FROM #NrcPlanea WHERE idExperienciaEducativa IS NULL)
    THROW 51100, 'No se pudo resolver la experiencia educativa de al menos un NRC.', 1;
```
```sql
-- ContarBajas  (devuelve activos, bajas)
SELECT
    (SELECT COUNT(*) FROM dbo.ExperienciaEducativaPeriodo
     WHERE idPeriodo = @idPeriodo AND fechaBaja IS NULL) AS activos,
    (SELECT COUNT(*) FROM dbo.ExperienciaEducativaPeriodo AS t
     WHERE t.idPeriodo = @idPeriodo AND t.fechaBaja IS NULL
       AND NOT EXISTS (SELECT 1 FROM #NrcPlanea AS s WHERE s.nrc = t.nrc)) AS bajas;
```
```sql
-- MergeNrc
DECLARE @ahora datetime2(0) = SYSDATETIME();
DECLARE @cambios TABLE (accion nvarchar(10), bajaAnterior datetime2(0) NULL, bajaNueva datetime2(0) NULL);

WITH destino AS (
    SELECT * FROM dbo.ExperienciaEducativaPeriodo WHERE idPeriodo = @idPeriodo
)
MERGE destino AS t
USING #NrcPlanea AS s
    ON t.nrc = s.nrc
WHEN MATCHED AND (t.fechaBaja IS NOT NULL OR EXISTS (
        SELECT s.idExperienciaEducativa, s.idPlanEstudios, s.idRegion, s.codigoPlan, s.titulo, s.campus, s.nivel, s.area
        EXCEPT
        SELECT t.idExperienciaEducativa, t.idPlanEstudios, t.idRegion, t.codigoPlanPlanea, t.titulo, t.campus, t.nivel, t.area))
    THEN UPDATE SET
        idExperienciaEducativa = s.idExperienciaEducativa,
        idPlanEstudios = s.idPlanEstudios,
        idRegion = s.idRegion,
        codigoPlanPlanea = s.codigoPlan,
        titulo = s.titulo,
        campus = s.campus,
        nivel = s.nivel,
        area = s.area,
        fechaActualizacion = @ahora,
        fechaBaja = NULL
WHEN NOT MATCHED BY TARGET
    THEN INSERT (idExperienciaEducativa, idPeriodo, idPlanEstudios, idRegion, nrc, codigoPlanPlanea,
                 titulo, campus, nivel, area, fechaAlta, fechaActualizacion)
         VALUES (s.idExperienciaEducativa, @idPeriodo, s.idPlanEstudios, s.idRegion, s.nrc, s.codigoPlan,
                 s.titulo, s.campus, s.nivel, s.area, @ahora, @ahora)
WHEN NOT MATCHED BY SOURCE AND t.fechaBaja IS NULL
    THEN UPDATE SET fechaBaja = @ahora, fechaActualizacion = @ahora
OUTPUT $action, deleted.fechaBaja, inserted.fechaBaja INTO @cambios;

SELECT
    ISNULL(SUM(CASE WHEN accion = 'INSERT' THEN 1 ELSE 0 END), 0) AS nuevos,
    ISNULL(SUM(CASE WHEN accion = 'UPDATE' AND bajaAnterior IS NULL AND bajaNueva IS NULL THEN 1 ELSE 0 END), 0) AS actualizados,
    ISNULL(SUM(CASE WHEN accion = 'UPDATE' AND bajaAnterior IS NOT NULL AND bajaNueva IS NULL THEN 1 ELSE 0 END), 0) AS reactivados,
    ISNULL(SUM(CASE WHEN accion = 'UPDATE' AND bajaAnterior IS NULL AND bajaNueva IS NOT NULL THEN 1 ELSE 0 END), 0) AS bajas
FROM @cambios;
```
```sql
-- ResolverNrcHijos
UPDATE d SET idExperienciaEducativaPeriodo = t.idExperienciaEducativaPeriodo
FROM #DocentePlanea AS d
INNER JOIN dbo.ExperienciaEducativaPeriodo AS t ON t.idPeriodo = @idPeriodo AND t.nrc = d.nrc;

UPDATE h SET idExperienciaEducativaPeriodo = t.idExperienciaEducativaPeriodo
FROM #HorarioPlanea AS h
INNER JOIN dbo.ExperienciaEducativaPeriodo AS t ON t.idPeriodo = @idPeriodo AND t.nrc = h.nrc;
```
```sql
-- MergeDocentes
-- El destino se filtra con subconsulta (no JOIN) para que la CTE siga siendo actualizable y admita DELETE.
DECLARE @cambios TABLE (accion nvarchar(10));

WITH destino AS (
    SELECT * FROM dbo.ExperienciaEducativaPeriodoDocente
    WHERE idExperienciaEducativaPeriodo IN (
        SELECT idExperienciaEducativaPeriodo FROM dbo.ExperienciaEducativaPeriodo WHERE idPeriodo = @idPeriodo)
)
MERGE destino AS t
USING #DocentePlanea AS s
    ON t.idExperienciaEducativaPeriodo = s.idExperienciaEducativaPeriodo
   AND t.idRadocPlanea = s.idRadoc
WHEN MATCHED AND EXISTS (
        SELECT s.idDocente, s.numeroPersonal, s.nombre, s.plaza, s.puesto, s.textoPuesto,
               s.tipoContratacion, s.textoContratacion, s.horas, s.imparte
        EXCEPT
        SELECT t.idDocente, t.numeroPersonal, t.nombreDocente, t.plaza, t.puesto, t.textoPuesto,
               t.tipoContratacion, t.textoContratacion, t.horas, t.imparte)
    THEN UPDATE SET
        idDocente = s.idDocente, numeroPersonal = s.numeroPersonal, nombreDocente = s.nombre,
        plaza = s.plaza, puesto = s.puesto, textoPuesto = s.textoPuesto,
        tipoContratacion = s.tipoContratacion, textoContratacion = s.textoContratacion,
        horas = s.horas, imparte = s.imparte
WHEN NOT MATCHED BY TARGET
    THEN INSERT (idExperienciaEducativaPeriodo, idRadocPlanea, idDocente, numeroPersonal, nombreDocente,
                 plaza, puesto, textoPuesto, tipoContratacion, textoContratacion, horas, imparte)
         VALUES (s.idExperienciaEducativaPeriodo, s.idRadoc, s.idDocente, s.numeroPersonal, s.nombre,
                 s.plaza, s.puesto, s.textoPuesto, s.tipoContratacion, s.textoContratacion, s.horas, s.imparte)
WHEN NOT MATCHED BY SOURCE
    THEN DELETE
OUTPUT $action INTO @cambios;

SELECT
    ISNULL(SUM(CASE WHEN accion = 'INSERT' THEN 1 ELSE 0 END), 0) AS insertados,
    ISNULL(SUM(CASE WHEN accion = 'UPDATE' THEN 1 ELSE 0 END), 0) AS actualizados,
    ISNULL(SUM(CASE WHEN accion = 'DELETE' THEN 1 ELSE 0 END), 0) AS eliminados
FROM @cambios;
```
```sql
-- MergeHorarios  (mismo patrón que MergeDocentes)
DECLARE @cambios TABLE (accion nvarchar(10));

WITH destino AS (
    SELECT * FROM dbo.ExperienciaEducativaPeriodoHorario
    WHERE idExperienciaEducativaPeriodo IN (
        SELECT idExperienciaEducativaPeriodo FROM dbo.ExperienciaEducativaPeriodo WHERE idPeriodo = @idPeriodo)
)
MERGE destino AS t
USING #HorarioPlanea AS s
    ON t.idExperienciaEducativaPeriodo = s.idExperienciaEducativaPeriodo
   AND t.idHorarioPlanea = s.idHorario
   AND t.dia = s.dia
WHEN MATCHED AND EXISTS (
        SELECT s.horaInicio, s.horaFin, s.edificio, s.aula, s.fechaInicio, s.fechaFin, s.numeroPersonalDocente, s.principal
        EXCEPT
        SELECT t.horaInicio, t.horaFin, t.edificio, t.aula, t.fechaInicio, t.fechaFin, t.numeroPersonalDocente, t.principal)
    THEN UPDATE SET
        horaInicio = s.horaInicio, horaFin = s.horaFin, edificio = s.edificio, aula = s.aula,
        fechaInicio = s.fechaInicio, fechaFin = s.fechaFin,
        numeroPersonalDocente = s.numeroPersonalDocente, principal = s.principal
WHEN NOT MATCHED BY TARGET
    THEN INSERT (idExperienciaEducativaPeriodo, idHorarioPlanea, dia, horaInicio, horaFin, edificio, aula,
                 fechaInicio, fechaFin, numeroPersonalDocente, principal)
         VALUES (s.idExperienciaEducativaPeriodo, s.idHorario, s.dia, s.horaInicio, s.horaFin, s.edificio, s.aula,
                 s.fechaInicio, s.fechaFin, s.numeroPersonalDocente, s.principal)
WHEN NOT MATCHED BY SOURCE
    THEN DELETE
OUTPUT $action INTO @cambios;

SELECT
    ISNULL(SUM(CASE WHEN accion = 'INSERT' THEN 1 ELSE 0 END), 0) AS insertados,
    ISNULL(SUM(CASE WHEN accion = 'UPDATE' THEN 1 ELSE 0 END), 0) AS actualizados,
    ISNULL(SUM(CASE WHEN accion = 'DELETE' THEN 1 ELSE 0 END), 0) AS eliminados
FROM @cambios;
```
Notas:
- Cada `MERGE` **debe** terminar en `;`.
- Los hijos de los NRC dados de baja se borran porque no vienen en la fuente. Es lo acordado: los docentes y horarios se reemplazan por lo que trae PLANEA.
- La fuente de docentes y horarios siempre tiene `idExperienciaEducativaPeriodo` resuelto, porque el normalizador ya descartó los NRC huérfanos. Aun así, si alguno quedó `NULL` tras `ResolverNrcHijos`, lanza `THROW 51101, …` antes de los MERGE.

### 4.5 Checkpoint Fase 4
- `dotnet build` compila. El SQL se valida en la Fase 6 contra un SQL Server real.
- Commit: `feat(planea): agrega repositorio de sincronización set-based`.

---

## FASE 5 — Servicios, worker y registro

### 5.1 `SincronizarPeriodoPlaneaService`
```csharp
public interface ISincronizarPeriodoPlaneaService
{
    Task<ResultadoSincronizacionPlanea> SincronizarAsync(PeriodoPorSincronizar periodo, CancellationToken cancellationToken);
}
```
Recibe por constructor `IPlaneaCliente`, `ISincronizacionPlaneaRepository`, `IOptions<PlaneaOpciones>` y `ILogger<SincronizarPeriodoPlaneaService>`.

Flujo de `SincronizarAsync`:
1. `var idBitacora = await _repositorio.IniciarBitacoraAsync(periodo.IdPeriodo, ct);` e inicia un `Stopwatch`.
2. Dentro de un `try`:
   1. `var respuesta = await ObtenerConReintentosAsync(periodo.Codigo, ct);`
   2. `var recibidos = respuesta.Resultado?.Count ?? 0;`
   3. **Validación.** Si falla, lanza `PlaneaRespuestaInvalidaException` con un mensaje claro:
      - `respuesta.Periodo?.Trim() == periodo.Codigo`.
      - `respuesta.Total == recibidos`.
   4. Si `respuesta.Total == 0`: cierra la bitácora como `SinDatos` y devuelve.
   5. `var datos = PlaneaNormalizador.Normalizar(respuesta);`
   6. `var resumen = await _repositorio.AplicarAsync(periodo.IdPeriodo, periodo.Codigo, datos, _opciones.PorcentajeMaximoBajas, ct);`
   7. Cierra la bitácora como `Exitosa`, con `recibidos`, el resumen y `string.Join(Environment.NewLine, datos.Advertencias)`.
   8. `LogInformation` con el periodo, la duración y el resumen, y devuelve.
3. `catch (SincronizacionEnCursoException ex)` → cierra como `Omitida` con `ex.Message` y usa `LogWarning`.
4. `catch (OperationCanceledException) when (ct.IsCancellationRequested)` → cierra como `Interrumpida` **usando `CancellationToken.None`** y relanza.
5. `catch (Exception ex)` → cierra como `Fallida` con `ex.Message` (usando `CancellationToken.None`), hace `LogError(ex, …)` y devuelve el resultado `Fallida`. **No relanza.**

`ObtenerConReintentosAsync`:
- Hace hasta `_opciones.Intentos` intentos.
- Reintenta solo ante `HttpRequestException` o `TaskCanceledException` cuando `!ct.IsCancellationRequested` (timeout).
- La espera es `EsperaEntreIntentos * 4^(intento-1)` (30 s, 2 min…), con `Task.Delay(espera, ct)`.
- En el último intento relanza la excepción.

### 5.2 `SincronizarPeriodosVigentesService`
```csharp
public interface ISincronizarPeriodosVigentesService
{
    Task<IReadOnlyList<ResultadoSincronizacionPlanea>> EjecutarAsync(CancellationToken cancellationToken);
}
```
1. `await _repositorio.MarcarInterrumpidasAsync(_opciones.UmbralInterrumpida, ct);`
2. `var hoy = DateOnly.FromDateTime(DateTime.Today);`
3. Obtiene los periodos vigentes con `VentanaAnticipacionMeses`. Si `ContarPeriodosSinFechasAsync > 0`, hace `LogWarning`.
4. Si no hay periodos: `LogInformation` y devuelve una lista vacía.
5. Llama a `_sincronizarPeriodo.SincronizarAsync` para cada periodo **en secuencia** (sin paralelismo) y acumula los resultados. Como ese servicio no relanza errores, un fallo no detiene a los demás.

### 5.3 `Infra/SincronizacionPlaneaWorker.cs`
```csharp
public sealed class SincronizacionPlaneaWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PlaneaOpciones _opciones;
    private readonly ILogger<SincronizacionPlaneaWorker> _logger;

    public SincronizacionPlaneaWorker(IServiceScopeFactory scopeFactory, IOptions<PlaneaOpciones> opciones,
        ILogger<SincronizacionPlaneaWorker> logger)
    { _scopeFactory = scopeFactory; _opciones = opciones.Value; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opciones.Habilitada)
        {
            _logger.LogInformation("La sincronización con PLANEA está deshabilitada.");
            return;
        }

        try
        {
            await Task.Delay(_opciones.RetrasoInicial, stoppingToken);
            using var timer = new PeriodicTimer(_opciones.Intervalo);
            do
            {
                await EjecutarCicloAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // apagado normal
        }
    }

    private async Task EjecutarCicloAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();   // DbContext es scoped
            var servicio = scope.ServiceProvider.GetRequiredService<ISincronizarPeriodosVigentesService>();
            await servicio.EjecutarAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Falló el ciclo de sincronización con PLANEA.");
        }
    }
}
```

### 5.4 `SincronizacionPlaneaModuleExtensions.cs`
```csharp
public static class SincronizacionPlaneaModuleExtensions
{
    public static IServiceCollection AddSincronizacionPlaneaModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PlaneaOpciones>()
            .Bind(configuration.GetSection(PlaneaOpciones.Seccion))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IPlaneaCliente, PlaneaCliente>((proveedor, cliente) =>
            {
                var opciones = proveedor.GetRequiredService<IOptions<PlaneaOpciones>>().Value;
                cliente.BaseAddress = new Uri(opciones.UrlBase.EndsWith('/') ? opciones.UrlBase : opciones.UrlBase + "/");
                cliente.Timeout = opciones.TiempoEspera;
                cliente.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All   // gzip/deflate/brotli si PLANEA lo soporta
            });

        services.AddScoped<ISincronizacionPlaneaRepository, SincronizacionPlaneaRepository>();
        services.AddScoped<ISincronizarPeriodoPlaneaService, SincronizarPeriodoPlaneaService>();
        services.AddScoped<ISincronizarPeriodosVigentesService, SincronizarPeriodosVigentesService>();
        services.AddHostedService<SincronizacionPlaneaWorker>();

        return services;
    }
}
```
En `SGPla/Program.cs`, después de `builder.Services.AddProgramasEducativosModule();` (línea ~153), agrega:
```csharp
builder.Services.AddSincronizacionPlaneaModule(builder.Configuration);
```

### 5.5 Configuración
- **`SGPla/appsettings.json`:** agrega la sección. En producción queda habilitada.
  ```json
  "Planea": {
    "Habilitada": true,
    "UrlBase": "https://planea.uv.mx/planea/index.php/apiroladoovr/",
    "Intervalo": "1.00:00:00",
    "RetrasoInicial": "00:01:00",
    "TiempoEspera": "00:05:00",
    "Intentos": 3,
    "EsperaEntreIntentos": "00:00:30",
    "VentanaAnticipacionMeses": 6,
    "PorcentajeMaximoBajas": 30,
    "UmbralInterrumpida": "01:00:00"
  }
  ```
- **`SGPla/appsettings.Development.json`:** agrega `"Planea": { "Habilitada": false }`, para no llamar a PLANEA desde cada máquina de desarrollo.
- **`docker-compose.yml`**, servicio `backend`: agrega `Planea__Habilitada: ${PLANEA_HABILITADA:-false}`.

### 5.6 Pruebas de la Fase 5 (Moq)
Sigue el estilo de `SGPla.Tests/Modules/SolicitudesApertura/CrearSolicitudAperturaServiceTests.cs`: helpers privados `CrearService(...)`, `Mock<IPort>` y `Callback` para capturar argumentos. En las pruebas, usa `Options.Create(new PlaneaOpciones { EsperaEntreIntentos = TimeSpan.Zero, Intentos = 3 })`.

**`SincronizarPeriodoPlaneaServiceTests`:**
- `SincronizarAsync_AplicaYCierraExitosa`: respuesta válida → `AplicarAsync` se llama una vez → `CerrarBitacoraAsync(…, "Exitosa", …)`.
- `SincronizarAsync_TotalNoCoincide_NoAplicaYFalla`: `AplicarAsync` → `Times.Never`; bitácora `Fallida`.
- `SincronizarAsync_PeriodoDistinto_NoAplicaYFalla`.
- `SincronizarAsync_TotalCero_SinDatos`.
- `SincronizarAsync_EnCurso_Omitida`: `AplicarAsync` lanza `SincronizacionEnCursoException`.
- `SincronizarAsync_BajasMasivas_Fallida`.
- `SincronizarAsync_ReintentaYLuegoExito`: el cliente falla 2 veces con `HttpRequestException` y luego responde → `Exitosa`, cliente llamado 3 veces.
- `SincronizarAsync_AgotaReintentos_Fallida`.

**`SincronizarPeriodosVigentesServiceTests`:**
- Procesa 2 periodos en orden y el primero `Fallida` no impide el segundo.
- Sin periodos, no llama al servicio de periodo.
- Siempre llama a `MarcarInterrumpidasAsync`.

### 5.7 Checkpoint Fase 5
- `dotnet build` y `dotnet test` en verde.
- Arranca la app con `dotnet run --project SGPla`, que en Development deja `Habilitada=false`. El log debe mostrar "La sincronización con PLANEA está deshabilitada." y la app debe funcionar normal.
- Commits: `feat(planea): agrega servicios y worker de sincronización programada` y `test(planea): …`.

---

## FASE 6 — Verificación end-to-end (requiere Docker)

1. Levanta la BD y aplica las migraciones:
   ```bash
   docker compose up -d db
   docker compose run --rm db-init
   ```
   El seed crea los periodos `202501`…`202951`, con fechas calculadas por la migración 0002. `202701` tiene `fechaFin = 2027-07-31`, así que es vigente.
2. **Mock de PLANEA** en el scratchpad o en `/tmp`, **nunca dentro del repo**:
   ```bash
   MOCK=/tmp/planea-mock/planea/index.php/apiroladoovr/periodo
   mkdir -p $MOCK && cp /Users/kaleb/repos/sgpla/Endpoint.json $MOCK/202701
   (cd /tmp/planea-mock && python3 -m http.server 8099)   # en background
   ```
   Para que el mock no dé de baja nada por error, los demás periodos vigentes devolverán 404: el mock solo tiene `202701`. Eso es correcto: esos periodos quedan como `Fallida` con el mensaje del 404.
3. Ejecuta la app apuntando al mock:
   ```bash
   ConnectionStrings__DefaultConnection="Server=localhost,14333;Database=GestionDePlazasBD;User Id=sa;Password=SgplaDev_2026!Password;TrustServerCertificate=True;Encrypt=False;" \
   Planea__Habilitada=true \
   Planea__UrlBase=http://localhost:8099/planea/index.php/apiroladoovr/ \
   Planea__RetrasoInicial=00:00:05 \
   Planea__Intentos=1 \
   dotnet run --project SGPla
   ```
4. **Primera corrida.** Verifica con `sqlcmd` o `docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'SgplaDev_2026!Password' -d GestionDePlazasBD -Q "…"`:
   - `SELECT TOP 5 * FROM SincronizacionPlanea ORDER BY idSincronizacionPlanea DESC`: la fila de `202701` debe estar `Exitosa`, con `registrosRecibidos = 18203` y `nrcNuevos = 18022`.
   - Sin duplicados: `SELECT idPeriodo, nrc, COUNT(*) FROM ExperienciaEducativaPeriodo GROUP BY idPeriodo, nrc HAVING COUNT(*) > 1` no devuelve filas.
   - `SELECT COUNT(*) FROM ExperienciaEducativaPeriodoDocente` da **12,584**.
   - `SELECT COUNT(*) FROM ExperienciaEducativaPeriodoHorario` da unas 38,800.
   - `SELECT COUNT(*) FROM ExperienciaEducativa WHERE origen = 'PLANEA'` da ≤ 4,476. `SELECT codigo FROM ExperienciaEducativa WHERE origen='PLANEA' GROUP BY codigo HAVING COUNT(*)>1` no devuelve filas.
   - El NRC `10709` tiene 2 docentes. El NRC `10676` tiene 5 horarios, de Lunes a Viernes, 14:00–14:59.
   - `SELECT COUNT(*) FROM ExperienciaEducativaPeriodo WHERE idRegion IS NULL` da 0: todas las regiones se resolvieron.
5. **Idempotencia:** reinicia la app o baja `Planea__Intervalo` a `00:02:00` y espera un ciclo. La nueva fila de bitácora debe tener todos los contadores de cambio en 0 y los `COUNT(*)` deben seguir iguales.
6. **Cambios:**
   1. Con un script de Python, quita un NRC del JSON del mock (todas sus filas), cambia un `sec_titulo` y ajusta `total`.
   2. Tras la corrida, espera `nrcBaja = 1` y `nrcActualizados = 1`. El NRC quitado queda con `fechaBaja` y sin hijos.
   3. Restaura el JSON: espera `nrcReactivados = 1`.
7. **Protecciones:**
   - Altera `total`: la corrida queda `Fallida` y no cambian los datos.
   - Deja solo el 50 % de los NRC con su `total` coherente: la corrida queda `Fallida` por `BajasMasivasException`, con rollback y sin cambios.
8. **Rendimiento:** anota en el resumen final la duración por fase y la memoria del proceso. Estima la memoria con `dotnet-counters` si está instalado; si no, con el Monitor de actividad. Objetivo orientativo: menos de 30 s en local.
9. **Regresión:** navega en la app los flujos de avisos, programación académica y planes de estudio con los datos del seed, y confirma que no hay errores por la nulabilidad.
10. Detén el mock y la app.

Si no hay Docker o SQL Server disponible, **no marques la Fase 6 como hecha**. Deja la lista de pasos pendientes en tu resumen final.

---

## 7. Casos borde (referencia rápida)

| Caso | Comportamiento esperado |
|---|---|
| NRC con varios docentes | 1 fila de NRC + N filas de docente |
| Re-ejecución idéntica | 0 cambios (`EXCEPT` detecta que no hay diferencias) |
| NRC que desaparece | `fechaBaja` y sus hijos borrados |
| NRC que reaparece | `fechaBaja = NULL` y cuenta como reactivado |
| Plan inexistente o `sec_programa` nulo | `idPlanEstudios` NULL, `codigoPlanPlanea` guardado. Se enlaza en una corrida posterior cuando el plan exista (el MERGE actualiza la FK) |
| Docente no registrado | `idDocente` NULL y los datos crudos se conservan |
| EE ya existe en el catálogo | Se reutiliza y **no se modifica** |
| Respuesta parcial, periodo distinto, JSON inválido, HTTP 5xx | `Fallida` sin tocar datos |
| Más del 30 % de bajas | Rollback y `Fallida` |
| Dos ejecuciones simultáneas | La segunda queda `Omitida` (`sp_getapplock`) |
| Caída a mitad de una ejecución | Rollback automático; la bitácora pasa a `Interrumpida` en el siguiente ciclo |

## 8. Qué NO hacer
- **No** agregues un controlador, una vista ni un endpoint: el disparo es solo automático.
- **No** modifiques `CargaAcademica`, `Oferta` ni `Horario`. La importación por Excel sigue igual; solo cambia el filtro de §2.4.
- **No** actualices nombre, perfil, créditos ni horas de EEs que ya existen en el catálogo.
- **No** borres físicamente NRC.
- **No** corrijas las fechas de `Periodo` (§10).
- **No** uses `SaveChanges` ni `AddRange` de EF para los datos masivos.
- **No** cargues la respuesta HTTP como `string` (`ReadAsStringAsync`) ni uses `JsonDocument` sobre el payload completo.
- **No** subas `Endpoint.json` ni el mock al repo.

## 9. Resumen final que debe entregar el implementador
- Lista de commits.
- Resultado de `dotnet build` y `dotnet test`.
- Resultado de cada paso de la Fase 6, o cuáles quedaron pendientes y por qué.
- Cualquier desviación de este plan y su motivo.

## 10. Hallazgos fuera de alcance (solo documentar)
- **Fechas de `Periodo`:** la migración `0002` asume «sufijo `01` = feb–jul, `51` = ago–ene». PLANEA muestra que `202701` va del 17 de agosto al 2 de diciembre de 2026, así que la regla parece estar invertida. La selección de periodos vigentes depende de esas fechas; conviene corregirlas en otra tarea.
- **Borrar EEs de un plan:** si un plan de estudios elimina una EE que ya tiene NRC sincronizados, la FK lo impide. Hoy pasa lo mismo con `Oferta`. Queda pendiente mostrar un mensaje amigable.
- **Consultar sincronizaciones:** no hay pantalla ni endpoint para ver la bitácora o forzar una sincronización; queda como posible siguiente iteración.
