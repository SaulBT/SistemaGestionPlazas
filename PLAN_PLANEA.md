# PLAN_PLANEA — Sincronización automática de la programación académica desde PLANEA

> **Para el agente implementador.** Este documento es autocontenido. Síguelo en orden, fase por fase.
> Al terminar cada fase, ejecuta su **checkpoint**. No avances a la siguiente fase si el checkpoint falla.
> Las decisiones de diseño de la §1 ya están tomadas con el usuario: **no las cambies**.
> Si algo del código real no coincide con lo que dice este plan (un nombre, una línea), **investígalo en el código**, adáptate y anótalo en tu resumen final.
>
> **Base de código:** commit `37625fe4aeafc0f5a0554f23f47b96fb9db8dbf2` ("Feature/cargar planes estudio (#24)"). La última migración aplicada es la `0011`.
> **NO** uses `origin/develop` ni commits posteriores: tienen otro esquema (`academico.*`, `integracion.*`, migraciones `0020`+) que **no** es la base de este trabajo. Ignora cualquier `DATABASE.md` o `DATABASE_DIAGRAM.md`.
>
> **Alcance técnico: solo el MVC (ASP.NET Core MVC + vistas Razor).** Queda fuera:
> - el cliente React (`sgpla-app/`);
> - la API REST (`SGPla/Modules/*` y cualquier controlador `[ApiController]` o ruta `/api/...`).
>
> No crees ni modifiques nada en `SGPla/Modules/` ni en `sgpla-app/`.

---

## 0. Reglas de trabajo

- **Raíz del repo:** `/Users/kaleb/repos/sgpla/SistemaGestionPlazas`.
  - Trabaja en la rama `feature/sincronizacion-planea`. Ya existe, parte de `37625fe` y solo contiene commits `docs(planea)` con este plan.
  - **No** hagas `pull`, `rebase` ni `merge` con `origin/develop`.
- **No hagas push ni abras PR** sin que el usuario lo pida.
  - Commits en español, estilo convencional: `feat(db): …`, `feat(planea): …`, `test(planea): …`.
  - Termina cada mensaje de commit con la línea `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.
- **Stack:**
  - .NET 10 (SDK 10.0.400) y ASP.NET Core MVC.
  - EF Core 10 Database First sobre SQL Server (`GestionDePlazasDbContext`). Las migraciones son SQL y las aplica **DbUp** (`SGPla.DbMigrator`).
  - Pruebas: xUnit 2.9 + Moq 4.20.
  - `Nullable` está habilitado en ambos proyectos.
- **No agregues paquetes NuGet.** Todo lo necesario ya está disponible:
  - `Microsoft.Data.SqlClient`, transitivo de `Microsoft.EntityFrameworkCore.SqlServer`.
  - `AddHttpClient`, `IOptions` y `ValidateDataAnnotations`, del framework compartido de ASP.NET Core.
  - `System.Text.Json`.
- **Convenciones del código.** Imita la **capa MVC** existente: usa como referencia `PeriodosEscolares` y `ProgramacionesAcademicas`, **no** `SGPla/Modules`.
  - Nombres en español.
  - `CancellationToken cancellationToken` en los métodos async nuevos (con valor por defecto en las interfaces).
  - Capas y carpetas:
    - Controladores: `SGPla/Controllers/XxxController.cs`, que heredan de `Controller` y se protegen con `[Authorize(Policy = PoliticasAutorizacion.…)]`.
    - Servicios: interfaz en `SGPla/Services/Interfaces/IXxxService.cs` e implementación en `SGPla/Services/Implementations/XxxService.cs`.
    - Repositorios: `SGPla/Repositories/Interfaces/IXxxRepository.cs` y `SGPla/Repositories/Implementations/XxxRepository.cs`, sobre `GestionDePlazasDbContext`.
    - DTOs: `SGPla/Models/DTOs/<Tema>/…`.
    - ViewModels: `SGPla/Models/ViewModels/<Tema>/…`.
    - Vistas: `SGPla/Views/<Controlador>/…`, usando los ViewComponents existentes (`Table`, `Buscador`, `SelectField`, `Boton`, `Modal`, `Toast`) y `TablaFactory`.
    - Parsers y normalizadores de datos externos: `SGPla/Parsers/`.
    - Constantes, opciones y excepciones: `SGPla/Commons/`.
  - Namespaces: sigue el estilo de cada carpeta. La capa MVC usa **namespaces con bloque** (`namespace SGPla.Services.Implementations { … }`); los modelos EF usan `namespace SGPla.Models;`. Los fragmentos de este plan usan namespaces *file-scoped* por brevedad.
  - Registro de dependencias: con `builder.Services.AddScoped<…>()` en `SGPla/Program.cs`, en el bloque `//Clases`, como el resto del MVC. **No** crees un `XxxModuleExtensions`.
- **Comandos:**
  - Compilar: `dotnet build SGPla/SGPla.csproj`
  - Probar: `dotnet test SGPla.Tests/SGPla.Tests.csproj`
  - Compilar el migrador: `dotnet build SGPla.DbMigrator/SGPla.DbMigrator.csproj`
- **Archivo de datos real** para análisis y fixtures: `/Users/kaleb/repos/sgpla/Endpoint.json` (65 MB, fuera del repo). **No lo copies al repo.**
  - Úsalo solo para generar el fixture (script de §3.7) y para el mock de la FASE 7.

### 0.1 Antes de empezar (obligatorio)
1. `git switch feature/sincronizacion-planea`.
2. Ejecuta `git log --oneline -5`:
   - Todos los commits por encima de `37625fe` ("Feature/cargar planes estudio (#24)") deben ser `docs(planea): …`.
   - Si aparece cualquier otro commit entre ellos, o si `37625fe` no está en la historia, **DETENTE** y avisa al usuario.
3. Confirma que la última migración es `SGPla.DbMigrator/database/migrations/0011_conservar_docentes_importados.sql` y que no existen `0012`+.
4. Lee este documento **completo** antes de escribir código, en especial:
   - §1: modelo de dominio y decisiones cerradas.
   - §12: qué NO hacer.
5. **Línea base.** Ejecuta `dotnet build SGPla/SGPla.csproj` y `dotnet test SGPla.Tests/SGPla.Tests.csproj`. Anota qué pruebas ya fallaban antes de tus cambios. En cada checkpoint, "tests en verde" significa **ningún fallo nuevo** respecto a esa línea base.

### 0.2 Reglas durante la implementación
- Ejecuta las fases en orden (FASE 1 → FASE 7). Cierra cada una con su checkpoint y su commit, usando los mensajes sugeridos.
- Usa **tal cual** el SQL de la migración `0012` (FASE 1) y el de `SincronizacionPlaneaSql` (FASE 4). Solo corrige errores de sintaxis reales y documéntalos como desviación.
- Si el código real no coincide con el plan (una línea, un nombre, una firma), investígalo, adáptate a lo que existe y anótalo como desviación. **No inventes APIs** ni métodos que no existan.
- Si no hay Docker ni SQL Server disponible:
  - Termina las fases 1–6, con build y pruebas unitarias.
  - Deja la FASE 7 y el checkpoint 6.6 como **pendientes**.
  - **No simules resultados** ni los describas como ejecutados.

---

## 1. Contexto, modelo de dominio y decisiones (NO modificar)

PLANEA expone `GET https://planea.uv.mx/planea/index.php/apiroladoovr/periodo/{codigoPeriodo}` (ej. `202701`). Devuelve **toda la programación académica del periodo, de todos los programas educativos y todas las regiones**. La respuesta pesa unos 65 MB y tiene unos 1.9 M de líneas.

Queremos guardar en nuestra BD lo que nos corresponde, para no consultar el servicio en cada uso. Debe leerse de forma eficiente y **sin duplicados**.

### 1.1 Modelo de dominio
- **`ExperienciaEducativa` (EE) es el catálogo.**
  - La **única fuente de verdad** es el caso de uso **"Cargar plan de estudios"** (`PlanEstudiosService` / `PlanEstudiosImportador`).
  - Cada EE pertenece a un plan (`idPlanEstudios`) y su `codigo` tiene el formato `"MATERIA CURSO"` (ej. `CVCB 18003`, regex `^[A-Z]{4} [0-9]{5}$`).
  - **La sincronización NUNCA crea ni modifica EEs, planes ni programas.**
- **Lo que trae PLANEA es una "copia" de la EE en un periodo concreto, identificada por su NRC** (una sección).
  - Esa copia tiene asociados su **horario** y su **espacio** (edificio/aula).
  - La guardamos en la nueva tabla ternaria **`ExperienciaEducativaPeriodo`** (EE × Periodo × PlanEstudios), con clave única **`(idPeriodo, nrc)`**.
- **Cómo se enlaza la copia con su EE del catálogo:**
  - Se combinan `radoc_materia` + `radoc_curso` + código de plan (`sec_programa`).
  - Se busca la EE cuyo `codigo = MATERIA + " " + CURSO` y cuyo plan tenga `PlanEstudios.codigoPlan = sec_programa`.
  - `PlanEstudios.codigoPlan` existe desde la migración `0010` y tiene índice único filtrado `UQ_PlanEstudios_codigoPlan`.

### 1.2 Decisiones
| Tema | Decisión |
|---|---|
| Qué se guarda | Solo la **copia por periodo** (NRC) y sus **horarios con espacio**. **No** se guardan docentes en esta fase |
| Copia cuya EE no existe en el catálogo | **Se omite**: no se guarda y se cuenta en la bitácora. Incluye plan no cargado, EE no registrada en ese plan y `sec_programa` nulo |
| Copia ya registrada (`idPeriodo` + `nrc` existe) | **Se omite** por completo: no se actualiza ni se agregan horarios |
| Copia nueva con EE en el catálogo | **Se registra** junto con todos sus horarios |
| Actualizaciones y bajas | **No hay.** La sincronización solo inserta copias nuevas; nunca actualiza ni borra |
| Enlaces futuros | Si después se carga un plan, la siguiente sincronización registra las copias que antes se omitían, porque aún no existen |
| Disparo | **Automático programado** (`BackgroundService`). No hay botón para sincronizar manualmente |
| Qué periodos | **Vigentes por fecha**: `fechaInicio` y `fechaFin` no nulos, `fechaFin >= hoy` y `fechaInicio <= hoy + VentanaAnticipacionMeses` |
| Alcance técnico | **Solo MVC + vistas Razor**. No se toca `sgpla-app/` (React) ni la API REST |
| Front MVC | Pantallas de **consulta** (solo lectura) para **SuperUsuario** (FASE 6): bitácora de sincronizaciones, listado de copias (NRC) registradas y detalle de una copia con sus horarios. Lleva un enlace en el menú de `_LayoutSuperUsuario` |
| Fechas de `Periodo` | Pueden estar mal calculadas o nulas (ver §14), pero **su corrección está fuera de alcance** |

---

## 2. Perfil de la respuesta de PLANEA (medido sobre `Endpoint.json`, periodo 202701)

- **Raíz**, en este orden: `periodo` (string), `total` (**número JSON**, igual a la cantidad de filas de `resultado`), `resultado` (array de 18,203) y `horarios` (array de 22,623). El resto de los valores son **strings** o `null`.
- **`resultado`**
  - Cada fila es una asignación docente-NRC. Hay **18,022 NRC distintos**; 152 NRC tienen de 2 a 4 filas, una por docente.
  - Para esta fase solo interesan los datos de la **sección**, que nunca difieren entre filas del mismo NRC: `radoc_nrc`, `radoc_materia`, `radoc_curso`, `sec_programa`, `sec_titulo`, `sec_campus`, `nivel`, `Region` y `Area`.
  - Los campos de docente y descarga (`ID_TITULAR`, `radoc_nombre`, `radoc_plaza`, `des_*`, `ddes_*`, …) **se ignoran**.
- **Código de EE:** `MATERIA + " " + CURSO`, por ejemplo `CVCB 18003`.
  - Hay 4,476 códigos distintos.
  - `MVZ 80001` y `MVZ 80002` tienen materia de 3 letras. No pueden existir en el catálogo por su regex, así que se omitirán de forma natural.
  - El título (`sec_titulo`) viene truncado en algunas filas (`DERECHO CONSTITUCIONAL MEX`). Se guarda tal cual en la copia; el nombre oficial es el de la EE del catálogo.
- **`sec_programa`** (ej. `CIVI-20-E-CR`, máximo 12 caracteres) es el código del plan. Viene `null` en **900 NRC**; esos NRC se omiten.
  - Un mismo código de plan aparece en varias regiones (62 de 343; `CIVI-20-E-CR` está en las 5). Como `codigoPlan` es único en nuestra BD, todas esas copias se enlazan al mismo plan. Esa es la regla acordada (ver §14).
- **`horarios`**
  - Cada fila es un bloque con `rhs_id` único, `NRC`, `EDIFICIO`, `AULA`, `FECHA_INICIO`, `FECHA_FIN` y las columnas `LUN_INI`/`LUN_FIN` … `SAB_INI`/`SAB_FIN` en formato `"HHmm"`. No hay domingo.
  - Todas las horas son válidas y siempre `FIN > INI`.
  - 1,144 bloques no tienen ningún día. 3 NRC de `horarios` no están en `resultado` (`25108`, `49268`, `93562`).
  - Expandidos por día producen **38,801** filas. Hay bloques distintos (por ejemplo, uno por docente) con el mismo día, horas, espacio y fechas; sin docentes son duplicados y se descartan.
- **Longitudes máximas:** `sec_titulo` 108, `Area` 53, `Region` 24, `EDIFICIO` 7, `AULA` 10, `sec_campus` 2, `nivel` 2.
- **Regiones:** llegan como `XALAPA`, `VERACRUZ`, `ORIZABA-CÓRDOBA`, `POZA RICA-TUXPAN` y `COATZACOALCOS-MINATITLÁN`. `dbo.Region.nombre` (migración 0009) las guarda como `Xalapa`, `Poza Rica-Túxpan`, etc., así que se comparan con `COLLATE Latin1_General_CI_AI`.
- **Días en BD:** usa los mismos nombres que `dbo.Horario`: `"Lunes"`, `"Martes"`, `"Miercoles"`, `"Jueves"`, `"Viernes"` y `"Sabado"`, **sin acentos** (ver `SGPla/Repositories/Implementations/ProgramacionAcademicaRepository.cs:241-246`).
- **Muestra para la verificación (FASE 7),** con `CVCB 18003` (FISICA) y `CVCB 18004` (GEOMETRIA ANALITICA) bajo el plan `CIVI-20-E-CR`:
  - Hay **33 NRC**, que generan **109 horarios** sin duplicados.
  - El NRC `10676` (CVCB 18003, región Veracruz) tiene 5 horarios, de Lunes a Viernes, 14:00–14:59, en edificio `I-INEB` y aula `I-02`.
  - Si luego se agrega `CVCB 18005` (QUIMICA) al plan, aparecen **16 NRC** más, con **68 horarios**.

---

## 3. Mapa de archivos

### Crear
```
SGPla.DbMigrator/database/migrations/0012_sincronizacion_planea.sql

# Modelos EF (Database First, mantenidos a mano)
SGPla/Models/ExperienciaEducativaPeriodo.cs
SGPla/Models/ExperienciaEducativaPeriodoHorario.cs
SGPla/Models/SincronizacionPlanea.cs

# Configuración, constantes y excepciones
SGPla/Commons/PlaneaOpciones.cs
SGPla/Commons/PlaneaConstantes.cs
SGPla/Commons/PlaneaExcepciones.cs

# DTOs
SGPla/Models/DTOs/Planea/PlaneaRespuesta.cs                 (DTOs JSON)
SGPla/Models/DTOs/Planea/PlaneaModelos.cs                   (records normalizados)
SGPla/Models/DTOs/Planea/ResumenAplicacionPlanea.cs
SGPla/Models/DTOs/Planea/ResultadoSincronizacionPlanea.cs
SGPla/Models/DTOs/Planea/ConsultaPlaneaDtos.cs              (filtros y filas para el front, FASE 6)

# Lectura y normalización
SGPla/Parsers/PlaneaNormalizador.cs

# Servicios
SGPla/Services/Interfaces/IPlaneaCliente.cs
SGPla/Services/Implementations/PlaneaCliente.cs
SGPla/Services/Interfaces/ISincronizarPeriodoPlaneaService.cs
SGPla/Services/Implementations/SincronizarPeriodoPlaneaService.cs
SGPla/Services/Interfaces/ISincronizarPeriodosVigentesService.cs
SGPla/Services/Implementations/SincronizarPeriodosVigentesService.cs
SGPla/Services/Implementations/SincronizacionPlaneaWorker.cs   (BackgroundService)
SGPla/Services/Interfaces/IConsultaPlaneaService.cs            (FASE 6)
SGPla/Services/Implementations/ConsultaPlaneaService.cs        (FASE 6)

# Repositorios
SGPla/Repositories/Interfaces/ISincronizacionPlaneaRepository.cs
SGPla/Repositories/Implementations/SincronizacionPlaneaRepository.cs
SGPla/Repositories/Implementations/SincronizacionPlaneaSql.cs   (constantes SQL)
SGPla/Repositories/Interfaces/IConsultaPlaneaRepository.cs      (FASE 6)
SGPla/Repositories/Implementations/ConsultaPlaneaRepository.cs  (FASE 6)

# Front MVC (FASE 6)
SGPla/Controllers/SincronizacionPlaneaController.cs
SGPla/Models/ViewModels/SincronizacionPlanea/IndexViewModel.cs
SGPla/Models/ViewModels/SincronizacionPlanea/CopiasViewModel.cs
SGPla/Models/ViewModels/SincronizacionPlanea/DetalleCopiaViewModel.cs
SGPla/Views/SincronizacionPlanea/Index.cshtml
SGPla/Views/SincronizacionPlanea/Copias.cshtml
SGPla/Views/SincronizacionPlanea/DetalleCopia.cshtml

# Pruebas
SGPla.Tests/Fixtures/planea_muestra.json
SGPla.Tests/Parsers/PlaneaNormalizadorTests.cs
SGPla.Tests/Services/PlaneaClienteTests.cs
SGPla.Tests/Services/SincronizarPeriodoPlaneaServiceTests.cs
SGPla.Tests/Services/SincronizarPeriodosVigentesServiceTests.cs
SGPla.Tests/Services/ConsultaPlaneaServiceTests.cs
```

### Modificar
```
SGPla/Models/ExperienciaEducativa.cs, Periodo.cs, PlanEstudios.cs, Region.cs   (solo colecciones inversas)
SGPla/Data/GestionDePlazasDbContext.cs          (DbSets + mapeos)
SGPla/Program.cs                                (AddOptions/AddHttpClient/AddScoped/AddHostedService en el bloque //Clases)
SGPla/Views/Shared/_LayoutSuperUsuario.cshtml   (enlace de menú "Sincronización PLANEA")
SGPla/appsettings.json, SGPla/appsettings.Development.json, docker-compose.yml
SGPla.Tests/SGPla.Tests.csproj                  (copiar fixture al output)
SGPla.Tests/Security/AutorizacionControllersTests.cs  (nuevo controlador → SuperUsuario)
```
**No se modifica** el esquema ni las propiedades de `ExperienciaEducativa`, `PlanEstudios`, `Oferta`, `Horario` ni `CargaAcademica`.

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
    - SincronizacionPlanea: bitácora de ejecuciones.
    - ExperienciaEducativaPeriodo: copia de una EE del catálogo en un periodo,
      identificada por NRC (tabla ternaria EE x Periodo x PlanEstudios).
    - ExperienciaEducativaPeriodoHorario: horario y espacio de cada copia.
    El catálogo (ExperienciaEducativa, PlanEstudios) NO se modifica: su fuente
    de verdad es la carga de planes de estudio.
*/

/* Índice de apoyo para enlazar copias con el catálogo por código de EE. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativa_codigo' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativa'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativa_codigo]
        ON [dbo].[ExperienciaEducativa] ([codigo] ASC)
        INCLUDE ([idPlanEstudios]);
GO

/* 1. Bitácora. */
IF OBJECT_ID(N'dbo.SincronizacionPlanea', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SincronizacionPlanea]
    (
        [idSincronizacionPlanea] [int] IDENTITY(1,1) NOT NULL,
        [idPeriodo]              [int] NOT NULL,
        [fechaInicio]            [datetime2](0) NOT NULL,
        [fechaFin]               [datetime2](0) NULL,
        [estado]                 [varchar](15) NOT NULL,
        [registrosRecibidos]     [int] NULL,   -- filas de "resultado"
        [nrcRecibidos]           [int] NULL,   -- NRC distintos
        [nrcSinPlan]             [int] NULL,   -- omitidos: sec_programa nulo
        [nrcSinExperiencia]      [int] NULL,   -- omitidos: EE/plan no está en el catálogo
        [nrcExistentes]          [int] NULL,   -- omitidos: copia ya registrada
        [nrcNuevos]              [int] NULL,   -- copias registradas en esta ejecución
        [horariosInsertados]     [int] NULL,
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

/* 2. Copia de la EE en el periodo (NRC). */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodo]
    (
        [idExperienciaEducativaPeriodo] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativa]        [int] NOT NULL,
        [idPeriodo]                     [int] NOT NULL,
        [idPlanEstudios]                [int] NOT NULL,
        [idRegion]                      [int] NULL,
        [idSincronizacionPlanea]        [int] NOT NULL,   -- ejecución que la registró
        [nrc]                           [varchar](5)   NOT NULL,
        [titulo]                        [varchar](150) NOT NULL,   -- sec_titulo tal cual
        [campus]                        [varchar](5)   NULL,
        [nivel]                         [varchar](5)   NULL,
        [area]                          [varchar](100) NULL,
        [fechaAlta]                     [datetime2](0) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_fechaAlta] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ExperienciaEducativaPeriodo] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodo] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodo_periodo_nrc] UNIQUE NONCLUSTERED ([idPeriodo] ASC, [nrc] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_ExperienciaEducativa]
            FOREIGN KEY ([idExperienciaEducativa]) REFERENCES [dbo].[ExperienciaEducativa] ([idExperienciaEducativa]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Periodo]
            FOREIGN KEY ([idPeriodo]) REFERENCES [dbo].[Periodo] ([idPeriodo]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_PlanEstudios]
            FOREIGN KEY ([idPlanEstudios]) REFERENCES [dbo].[PlanEstudios] ([idPlanEstudios]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_Region]
            FOREIGN KEY ([idRegion]) REFERENCES [dbo].[Region] ([id]),
        CONSTRAINT [FK_ExperienciaEducativaPeriodo_SincronizacionPlanea]
            FOREIGN KEY ([idSincronizacionPlanea]) REFERENCES [dbo].[SincronizacionPlanea] ([idSincronizacionPlanea])
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
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_idSincronizacionPlanea' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_idSincronizacionPlanea]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idSincronizacionPlanea] ASC);
GO

/* 3. Horario y espacio de cada copia: una fila por día. */
IF OBJECT_ID(N'dbo.ExperienciaEducativaPeriodoHorario', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExperienciaEducativaPeriodoHorario]
    (
        [idExperienciaEducativaPeriodoHorario] [int] IDENTITY(1,1) NOT NULL,
        [idExperienciaEducativaPeriodo]        [int] NOT NULL,
        [idHorarioPlanea]                      [int] NOT NULL,   -- rhs_id de referencia
        [dia]                                  [varchar](10)  NOT NULL,
        [horaInicio]                           [time](0) NOT NULL,
        [horaFin]                              [time](0) NOT NULL,
        [edificio]                             [varchar](50)  NULL,
        [aula]                                 [varchar](100) NULL,
        [fechaInicio]                          [date] NULL,
        [fechaFin]                             [date] NULL,
        CONSTRAINT [PK_ExperienciaEducativaPeriodoHorario] PRIMARY KEY CLUSTERED ([idExperienciaEducativaPeriodoHorario] ASC),
        CONSTRAINT [UX_ExperienciaEducativaPeriodoHorario_sesion]
            UNIQUE NONCLUSTERED ([idExperienciaEducativaPeriodo] ASC, [dia] ASC, [horaInicio] ASC, [horaFin] ASC,
                                 [edificio] ASC, [aula] ASC, [fechaInicio] ASC, [fechaFin] ASC),
        CONSTRAINT [FK_ExperienciaEducativaPeriodoHorario_ExperienciaEducativaPeriodo]
            FOREIGN KEY ([idExperienciaEducativaPeriodo])
            REFERENCES [dbo].[ExperienciaEducativaPeriodo] ([idExperienciaEducativaPeriodo]) ON DELETE CASCADE,
        CONSTRAINT [CK_ExperienciaEducativaPeriodoHorario_horas] CHECK ([horaInicio] < [horaFin])
    );
END
GO
```
La única FK con `ON DELETE CASCADE` es la del horario hacia su copia. El resto usa `NO ACTION`, como el esquema actual.

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

## FASE 2 — Modelos EF y DbContext

### 2.1 Modelos nuevos
Usa el estilo de `SGPla/Models/CargaAcademica.cs`: `namespace SGPla.Models;` y `public partial class`.

```csharp
// ExperienciaEducativaPeriodo.cs
public partial class ExperienciaEducativaPeriodo
{
    public int IdExperienciaEducativaPeriodo { get; set; }
    public int IdExperienciaEducativa { get; set; }
    public int IdPeriodo { get; set; }
    public int IdPlanEstudios { get; set; }
    public int? IdRegion { get; set; }
    public int IdSincronizacionPlanea { get; set; }
    public string Nrc { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string? Campus { get; set; }
    public string? Nivel { get; set; }
    public string? Area { get; set; }
    public DateTime FechaAlta { get; set; }

    public virtual ExperienciaEducativa IdExperienciaEducativaNavigation { get; set; } = null!;
    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
    public virtual Region? IdRegionNavigation { get; set; }
    public virtual SincronizacionPlanea IdSincronizacionPlaneaNavigation { get; set; } = null!;
    public virtual ICollection<ExperienciaEducativaPeriodoHorario> Horarios { get; set; } = new List<ExperienciaEducativaPeriodoHorario>();
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
    public int? NrcRecibidos { get; set; }
    public int? NrcSinPlan { get; set; }
    public int? NrcSinExperiencia { get; set; }
    public int? NrcExistentes { get; set; }
    public int? NrcNuevos { get; set; }
    public int? HorariosInsertados { get; set; }
    public string? Advertencias { get; set; }
    public string? MensajeError { get; set; }

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;
    public virtual ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo { get; set; } = new List<ExperienciaEducativaPeriodo>();
}
```

**Colecciones inversas.** Solo se agregan propiedades de navegación; **ninguna propiedad existente cambia**:
- `ExperienciaEducativa`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo`.
- `Periodo`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo` y `ICollection<SincronizacionPlanea> SincronizacionPlanea`.
- `PlanEstudios`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo`.
- `Region`: `ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo`.

### 2.2 `SGPla/Data/GestionDePlazasDbContext.cs`
- **DbSets:** `ExperienciaEducativaPeriodo`, `ExperienciaEducativaPeriodoHorario` y `SincronizacionPlanea`, con el patrón `public virtual DbSet<X> X { get; set; }`.
- **Bloque existente de `ExperienciaEducativa`** (línea ~509): agrega `entity.HasIndex(e => e.Codigo, "IX_ExperienciaEducativa_codigo");`. No cambies nada más.
- **Mapeos nuevos:** agrégalos **al final de `OnModelCreating`**, antes del cierre del método (el archivo no tiene `OnModelCreatingPartial`). Sigue el estilo de `CargaAcademica`:
  - `HasKey`, y `HasColumnName` en **camelCase** exactamente como en el SQL.
  - `HasMaxLength` e `IsUnicode(false)` en los `varchar`; `HasColumnType("time(0)")` y `HasColumnType("datetime2(0)")` donde corresponda.
  - `ToTable("…")` con el nombre de cada tabla.
  - `HasDefaultValueSql("(sysdatetime())")` en `fechaAlta`.
  - Índices:
    - `HasIndex(e => new { e.IdPeriodo, e.Nrc }, "UX_ExperienciaEducativaPeriodo_periodo_nrc").IsUnique()`.
    - `HasIndex(e => new { e.IdExperienciaEducativaPeriodo, e.Dia, e.HoraInicio, e.HoraFin, e.Edificio, e.Aula, e.FechaInicio, e.FechaFin }, "UX_ExperienciaEducativaPeriodoHorario_sesion").IsUnique()`.
    - Los índices simples de la migración.
  - Relaciones:
    - Horario → copia: `.OnDelete(DeleteBehavior.Cascade)`.
    - Todas las demás: `.OnDelete(DeleteBehavior.ClientSetNull)`.
    - Usa `.HasConstraintName("FK_…")` con los mismos nombres del SQL.
    - `Region` mapea su PK a la columna `id`; usa `.HasForeignKey(d => d.IdRegion)`.

### 2.3 Checkpoint Fase 2
- `dotnet build SGPla/SGPla.csproj` compila con 0 errores y sin warnings nuevos en los archivos tocados.
- `dotnet test SGPla.Tests/SGPla.Tests.csproj`: ningún fallo nuevo respecto a la línea base.
- Commit: `feat(db): agrega modelos EF para la sincronización con PLANEA`.

---

## FASE 3 — DTOs, normalizador y cliente HTTP

| Pieza | Archivo | Namespace |
|---|---|---|
| Opciones, constantes y excepciones | `SGPla/Commons/PlaneaOpciones.cs`, `PlaneaConstantes.cs`, `PlaneaExcepciones.cs` | `SGPla.Commons` |
| DTOs JSON y records normalizados | `SGPla/Models/DTOs/Planea/*.cs` | `SGPla.Models.DTOs.Planea` |
| Normalizador | `SGPla/Parsers/PlaneaNormalizador.cs` | `SGPla.Parsers` |
| Cliente HTTP | `SGPla/Services/Interfaces/IPlaneaCliente.cs` y `SGPla/Services/Implementations/PlaneaCliente.cs` | `SGPla.Services.Interfaces` / `SGPla.Services.Implementations` |

### 3.1 `SGPla/Commons/PlaneaOpciones.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace SGPla.Commons;

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

    public TimeSpan UmbralInterrumpida { get; set; } = TimeSpan.FromHours(1);
}
```

### 3.2 `SGPla/Commons/PlaneaConstantes.cs` y `SGPla/Commons/PlaneaExcepciones.cs`
```csharp
public static class PlaneaConstantes
{
    public const string ESTADO_EN_PROCESO = "EnProceso";
    public const string ESTADO_EXITOSA = "Exitosa";
    public const string ESTADO_SIN_DATOS = "SinDatos";
    public const string ESTADO_OMITIDA = "Omitida";
    public const string ESTADO_FALLIDA = "Fallida";
    public const string ESTADO_INTERRUMPIDA = "Interrumpida";

    public static readonly IReadOnlyList<string> ESTADOS =
        [ESTADO_EN_PROCESO, ESTADO_EXITOSA, ESTADO_SIN_DATOS, ESTADO_OMITIDA, ESTADO_FALLIDA, ESTADO_INTERRUMPIDA];

    public static readonly IReadOnlyList<string> DIAS = ["Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"];

    public const int LONGITUD_NRC = 5;
    public const int LONGITUD_CODIGO_EE = 10;
    public const int LONGITUD_CODIGO_PLAN = 50;
    public const int LONGITUD_TITULO = 150;
}

public sealed class PlaneaRespuestaInvalidaException(string mensaje) : Exception(mensaje);
public sealed class SincronizacionEnCursoException(string codigoPeriodo)
    : Exception($"Ya hay una sincronización en curso para el periodo {codigoPeriodo}.");
```

### 3.3 `SGPla/Models/DTOs/Planea/PlaneaRespuesta.cs` (DTOs JSON ligeros)
Mapea **solo** estas propiedades. System.Text.Json ignora las demás al leer, sin crear strings para ellas.
```csharp
using System.Text.Json.Serialization;

namespace SGPla.Models.DTOs.Planea;

public sealed class PlaneaRespuesta
{
    [JsonPropertyName("periodo")] public string? Periodo { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("resultado")] public List<PlaneaSeccion>? Resultado { get; set; }
    [JsonPropertyName("horarios")] public List<PlaneaHorario>? Horarios { get; set; }
}

public sealed class PlaneaSeccion
{
    [JsonPropertyName("radoc_nrc")] public string? Nrc { get; set; }
    [JsonPropertyName("radoc_materia")] public string? Materia { get; set; }
    [JsonPropertyName("radoc_curso")] public string? Curso { get; set; }
    [JsonPropertyName("sec_programa")] public string? CodigoPlan { get; set; }
    [JsonPropertyName("sec_titulo")] public string? Titulo { get; set; }
    [JsonPropertyName("sec_campus")] public string? Campus { get; set; }
    [JsonPropertyName("nivel")] public string? Nivel { get; set; }
    [JsonPropertyName("Region")] public string? Region { get; set; }
    [JsonPropertyName("Area")] public string? Area { get; set; }
}

public sealed class PlaneaHorario
{
    [JsonPropertyName("NRC")] public string? Nrc { get; set; }
    [JsonPropertyName("rhs_id")] public string? IdHorario { get; set; }
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

### 3.4 `SGPla/Models/DTOs/Planea/PlaneaModelos.cs`, `ResumenAplicacionPlanea.cs` y `ResultadoSincronizacionPlanea.cs`
```csharp
namespace SGPla.Models.DTOs.Planea;

public sealed record PeriodoPorSincronizar(int IdPeriodo, string Codigo);

/// Copia normalizada de una EE en el periodo (una por NRC, con código de plan).
public sealed record CopiaPlanea(
    string Nrc, string CodigoExperiencia, string CodigoPlan, string Titulo,
    string? Campus, string? Nivel, string? Region, string? Area);

public sealed record HorarioCopiaPlanea(
    int IdHorario, string Nrc, string Dia, TimeOnly HoraInicio, TimeOnly HoraFin,
    string? Edificio, string? Aula, DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed record DatosPeriodoPlanea(
    int NrcRecibidos,                       // NRC distintos válidos en "resultado"
    int NrcSinPlan,                         // omitidos por sec_programa nulo
    IReadOnlyList<CopiaPlanea> Copias,      // candidatas (tienen código de plan)
    IReadOnlyList<HorarioCopiaPlanea> Horarios,
    IReadOnlyList<string> Advertencias);

// ResumenAplicacionPlanea.cs
public sealed record ResumenAplicacionPlanea(
    int NrcNuevos, int NrcExistentes, int NrcSinExperiencia, int HorariosInsertados);

// ResultadoSincronizacionPlanea.cs
public sealed record ResultadoSincronizacionPlanea(
    string CodigoPeriodo, string Estado, ResumenAplicacionPlanea? Resumen, string? Mensaje);
```

### 3.5 `SGPla/Parsers/PlaneaNormalizador.cs`
Clase **estática y pura**: sin I/O ni dependencias. Recorre los datos una sola vez usando diccionarios (O(n)). Implementa exactamente estas reglas:

```csharp
using System.Globalization;
using System.Text.RegularExpressions;
using SGPla.Commons;
using SGPla.Models.DTOs.Planea;

namespace SGPla.Parsers;

public static partial class PlaneaNormalizador
{
    public static DatosPeriodoPlanea Normalizar(PlaneaRespuesta respuesta)
    {
        var advertencias = new AcumuladorAdvertencias();
        var nrcsVistos = new HashSet<string>(StringComparer.Ordinal);
        var copias = new Dictionary<string, CopiaPlanea>(StringComparer.Ordinal);
        var nrcSinPlan = 0;

        foreach (var fila in respuesta.Resultado ?? [])
        {
            var nrc = Limpiar(fila.Nrc);
            var materia = Limpiar(fila.Materia)?.ToUpperInvariant();
            var curso = Limpiar(fila.Curso)?.ToUpperInvariant();
            var titulo = NormalizarEspacios(fila.Titulo);

            if (nrc is null || nrc.Length > PlaneaConstantes.LONGITUD_NRC
                || materia is null || curso is null || titulo is null)
            {
                advertencias.Agregar("Filas sin NRC, materia, curso o título válidos", nrc);
                continue;
            }

            // Una fila por docente: el NRC se procesa una sola vez (los datos de sección son idénticos).
            if (!nrcsVistos.Add(nrc)) continue;

            var codigoPlan = Limpiar(fila.CodigoPlan)?.ToUpperInvariant();
            if (codigoPlan is null)
            {
                nrcSinPlan++;               // se omite: sin plan no hay EE del catálogo
                continue;
            }

            var codigoExperiencia = $"{materia} {curso}";
            if (codigoExperiencia.Length > PlaneaConstantes.LONGITUD_CODIGO_EE
                || codigoPlan.Length > PlaneaConstantes.LONGITUD_CODIGO_PLAN)
            {
                advertencias.Agregar("Códigos de EE o de plan con longitud inválida", nrc);
                continue;
            }

            copias[nrc] = new CopiaPlanea(
                nrc, codigoExperiencia, codigoPlan,
                Truncar(titulo, PlaneaConstantes.LONGITUD_TITULO)!,
                Truncar(Limpiar(fila.Campus), 5),
                Truncar(Limpiar(fila.Nivel), 5),
                Truncar(Limpiar(fila.Region), 50),
                Truncar(Limpiar(fila.Area), 100));
        }

        // Clave de sesión sin docente: si dos bloques coinciden en todo, son la misma sesión.
        var horarios = new Dictionary<(string, string, TimeOnly, TimeOnly, string?, string?, DateOnly?, DateOnly?), HorarioCopiaPlanea>();
        foreach (var bloque in respuesta.Horarios ?? [])
        {
            var nrc = Limpiar(bloque.Nrc);
            if (nrc is null || !nrcsVistos.Contains(nrc))
            {
                advertencias.Agregar("Horarios de NRC que no vienen en resultado", nrc);
                continue;
            }
            if (!copias.ContainsKey(nrc)) continue;   // NRC omitido (sin plan o inválido): su horario también

            if (!int.TryParse(bloque.IdHorario, NumberStyles.None, CultureInfo.InvariantCulture, out var idHorario))
            {
                advertencias.Agregar("Horarios con rhs_id inválido", nrc);
                continue;
            }

            var edificio = Truncar(Limpiar(bloque.Edificio), 50);
            var aula = Truncar(Limpiar(bloque.Aula), 100);
            var fechaInicio = ParsearFecha(bloque.FechaInicio);
            var fechaFin = ParsearFecha(bloque.FechaFin);

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

                var clave = (nrc, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin);
                horarios.TryAdd(clave, new HorarioCopiaPlanea(
                    idHorario, nrc, dia, horaInicio, horaFin, edificio, aula, fechaInicio, fechaFin));
                // Si ya existía, es la misma sesión (otro docente): se descarta sin advertencia.
            }

            if (!tieneDias)
                advertencias.Agregar("Bloques de horario sin días", nrc);
        }

        return new DatosPeriodoPlanea(
            nrcsVistos.Count, nrcSinPlan,
            copias.Values.ToList(), horarios.Values.ToList(),
            advertencias.Resumir());
    }

    // ---- helpers públicos (se prueban directamente) ----

    /// Trim; "", "-", "---" => null.
    public static string? Limpiar(string? valor) { ... }

    /// Limpiar + colapsar espacios internos múltiples (Regex \s+ -> " ").
    public static string? NormalizarEspacios(string? valor) { ... }

    /// "1400" -> 14:00. Exactamente 4 dígitos, HH <= 23, mm <= 59.
    public static bool TryParsearHora(string? valor, out TimeOnly hora) { ... }

    /// "yyyy-MM-dd" (InvariantCulture) o null.
    public static DateOnly? ParsearFecha(string? valor) { ... }

    public static string? Truncar(string? valor, int longitud) { ... }

    // ---- privados ----

    private static IEnumerable<(string Dia, string? Inicio, string? Fin)> DiasDe(PlaneaHorario h) =>
    [
        ("Lunes", h.LunIni, h.LunFin),
        ("Martes", h.MarIni, h.MarFin),
        ("Miercoles", h.MieIni, h.MieFin),
        ("Jueves", h.JueIni, h.JueFin),
        ("Viernes", h.VieIni, h.VieFin),
        ("Sabado", h.SabIni, h.SabFin),
    ];

    /// Agrupa advertencias por categoría: "1144 × Bloques de horario sin días (ej. 10795, …)".
    /// Guarda como máximo 5 ejemplos por categoría. Resumir() devuelve una línea por categoría, ordenadas por conteo descendente.
    private sealed class AcumuladorAdvertencias { ... }
}
```
Reglas adicionales:
- **No** guardes `FIN` sumándole un minuto. `"1459"` se guarda literal como `14:59`.
- El normalizador **no** decide si la EE existe en el catálogo: eso se resuelve en SQL (FASE 4).

### 3.6 `IPlaneaCliente` / `PlaneaCliente`
```csharp
public interface IPlaneaCliente
{
    Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken = default);
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

    public async Task<PlaneaRespuesta> ObtenerPeriodoAsync(string codigoPeriodo, CancellationToken cancellationToken = default)
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
- **Por qué es eficiente:** los DTOs solo mapean unas 9 propiedades de `resultado` y 18 de `horarios`. `Utf8JsonReader` se salta los otros ~55 campos por fila sin crear strings. El JSON nunca se carga como `string`, y lo que queda en memoria son unos 40k objetos pequeños.
- **Importante:** la `BaseAddress` termina en `/` y la ruta relativa **no** empieza con `/`. Si no, se pierde el segmento `apiroladoovr/`.

### 3.7 Pruebas de la Fase 3

**Fixture `SGPla.Tests/Fixtures/planea_muestra.json`**

Genéralo con este script, **sin editarlo a mano**. Así conserva la forma real del JSON, con todos sus campos.
```bash
python3 - <<'EOF'
import json, os
d = json.load(open('/Users/kaleb/repos/sgpla/Endpoint.json'))
nrcs = {'10676','10677','10709','10795','59262','10812','10695','25637','38444'}
res = [x for x in d['resultado'] if x['radoc_nrc'] in nrcs]
hor = [x for x in d['horarios'] if x['NRC'] in nrcs or x['NRC'] == '25108']
out = {'periodo': d['periodo'], 'total': len(res), 'resultado': res, 'horarios': hor}
os.makedirs('SGPla.Tests/Fixtures', exist_ok=True)
json.dump(out, open('SGPla.Tests/Fixtures/planea_muestra.json','w'), ensure_ascii=False, indent=4)
print(len(res), len(hor))
EOF
```
Qué cubre cada NRC:

| NRC | Caso |
|---|---|
| `10676` | `CVCB 18003` / `CIVI-20-E-CR`; horario de lunes a viernes, 14:00–14:59, `I-INEB`/`I-02` |
| `10677` | Otro NRC del mismo plan |
| `10709` | 2 filas en `resultado` (2 docentes) → **una sola** copia |
| `10795` | Varios bloques de horario; al menos uno sin días |
| `59262` | `MVZ 80001` (materia de 3 letras) |
| `10812` | `sec_programa` nulo → cuenta en `NrcSinPlan` |
| `10695` y `25637` | Mismo código `DECA 28003`, título truncado y completo |
| `38444` | Un NRC más, para el conteo |
| `25108` | Horario huérfano (solo aparece en `horarios`) |

En `SGPla.Tests/SGPla.Tests.csproj` agrega:
```xml
<ItemGroup>
  <None Update="Fixtures\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```
Lee el fixture con `Path.Combine(AppContext.BaseDirectory, "Fixtures", "planea_muestra.json")`.

**`SGPla.Tests/Parsers/PlaneaNormalizadorTests.cs`.** Carga el fixture con `JsonSerializer.Deserialize<PlaneaRespuesta>` y las mismas opciones del cliente. Prueba:
- `NrcRecibidos` es 9 (los NRC del fixture) y `NrcSinPlan` es 1 (`10812`).
- `10709` produce una sola copia, aunque venga en 2 filas.
- `10676` tiene `CodigoExperiencia = "CVCB 18003"` y `CodigoPlan = "CIVI-20-E-CR"`.
- `59262` tiene `CodigoExperiencia = "MVZ 80001"`: el normalizador lo deja pasar y el SQL lo omitirá.
- `10676` genera exactamente 5 horarios: días `Lunes`…`Viernes`, `14:00`–`14:59`, `Edificio = "I-INEB"`, `Aula = "I-02"`.
- No hay horarios de `10812`, porque es un NRC omitido.
- No hay horarios de `25108`, y aparece la advertencia de horarios huérfanos.
- Dos bloques con la misma sesión producen un solo horario. Pruébalo con un `PlaneaRespuesta` armado en código.
- `[Theory]` para los helpers:
  - `Limpiar`: `""`, `"  "`, `"-"`, `"---"` → null; `" X "` → `"X"`.
  - `TryParsearHora`: `"0700"` → 07:00; `"2460"`, `"123"`, `"ab12"` y null → false.
  - `ParsearFecha`: `"2026-08-17"` → fecha; `"x"` → null.
  - `Truncar`.

**`SGPla.Tests/Services/PlaneaClienteTests.cs`.** Crea un `HttpMessageHandler` falso (subclase privada que sobrescribe `SendAsync`). Úsalo con `new HttpClient(handler) { BaseAddress = new Uri("https://planea.test/planea/index.php/apiroladoovr/") }`. Prueba:
- Con el fixture en un `StreamContent`, deserializa y `Total == Resultado.Count`.
- El handler recibió la URL `…/apiroladoovr/periodo/202701`.
- Una respuesta 500 produce `HttpRequestException`.
- Un contenido `"{no json"` produce `PlaneaRespuestaInvalidaException`.
- Un contenido `"null"` produce `PlaneaRespuestaInvalidaException`.

### 3.8 Checkpoint Fase 3
- `dotnet build` y `dotnet test`: sin fallos nuevos.
- Commits: `feat(planea): agrega cliente y normalizador de PLANEA` y `test(planea): …`.

---

## FASE 4 — Repositorio de sincronización (SqlBulkCopy + inserción set-based)

### 4.1 `SGPla/Repositories/Interfaces/ISincronizacionPlaneaRepository.cs`
```csharp
public interface ISincronizacionPlaneaRepository
{
    Task<IReadOnlyList<PeriodoPorSincronizar>> ObtenerPeriodosVigentesAsync(
        DateOnly hoy, int ventanaAnticipacionMeses, CancellationToken cancellationToken = default);

    Task<int> ContarPeriodosSinFechasAsync(CancellationToken cancellationToken = default);

    Task<int> IniciarBitacoraAsync(int idPeriodo, CancellationToken cancellationToken = default);

    Task CerrarBitacoraAsync(
        int idSincronizacion, string estado, int? registrosRecibidos, DatosPeriodoPlanea? datos,
        ResumenAplicacionPlanea? resumen, string? advertencias, string? mensajeError,
        CancellationToken cancellationToken = default);

    Task<int> MarcarInterrumpidasAsync(TimeSpan umbral, CancellationToken cancellationToken = default);

    /// Inserta SOLO copias nuevas (con EE en el catálogo) y sus horarios. Nunca actualiza ni borra.
    Task<ResumenAplicacionPlanea> RegistrarNuevasAsync(
        int idPeriodo, string codigoPeriodo, int idSincronizacion, DatosPeriodoPlanea datos,
        CancellationToken cancellationToken = default);
}
```

### 4.2 `SincronizacionPlaneaRepository`: métodos simples (EF)
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
- **`CerrarBitacoraAsync`:** `_context.SincronizacionPlanea.Where(s => s.IdSincronizacionPlanea == id).ExecuteUpdateAsync(...)`. Asigna:
  - `FechaFin = DateTime.Now` y el estado.
  - `RegistrosRecibidos`.
  - `NrcRecibidos` y `NrcSinPlan`, tomados de `datos`.
  - `NrcNuevos`, `NrcExistentes`, `NrcSinExperiencia` y `HorariosInsertados`, tomados de `resumen`.
  - Las advertencias y el mensaje de error.
  - Todo lo que no se tenga queda en null.
- **`MarcarInterrumpidasAsync`:** `ExecuteUpdateAsync` sobre los registros con `Estado == EnProceso && FechaInicio < DateTime.Now - umbral`. Asigna `Estado = Interrumpida`, `FechaFin = DateTime.Now` y `MensajeError = "La ejecución no terminó (reinicio o caída de la aplicación)."`.

### 4.3 `RegistrarNuevasAsync`: algoritmo
Los datos masivos **no** pasan por el ChangeTracker de EF. Se usa ADO.NET sobre la misma conexión del DbContext, en **una sola transacción**:

```csharp
public async Task<ResumenAplicacionPlanea> RegistrarNuevasAsync(int idPeriodo, string codigoPeriodo,
    int idSincronizacion, DatosPeriodoPlanea datos, CancellationToken cancellationToken = default)
{
    await _context.Database.OpenConnectionAsync(cancellationToken);
    try
    {
        await using var transaccionEf = await _context.Database.BeginTransactionAsync(cancellationToken);
        var conexion = (SqlConnection)_context.Database.GetDbConnection();
        var transaccion = (SqlTransaction)transaccionEf.GetDbTransaction();

        await AdquirirBloqueoAsync(conexion, transaccion, codigoPeriodo, cancellationToken);          // 1
        await EjecutarAsync(conexion, transaccion, SincronizacionPlaneaSql.CrearTablasTemporales, cancellationToken); // 2
        await CopiarAsync(conexion, transaccion, "#CopiaPlanea", CrearTablaCopias(datos.Copias), cancellationToken);
        await CopiarAsync(conexion, transaccion, "#HorarioPlanea", CrearTablaHorarios(datos.Horarios), cancellationToken);
        await EjecutarAsync(conexion, transaccion, SincronizacionPlaneaSql.ResolverReferencias, cancellationToken); // 3
        var resumen = await LeerResumenAsync(conexion, transaccion,
            SincronizacionPlaneaSql.RegistrarNuevas, idPeriodo, idSincronizacion, cancellationToken);  // 4

        await transaccionEf.CommitAsync(cancellationToken);                                          // 5
        return resumen;
    }
    finally
    {
        await _context.Database.CloseConnectionAsync();
    }
}
```
**Helpers ADO.NET:**
- Cada `SqlCommand` lleva `Connection = conexion`, `Transaction = transaccion` y `CommandTimeout = 300`. Cuando el SQL los use, agrega los parámetros `@idPeriodo` e `@idSincronizacion` (`SqlDbType.Int`).
- **`AdquirirBloqueoAsync`:**
  - Ejecuta `DECLARE @r int; EXEC @r = sp_getapplock @Resource = @recurso, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 0; SELECT @r;` con `@recurso = "SincronizacionPlanea:" + codigoPeriodo`.
  - Si el resultado es `< 0`, lanza `SincronizacionEnCursoException(codigoPeriodo)`.
- **`CopiarAsync`:**
  - `using var bulk = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, transaccion) { DestinationTableName = tabla, BatchSize = 5000, BulkCopyTimeout = 300 };`
  - Agrega un `ColumnMappings.Add(col.ColumnName, col.ColumnName)` por cada columna del `DataTable`.
  - Termina con `await bulk.WriteToServerAsync(dataTable, cancellationToken)`.
- **`CrearTabla…`:**
  - Crea `DataTable`s cuyos nombres de columna coinciden exactamente con los de las tablas temporales, **sin** las columnas `id…` que se resuelven en SQL.
  - Los `null` se escriben como `DBNull.Value`.
  - Tipos: `TimeOnly` → `TimeSpan` (`hora.ToTimeSpan()`) y `DateOnly?` → `DateTime` (`fecha.ToDateTime(TimeOnly.MinValue)`).
- **`LeerResumenAsync`:** ejecuta el lote y lee la **única fila del `SELECT` final** con `SqlDataReader`, mapeándola a `ResumenAplicacionPlanea`.

### 4.4 `SGPla/Repositories/Implementations/SincronizacionPlaneaSql.cs`: SQL exacto
`internal static class` con constantes `const string` en raw string literals (`"""…"""`).

> **Collation:** las tablas temporales y las variables de tabla viven en `tempdb`, que puede tener otra collation. **Todas** sus columnas de texto llevan `COLLATE DATABASE_DEFAULT`; si falta, SQL Server lanza "Cannot resolve the collation conflict".

```sql
-- CrearTablasTemporales
DROP TABLE IF EXISTS #CopiaPlanea;
DROP TABLE IF EXISTS #HorarioPlanea;

CREATE TABLE #CopiaPlanea (
    nrc               varchar(5)   COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    codigoExperiencia varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL,
    codigoPlan        varchar(50)  COLLATE DATABASE_DEFAULT NOT NULL,
    titulo            varchar(150) COLLATE DATABASE_DEFAULT NOT NULL,
    campus            varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    nivel             varchar(5)   COLLATE DATABASE_DEFAULT NULL,
    region            varchar(50)  COLLATE DATABASE_DEFAULT NULL,
    area              varchar(100) COLLATE DATABASE_DEFAULT NULL,
    idExperienciaEducativa int NULL,
    idPlanEstudios         int NULL,
    idRegion               int NULL
);

CREATE TABLE #HorarioPlanea (
    idHorario   int NOT NULL,
    nrc         varchar(5)   COLLATE DATABASE_DEFAULT NOT NULL,
    dia         varchar(10)  COLLATE DATABASE_DEFAULT NOT NULL,
    horaInicio  time(0) NOT NULL,
    horaFin     time(0) NOT NULL,
    edificio    varchar(50)  COLLATE DATABASE_DEFAULT NULL,
    aula        varchar(100) COLLATE DATABASE_DEFAULT NULL,
    fechaInicio date NULL,
    fechaFin    date NULL
);
CREATE INDEX IX_HorarioPlanea_nrc ON #HorarioPlanea (nrc);
```
Las columnas de los `DataTable` son:
- `#CopiaPlanea`: `nrc`, `codigoExperiencia`, `codigoPlan`, `titulo`, `campus`, `nivel`, `region`, `area`.
- `#HorarioPlanea`: `idHorario`, `nrc`, `dia`, `horaInicio`, `horaFin`, `edificio`, `aula`, `fechaInicio`, `fechaFin`.

```sql
-- ResolverReferencias
-- EE del catálogo: mismo código "MATERIA CURSO" dentro del plan cuyo codigoPlan coincide.
-- Solo planes de programas vigentes. El catálogo NO se modifica.
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
```
```sql
-- RegistrarNuevas  (devuelve una fila: nuevos, existentes, sinExperiencia, horarios)
DECLARE @ahora datetime2(0) = SYSDATETIME();
DECLARE @horarios int = 0;
DECLARE @nuevas TABLE (
    idExperienciaEducativaPeriodo int NOT NULL,
    nrc varchar(5) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY
);

-- Precedencia de conteo: 1) ya registrada  2) sin EE en catálogo  3) nueva.
DECLARE @existentes int = (
    SELECT COUNT(*) FROM #CopiaPlanea AS c
    WHERE EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t
                  WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc));

DECLARE @sinExperiencia int = (
    SELECT COUNT(*) FROM #CopiaPlanea AS c
    WHERE c.idExperienciaEducativa IS NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.ExperienciaEducativaPeriodo AS t
                      WHERE t.idPeriodo = @idPeriodo AND t.nrc = c.nrc));

-- Solo copias nuevas cuya EE existe en el catálogo. Las existentes se omiten sin tocarlas.
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

-- Horarios únicamente de las copias recién registradas.
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
```
Notas:
- Todo `RegistrarNuevas` se envía en **un solo `SqlCommand`**, porque `@nuevas` es una variable de tabla del lote.
- El normalizador ya eliminó los horarios duplicados. El índice único `UX_ExperienciaEducativaPeriodoHorario_sesion` es la red de seguridad: si falla, la transacción entera hace rollback y la bitácora queda `Fallida`.
- **Nunca** se ejecuta `UPDATE` ni `DELETE` sobre `ExperienciaEducativaPeriodo` ni sobre `ExperienciaEducativaPeriodoHorario`.

### 4.5 Checkpoint Fase 4
- `dotnet build` compila. El SQL se valida en la Fase 7 contra un SQL Server real.
- Commit: `feat(planea): agrega repositorio de sincronización`.

---

## FASE 5 — Servicios, worker y registro

### 5.1 `SincronizarPeriodoPlaneaService` (`SGPla/Services/Interfaces` + `SGPla/Services/Implementations`)
```csharp
public interface ISincronizarPeriodoPlaneaService
{
    Task<ResultadoSincronizacionPlanea> SincronizarAsync(PeriodoPorSincronizar periodo, CancellationToken cancellationToken = default);
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
   6. `var resumen = await _repositorio.RegistrarNuevasAsync(periodo.IdPeriodo, periodo.Codigo, idBitacora, datos, ct);`
   7. Cierra la bitácora como `Exitosa`, con `recibidos`, `datos`, `resumen` y `string.Join(Environment.NewLine, datos.Advertencias)`.
   8. `LogInformation` con el periodo, la duración y el resumen (nuevas, existentes, sin plan, sin EE, horarios), y devuelve.
3. `catch (SincronizacionEnCursoException ex)` → cierra como `Omitida` con `ex.Message` y usa `LogWarning`.
4. `catch (OperationCanceledException) when (ct.IsCancellationRequested)` → cierra como `Interrumpida` **usando `CancellationToken.None`** y relanza.
5. `catch (Exception ex)` → cierra como `Fallida` con `ex.Message` (usando `CancellationToken.None`), hace `LogError(ex, …)` y devuelve el resultado `Fallida`. **No relanza.**

`ObtenerConReintentosAsync`:
- Hace hasta `_opciones.Intentos` intentos.
- Reintenta solo ante `HttpRequestException` o `TaskCanceledException` cuando `!ct.IsCancellationRequested` (timeout).
- La espera es `EsperaEntreIntentos * 4^(intento-1)`, con `Task.Delay(espera, ct)`.
- En el último intento relanza la excepción.

### 5.2 `SincronizarPeriodosVigentesService` (`SGPla/Services/Interfaces` + `SGPla/Services/Implementations`)
```csharp
public interface ISincronizarPeriodosVigentesService
{
    Task<IReadOnlyList<ResultadoSincronizacionPlanea>> EjecutarAsync(CancellationToken cancellationToken = default);
}
```
1. `await _repositorio.MarcarInterrumpidasAsync(_opciones.UmbralInterrumpida, ct);`
2. `var hoy = DateOnly.FromDateTime(DateTime.Today);`
3. Obtiene los periodos vigentes con `VentanaAnticipacionMeses`. Si `ContarPeriodosSinFechasAsync > 0`, hace `LogWarning`: esos periodos no se sincronizan hasta que tengan fechas.
4. Si no hay periodos: `LogInformation` y devuelve una lista vacía.
5. Llama a `_sincronizarPeriodo.SincronizarAsync` para cada periodo **en secuencia** (sin paralelismo) y acumula los resultados. Como ese servicio no relanza errores, un fallo no detiene a los demás.

### 5.3 `SGPla/Services/Implementations/SincronizacionPlaneaWorker.cs`
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

### 5.4 Registro en `SGPla/Program.cs`
No crees un `XxxModuleExtensions`. Agrega esto **en el bloque `//Clases`**, después de `builder.Services.AddScoped<IPlantillaService, PlantillaService>();`:
```csharp
// Sincronización PLANEA (MVC)
builder.Services.AddOptions<PlaneaOpciones>()
    .Bind(builder.Configuration.GetSection(PlaneaOpciones.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<IPlaneaCliente, PlaneaCliente>((proveedor, cliente) =>
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
builder.Services.AddScoped<ISincronizacionPlaneaRepository, SincronizacionPlaneaRepository>();
builder.Services.AddScoped<ISincronizarPeriodoPlaneaService, SincronizarPeriodoPlaneaService>();
builder.Services.AddScoped<ISincronizarPeriodosVigentesService, SincronizarPeriodosVigentesService>();
builder.Services.AddHostedService<SincronizacionPlaneaWorker>();
```
Agrega los `using` que falten: `System.Net`, `Microsoft.Extensions.Options` y `SGPla.Commons`. Los registros de la consulta del front se agregan en la FASE 6.

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
    "UmbralInterrumpida": "01:00:00"
  }
  ```
- **`SGPla/appsettings.Development.json`:** agrega `"Planea": { "Habilitada": false }`, para no llamar a PLANEA desde cada máquina de desarrollo.
- **`docker-compose.yml`**, servicio `backend`: agrega `Planea__Habilitada: ${PLANEA_HABILITADA:-false}`.

### 5.6 Pruebas de la Fase 5 (Moq)
Ubica las pruebas en `SGPla.Tests/Services/`. Sigue el estilo de `SGPla.Tests/Services/PeriodoEscolarServiceTest.cs` (mocks de repositorio con Moq). Para capturar argumentos, usa `Callback`. En las pruebas, usa `Options.Create(new PlaneaOpciones { EsperaEntreIntentos = TimeSpan.Zero, Intentos = 3 })`.

**`SincronizarPeriodoPlaneaServiceTests`:**
- `SincronizarAsync_RegistraYCierraExitosa`: respuesta válida → `RegistrarNuevasAsync` se llama una vez con el `idBitacora` de `IniciarBitacoraAsync` → `CerrarBitacoraAsync(…, "Exitosa", …)` con el resumen.
- `SincronizarAsync_TotalNoCoincide_NoRegistraYFalla`: `RegistrarNuevasAsync` → `Times.Never`; bitácora `Fallida`.
- `SincronizarAsync_PeriodoDistinto_NoRegistraYFalla`.
- `SincronizarAsync_TotalCero_SinDatos`.
- `SincronizarAsync_EnCurso_Omitida`: `RegistrarNuevasAsync` lanza `SincronizacionEnCursoException`.
- `SincronizarAsync_ReintentaYLuegoExito`: el cliente falla 2 veces con `HttpRequestException` y luego responde → `Exitosa`, cliente llamado 3 veces.
- `SincronizarAsync_AgotaReintentos_Fallida`.

**`SincronizarPeriodosVigentesServiceTests`:**
- Procesa 2 periodos en orden y el primero `Fallida` no impide el segundo.
- Sin periodos, no llama al servicio de periodo.
- Siempre llama a `MarcarInterrumpidasAsync`.

### 5.7 Checkpoint Fase 5
- `dotnet build` y `dotnet test` sin fallos nuevos.
- Arranca la app con `dotnet run --project SGPla`, que en Development deja `Habilitada=false`. El log debe mostrar "La sincronización con PLANEA está deshabilitada." y la app debe funcionar normal.
- Commits: `feat(planea): agrega servicios y worker de sincronización programada` y `test(planea): …`.

---

## FASE 6 — Front MVC de consulta (solo lectura)

Estas pantallas permiten consultar lo sincronizado **sin volver a llamar a PLANEA**. Son de solo lectura: no hay botón para sincronizar, porque el disparo es solo automático.

- **Acceso:** política `SuperUsuario`. Se usa `_LayoutSuperUsuario`, que `_ViewStart` elige automáticamente según el rol.
- **Referencias de estilo:**
  - `Controllers/PeriodosEscolaresController.cs` y `Views/PeriodosEscolares/Index.cshtml`: tabla con `TableModel`, `TablaFactory.GenerarTablaConMensaje`, paginación del lado del servidor y los ViewComponents `Buscador` y `SelectField` con `data-autosubmit`.
  - `Controllers/ProgramacionesAcademicasController.cs`: filtros por periodo.

### 6.1 Consultas: `IConsultaPlaneaRepository` / `ConsultaPlaneaRepository`
Usa EF con `AsNoTracking`, **paginado en SQL** (`Skip`/`Take`) y proyecciones a DTOs. Nunca materialices todas las copias del periodo.

DTOs en `SGPla/Models/DTOs/Planea/ConsultaPlaneaDtos.cs`:
```csharp
public sealed class FiltroBitacoraPlaneaDTO
{
    public int? IdPeriodo { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int Cantidad { get; set; } = 10;
}

public sealed record BitacoraPlaneaDTO(int IdSincronizacion, string CodigoPeriodo, DateTime FechaInicio, DateTime? FechaFin,
    string Estado, int? RegistrosRecibidos, int? NrcRecibidos, int? NrcNuevos, int? NrcExistentes,
    int? NrcSinPlan, int? NrcSinExperiencia, int? HorariosInsertados, string? Advertencias, string? MensajeError);

public sealed class FiltroCopiaPlaneaDTO
{
    public int? IdPeriodo { get; set; }
    public string? Busqueda { get; set; }        // NRC, código o nombre de la EE
    public int? IdPlanEstudios { get; set; }
    public int? IdRegion { get; set; }
    public int Pagina { get; set; } = 1;
    public int Cantidad { get; set; } = 20;
}

public sealed record CopiaPlaneaFilaDTO(int IdExperienciaEducativaPeriodo, string CodigoPeriodo, string Nrc,
    string CodigoExperiencia, string NombreExperiencia, string? CodigoPlan, string? Region, string? Campus,
    int Horarios, DateTime FechaAlta);

public sealed record HorarioCopiaDTO(string Dia, TimeOnly HoraInicio, TimeOnly HoraFin, string? Edificio, string? Aula,
    DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed record DetalleCopiaPlaneaDTO(CopiaPlaneaFilaDTO Copia, string TituloPlanea, string NombrePlan,
    string? Nivel, string? Area, int IdSincronizacionPlanea, IReadOnlyList<HorarioCopiaDTO> Horarios);
```
Métodos del repositorio. Todos reciben `CancellationToken`, y los de listado devuelven también el total para paginar:
- `(List<BitacoraPlaneaDTO> Items, int Total) ObtenerBitacoraAsync(FiltroBitacoraPlaneaDTO)`: ordena por `fechaInicio` descendente.
- `(List<CopiaPlaneaFilaDTO> Items, int Total) ObtenerCopiasAsync(FiltroCopiaPlaneaDTO)`.
  - `NombreExperiencia` y `CodigoExperiencia` vienen de la EE **del catálogo**.
  - `CodigoPlan` es `PlanEstudios.CodigoPlan`.
  - `Busqueda` filtra con `Contains` sobre `nrc`, `ExperienciaEducativa.Codigo` y `ExperienciaEducativa.Nombre`.
  - Ordena por `nrc`.
  - `Horarios` es un `COUNT` en la proyección.
- `DetalleCopiaPlaneaDTO? ObtenerDetalleCopiaAsync(int idExperienciaEducativaPeriodo)`: los horarios se ordenan por día (Lunes → Sabado, según `PlaneaConstantes.DIAS`, en memoria sobre la lista pequeña) y luego por `horaInicio`.
- Catálogos para los `SelectField`, cada uno como `List<OptionModel>`:
  - `ObtenerPeriodosConSincronizacionAsync()`: periodos con bitácora o con copias.
  - `ObtenerPlanesConCopiasAsync(int? idPeriodo)`: planes que tienen copias, con texto `codigoPlan - nombre`.
  - `ObtenerRegionesAsync()`.

### 6.2 Servicio: `IConsultaPlaneaService` / `ConsultaPlaneaService`
- Es un pase fino al repositorio.
- Valida los filtros: `Pagina >= 1`, y `Cantidad` entre 1 y 100 (fuera de rango usa el default). Aplica `Trim` a `Busqueda` y la limita a 100 caracteres. `Estado` debe estar en `PlaneaConstantes.ESTADOS`; si no, se ignora.
- Si la copia no existe, `ObtenerDetalleCopiaAsync` devuelve `null`.

Registra en `Program.cs`, en el bloque `//Clases`:
```csharp
builder.Services.AddScoped<IConsultaPlaneaRepository, ConsultaPlaneaRepository>();
builder.Services.AddScoped<IConsultaPlaneaService, ConsultaPlaneaService>();
```

### 6.3 Controlador `SGPla/Controllers/SincronizacionPlaneaController.cs`
```csharp
[Authorize(Policy = PoliticasAutorizacion.SuperUsuario)]
public class SincronizacionPlaneaController : Controller
{
    // GET /SincronizacionPlanea?idPeriodo=&estado=&pagina=1&cantidad=10
    public async Task<IActionResult> Index(int? idPeriodo, string? estado, int pagina = 1, int cantidad = 10, CancellationToken cancellationToken = default);

    // GET /SincronizacionPlanea/Copias?idPeriodo=&busqueda=&idPlanEstudios=&idRegion=&pagina=1&cantidad=20
    public async Task<IActionResult> Copias(int? idPeriodo, string? busqueda, int? idPlanEstudios, int? idRegion,
        int pagina = 1, int cantidad = 20, CancellationToken cancellationToken = default);

    // GET /SincronizacionPlanea/DetalleCopia/{id}
    public async Task<IActionResult> DetalleCopia(int id, CancellationToken cancellationToken = default); // NotFound() si no existe
}
```
- Solo acciones **GET**. No lleva `[ApiController]` ni rutas `/api`.
- Arma `TableModel` en el controlador, como `PeriodosEscolaresController.LlenarTabla`:
  - Usa `TablaFactory.GenerarTablaConMensaje` cuando no hay filas.
  - Llena `Pagination` (`CurrentPage`, `PageSize`, `TotalItems`).
- **Tabla de bitácora (Index).** Columnas:
  - Periodo, Inicio, Fin, Estado.
  - NRC recibidos, Nuevos, Ya registrados, Sin plan, Sin EE en catálogo, Horarios.
  - Acciones: "Ver detalle", que abre un `Modal` con `Advertencias` y `MensajeError`. Si el texto es largo, puede ir en un `<pre>` dentro del modal.
  - Formato de fechas: `dd/MM/yyyy HH:mm`.
- **Tabla de copias (Copias).** Columnas:
  - Periodo, NRC, Código EE, Experiencia Educativa, Plan, Región, Campus, Horarios, Alta.
  - Acciones: "Ver" → `DetalleCopia/{id}`.
- **Detalle (DetalleCopia):**
  - Encabezado con NRC, periodo, EE del catálogo (código y nombre), plan (código y nombre), título según PLANEA, región, campus, nivel, área y la ejecución que la registró.
  - Tabla de horarios con Día, Inicio, Fin (`HH:mm`), Edificio, Aula, Fecha inicio y Fecha fin (`dd/MM/yyyy`).
  - Botón "Volver" al listado, conservando los filtros por query string.
- Captura `ValidacionExcepction` (de `SGPla.Commons`) y muestra el mensaje con el componente `Toast`, igual que las pantallas existentes. Cualquier otra excepción se propaga.

### 6.4 ViewModels y vistas
- **ViewModels** en `SGPla/Models/ViewModels/SincronizacionPlanea/`:
  - `IndexViewModel`: `TableModel Table`, `List<OptionModel> Periodos`, `List<OptionModel> Estados`, los filtros actuales y la paginación.
  - `CopiasViewModel`: `TableModel Table`, los catálogos `Periodos`, `Planes` y `Regiones`, `Busqueda`, los filtros actuales y la paginación.
  - `DetalleCopiaViewModel`: `DetalleCopiaPlaneaDTO Detalle`, `TableModel TablaHorarios` y `string UrlVolver`.
- **Vistas** en `SGPla/Views/SincronizacionPlanea/`: `Index.cshtml`, `Copias.cshtml` y `DetalleCopia.cshtml`.
  - Formularios GET con `SelectField` (periodo, estado, plan, región) y `Buscador` (NRC/EE), con `data-autosubmit`.
  - `@await Component.InvokeAsync("Table", Model.Table)`.
  - Hidden `pagina`/`cantidad`, como en `PeriodosEscolares/Index.cshtml`.
  - Enlaces o pestañas entre "Bitácora" y "Copias registradas". Puedes usar el ViewComponent `Tabs` o dos enlaces simples.
  - `ViewData["Title"]`: "Sincronización PLANEA", "Copias registradas" y "NRC {nrc}".
- **Menú:** en `SGPla/Views/Shared/_LayoutSuperUsuario.cshtml`, agrega después de "Periodos Escolares":
  ```html
  <!-- Sincronización PLANEA -->
  <a href="/SincronizacionPlanea" class="menu-item @(IsActiveController("SincronizacionPlanea"))">
      <span>Sincronización PLANEA</span>
  </a>
  ```

### 6.5 Pruebas de la Fase 6
- **`SGPla.Tests/Services/ConsultaPlaneaServiceTests.cs`** (Moq sobre `IConsultaPlaneaRepository`):
  - Normaliza la paginación fuera de rango.
  - Recorta la búsqueda.
  - Ignora un estado inválido.
  - Devuelve `null` si no existe el detalle.
- **`SGPla.Tests/Security/AutorizacionControllersTests.cs`:** agrega `[InlineData(typeof(SincronizacionPlaneaController), PoliticasAutorizacion.SuperUsuario)]`.

### 6.6 Checkpoint Fase 6
- `dotnet build` y `dotnet test` sin fallos nuevos.
- Con la app corriendo y datos sincronizados (FASE 7), entra como SuperUsuario y revisa:
  - Aparece el menú "Sincronización PLANEA".
  - La bitácora muestra las corridas con sus contadores.
  - El listado de copias filtra por periodo, plan y búsqueda, y pagina.
  - El detalle del NRC `10676` muestra FISICA (`CVCB 18003`) y 5 horarios, de Lunes a Viernes, 14:00–14:59, en `I-INEB`/`I-02`.
- Un usuario con otro rol recibe acceso denegado (redirección a `/Login`).
- Commit: `feat(planea): agrega pantallas MVC de consulta de la sincronización`.

---

## FASE 7 — Verificación end-to-end (requiere Docker)

1. **Levanta la BD y aplica las migraciones:**
   ```bash
   docker compose up -d db
   docker compose run --rm db-init
   ```
2. **Prepara el catálogo de prueba.** Hazlo con un script SQL en `/tmp`, **nunca en el repo** ni en el seed.
   - El seed de desarrollo crea periodos solo con `codigo`, sin fechas, y no trae planes ni EEs.
   - Simula lo que haría el caso de uso "Cargar plan de estudios":
   ```sql
   -- /tmp/planea_catalogo_prueba.sql
   UPDATE dbo.Periodo SET fechaInicio = '2026-08-01', fechaFin = '2027-01-31' WHERE codigo = '202701';

   DECLARE @idPlan int;
   INSERT INTO dbo.PlanEstudios (idProgramaEducativo, nombre, codigoPlan, modalidad, idArchivoPlan)
   VALUES (3, 'Plan 2020 Ingeniería Civil', 'CIVI-20-E-CR', NULL, NULL);   -- programa 3 = Ingeniería Civil (seed)
   SET @idPlan = SCOPE_IDENTITY();

   INSERT INTO dbo.ExperienciaEducativa (idPlanEstudios, codigo, nombre, perfilDocente, creditos, horas)
   VALUES (@idPlan, 'CVCB 18003', 'FISICA', 'Perfil de prueba', '8', '5'),
          (@idPlan, 'CVCB 18004', 'GEOMETRIA ANALITICA', 'Perfil de prueba', '8', '5');
   ```
   - Ejecútalo con `docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'SgplaDev_2026!Password' -d GestionDePlazasBD -i …`. Si el archivo no está dentro del contenedor, cópialo con `docker compose cp`.
   - Ajusta las columnas si el esquema real difiere y anótalo.
3. **Mock de PLANEA** en `/tmp`, **nunca dentro del repo**:
   ```bash
   MOCK=/tmp/planea-mock/planea/index.php/apiroladoovr/periodo
   mkdir -p $MOCK && cp /Users/kaleb/repos/sgpla/Endpoint.json $MOCK/202701
   (cd /tmp/planea-mock && python3 -m http.server 8099)   # en background
   ```
   Si otro periodo resulta vigente, el mock devolverá 404 y ese periodo quedará `Fallida`. Eso es correcto.
4. **Ejecuta la app apuntando al mock:**
   ```bash
   ConnectionStrings__DefaultConnection="Server=localhost,14333;Database=GestionDePlazasBD;User Id=sa;Password=SgplaDev_2026!Password;TrustServerCertificate=True;Encrypt=False;" \
   Planea__Habilitada=true \
   Planea__UrlBase=http://localhost:8099/planea/index.php/apiroladoovr/ \
   Planea__RetrasoInicial=00:00:05 \
   Planea__Intervalo=00:02:00 \
   Planea__Intentos=1 \
   dotnet run --project SGPla
   ```
5. **Primera corrida.** Verifica con `sqlcmd`:
   - `SELECT TOP 5 * FROM SincronizacionPlanea ORDER BY idSincronizacionPlanea DESC`. La fila de `202701` debe tener:
     - `estado = 'Exitosa'`.
     - `registrosRecibidos = 18203` y `nrcRecibidos = 18022`.
     - `nrcSinPlan = 900`, `nrcNuevos = 33`, `nrcExistentes = 0` y `nrcSinExperiencia = 17089` (18022 − 900 − 33).
     - `horariosInsertados = 109`.
   - `SELECT COUNT(*) FROM ExperienciaEducativaPeriodo` da **33**, y todas las filas apuntan al plan `CIVI-20-E-CR`.
   - Sin duplicados: `SELECT idPeriodo, nrc, COUNT(*) FROM ExperienciaEducativaPeriodo GROUP BY idPeriodo, nrc HAVING COUNT(*) > 1` no devuelve filas.
   - El NRC `10676` tiene 5 horarios, de Lunes a Viernes, 14:00–14:59, en `I-INEB`/`I-02`.
   - El catálogo no cambió: `SELECT COUNT(*) FROM ExperienciaEducativa` sigue en 2 y `PlanEstudios` en 1.
6. **Segunda corrida (idempotencia).** Espera el siguiente ciclo (2 min). Debe terminar con `nrcNuevos = 0`, `nrcExistentes = 33`, `nrcSinExperiencia = 17089` y `horariosInsertados = 0`. Los `COUNT(*)` no cambian.
7. **Enlace posterior** (simula que se cargó más del plan):
   - Inserta `('CVCB 18005', 'QUIMICA')` en el mismo plan y espera un ciclo.
   - Debe dar `nrcNuevos = 16`, `nrcExistentes = 33` y `horariosInsertados = 68`.
   - Las 33 copias anteriores no se modificaron: su `fechaAlta` e `idSincronizacionPlanea` siguen iguales.
8. **Protecciones:** altera `total` en el JSON del mock. La corrida debe quedar `Fallida` y no debe haber cambios en las tablas.
9. **Front (FASE 6):** recorre las 3 pantallas de consulta con estos datos (ver el checkpoint 6.6).
10. **Rendimiento:** anota en el resumen final la duración de descarga, normalización y escritura, y la memoria del proceso (`dotnet-counters` si está instalado; si no, el Monitor de actividad). Objetivo orientativo: menos de 30 s en total en local.
11. **Regresión:** navega en la app los flujos de avisos, programación académica y planes de estudio con los datos del seed, y confirma que no hay errores.
12. Detén el mock y la app.

Si no hay Docker o SQL Server disponible, **no marques la Fase 7 como hecha**. Deja la lista de pasos pendientes en tu resumen final.

---

## 11. Casos borde (referencia rápida)

| Caso | Comportamiento esperado |
|---|---|
| NRC con varias filas en `resultado` (varios docentes) | Una sola copia |
| `sec_programa` nulo | Omitido; cuenta en `nrcSinPlan` |
| Plan no cargado en el sistema, o EE no registrada en ese plan | Omitido; cuenta en `nrcSinExperiencia`. No se crea nada en el catálogo |
| Materia de 3 letras (`MVZ 80001`) | No puede existir en el catálogo → omitido como sin EE |
| Copia ya registrada (periodo + NRC) | Omitida; no se actualiza ni se le agregan horarios. Cuenta en `nrcExistentes` |
| Plan cargado después | La siguiente corrida registra las copias de ese plan |
| Bloques de horario con la misma sesión (otro docente) | Un solo horario |
| Bloque sin días / horario de NRC ausente en `resultado` | Se descarta con advertencia |
| Respuesta parcial, periodo distinto, JSON inválido, HTTP 5xx | `Fallida` sin tocar datos |
| Dos ejecuciones simultáneas del mismo periodo | La segunda queda `Omitida` (`sp_getapplock`) |
| Caída a mitad de una ejecución | Rollback automático; la bitácora pasa a `Interrumpida` en el siguiente ciclo |
| Periodo sin `fechaInicio`/`fechaFin` | No se sincroniza; se registra un warning en el log |

## 12. Qué NO hacer
- **No** toques `sgpla-app/` (React) ni la API REST: nada en `SGPla/Modules/`, ni controladores `[ApiController]`, ni rutas `/api/...`.
- **No** trabajes sobre `origin/develop` ni traigas cambios de ahí: la base es `37625fe` (migración `0011`).
- **No** crees, actualices ni borres registros de `ExperienciaEducativa`, `PlanEstudios` ni `ProgramaEducativo`. Su única fuente de verdad es la carga de planes de estudio.
- **No** cambies el esquema ni las propiedades existentes de `ExperienciaEducativa`, `PlanEstudios`, `Oferta`, `Horario` ni `CargaAcademica`.
- **No** guardes docentes en esta fase.
- **No** ejecutes `UPDATE` ni `DELETE` sobre copias u horarios ya registrados.
- **No** agregues un botón ni una acción POST para sincronizar manualmente: el disparo es solo automático y el front es de solo lectura.
- **No** corrijas las fechas de `Periodo` (§14).
- **No** uses `SaveChanges` ni `AddRange` de EF para los datos masivos.
- **No** cargues la respuesta HTTP como `string` (`ReadAsStringAsync`) ni uses `JsonDocument` sobre el payload completo.
- **No** subas `Endpoint.json`, el mock ni el script del catálogo de prueba al repo.

## 13. Resumen final que debe entregar el implementador
- Lista de commits.
- Resultado de `dotnet build` y `dotnet test`: fallos nuevos contra la línea base de §0.1, o confirmación de que no hay ninguno.
- Resultado de cada paso de la FASE 7 y del checkpoint 6.6, o cuáles quedaron pendientes y por qué.
- Cualquier desviación de este plan y su motivo.

## 14. Hallazgos fuera de alcance (solo documentar)
- **Fechas de `Periodo`:**
  - La migración `0002` asume «sufijo `01` = feb–jul, `51` = ago–ene», pero PLANEA muestra que `202701` va del 17 de agosto al 2 de diciembre de 2026, así que la regla parece estar invertida.
  - Además, los periodos que se insertan después de la `0002` (por ejemplo, los del seed) quedan con fechas nulas.
  - La selección de periodos vigentes depende de esas fechas; conviene corregirlas en otra tarea.
- **Mismo plan en varias regiones:** `codigoPlan` es único en el sistema, pero PLANEA usa el mismo código de plan en varias regiones (por ejemplo, `CIVI-20-E-CR` en las 5). Por la regla acordada (materia + curso + código de plan), todas esas copias se enlazan al mismo plan. Si en el futuro se necesita distinguir por región o campus, habrá que revisar el modelo.
- **Borrar EEs de un plan:** si la edición de un plan intenta eliminar una EE que ya tiene copias registradas, la FK lo impide. Hoy pasa lo mismo con `Oferta`. Queda pendiente mostrar un mensaje amigable.
- **Siguientes iteraciones:** docentes por copia (vienen en `resultado`) y una acción manual para forzar la sincronización.
