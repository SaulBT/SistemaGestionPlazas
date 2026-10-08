# PLAN_APROBACION_PLANEA — Aprobación DGAA de la programación importada de PLANEA

> **Para el agente implementador.** Este documento es autocontenido. Síguelo fase por fase y ejecuta el **checkpoint** de cada una antes de avanzar.
> Las decisiones de la §1 ya están cerradas con el usuario: **no las cambies**.
> Si el código real no coincide con algo de este plan (un nombre, una línea, una firma), **investígalo**, adáptate a lo que existe y anótalo como desviación en tu resumen final. **No inventes APIs.**
>
> **Base de código:** rama `develop`, commit `61a4ac0`. La última migración es `0016_enlace_planea_pendiente.sql`.
> `SGPla.DbMigrator/database/seed/development.sql` tiene cambios sin commitear del usuario: **no los descartes**.
>
> **Alcance técnico: solo el MVC** (ASP.NET Core MVC + Razor). No toques `SGPla/Modules/` ni `sgpla-app/`.

---

## 0. Reglas de trabajo

- **Stack:** .NET 10, ASP.NET Core MVC, EF Core Database First sobre SQL Server (`GestionDePlazasDbContext`), migraciones SQL aplicadas con DbUp (`SGPla.DbMigrator`). Pruebas: xUnit + Moq.
- **No agregues paquetes NuGet.**
- **Convenciones:** imita `ProgramacionesAcademicas` y `ProgramacionPlanea*` (capa MVC).
  - Nombres en español; `CancellationToken cancellationToken = default` en los métodos async nuevos de interfaces.
  - Servicios/repositorios con namespace de bloque (`namespace X { … }`); modelos EF con `namespace SGPla.Models;`.
  - Registro DI en `SGPla/Program.cs`, bloque `//Clases`, si hiciera falta (en principio no hay clases nuevas registrables).
- **Comandos:**
  - `dotnet build SGPla/SGPla.csproj`
  - `dotnet build SGPla.DbMigrator/SGPla.DbMigrator.csproj`
  - `dotnet test SGPla.Tests/SGPla.Tests.csproj`
- **Línea base:** antes de tocar nada, compila y corre las pruebas; anota las que ya fallan. "Tests en verde" = ningún fallo nuevo.
- **No hagas commit, push ni PR** salvo que el usuario lo pida. Si los pide: mensajes en español estilo convencional (`feat(planea): …`).
- Si no hay Docker/SQL Server, deja la verificación de BD como **pendiente**; no simules resultados.

---

## 1. Contexto y decisiones (NO modificar)

### 1.1 Cómo funciona hoy
- `ProgramacionesAcademicasController.Index` (policy `OperadorAcademico` = DGAA + EA) arma el resumen en `ProgramacionAcademicaRepository.ObtenerResumenPorProgramaPeriodoAsync` mezclando **dos fuentes**:
  1. **`Oferta`** (EE convocadas/vacantes con su `Horario`). Hoy **ninguna ruta de la app crea `Oferta`**: solo existen las del seed (`development.sql`, bloque «Ofertas del plan de Ingeniería de Software»).
  2. **Copias PLANEA** (`ExperienciaEducativaPeriodo`, NRC que `SincronizacionPlaneaWorker` sincroniza a diario) ya enlazadas con el catálogo → `TieneProgramacionPlanea = true` (líneas ~82-127 del repositorio).
- DGAA solo tiene la acción «planea» → `ProgramacionPlanea` (solo lectura, paginada en servidor). `MatrizPermisos` le da a DGAA únicamente `Ver` y `VerSolicitudes`.
- La EA ve **las mismas filas PLANEA** y, si hay `Oferta` (`TotalEE > 0`), las acciones «Ver» y «Solicitudes».
- Problema: lo de PLANEA nunca se vuelve oferta gestionable y la EA lo ve sin que DGAA lo filtre.

### 1.2 Objetivo
DGAA revisa los NRC de PLANEA de un **plan de estudios × periodo** en una tabla con checkboxes, desmarca los no deseados y confirma. Al confirmar, los NRC aprobados se materializan como `Oferta` + `Horario` y **solo entonces** la EA ve esa programación (fila en el Index con «Ver», asignar docentes, etc.).

### 1.3 Decisiones
| Tema | Decisión |
|---|---|
| Unidad de aprobación | **Plan de estudios × periodo**, desde la pantalla `ProgramacionPlanea` existente |
| Qué ve la EA antes de confirmar | **Nada**: ni fila PLANEA ni enlace «planea». `ProgramacionPlanea` pasa a ser solo DGAA |
| Docentes que trae PLANEA | Se copian a la oferta. Si el `NumeroPersonal` existe en `Docente` → `IdDocente`; si no → `NumeroPersonalImportado` / `NombreDocenteImportado`. Solo cuentan los docentes con `Imparte != false`; si hay varios, se toma el primero por `IdExperienciaEducativaPeriodoDocente`. Sin docente → vacante |
| NRC nuevos que llegan tras confirmar | Quedan **Pendientes**; DGAA puede volver a confirmar solo esos. Lo ya aprobado/descartado no cambia |
| Artículo | DGAA lo elige en un select en la pantalla de aprobación; aplica a todo el lote |
| H/S/M (`Oferta.Hsm`) | Suma de las horas semanales de los horarios PLANEA de la copia (redondeo al entero más cercano; `14:00–14:59` cuenta como 1 h). Si no hay horarios o da 0 → `int.TryParse(ExperienciaEducativa.Horas)`; si tampoco → 0 |
| `TipoContratacion` | `null`; la EA lo edita después en «Editar EE» |
| `Incluida` | `false` si tiene docente; `true` si es vacante (mismo criterio que `AsignarDocenteAsync` / `CambiarAVacanteAsync`) |
| `EstadoSolicitudApertura` | El default de BD (`'Aceptada'`) |
| Sincronización PLANEA | **No se modifica.** Las copias nuevas toman el default `Pendiente` |

### 1.4 Estados de una copia (`ExperienciaEducativaPeriodo.estadoAprobacion`)
```
Pendiente ──(DGAA marca y confirma)──▶ Aprobada   (con idOferta)
Pendiente ──(DGAA desmarca y confirma)─▶ Descartada
Descartada ──(DGAA «Restaurar»)───────▶ Pendiente
```
- Solo las copias **enlazadas** (`IdExperienciaEducativa` y `IdPlanEstudios` no nulos) se pueden aprobar; las no enlazadas no aparecen (igual que hoy).
- Si la `Oferta` de una copia aprobada se elimina, `idOferta` queda `NULL` por la FK (`ON DELETE SET NULL`) y la copia sigue `Aprobada` (no se reofrece automáticamente).

---

## FASE 1 — Migración

Crear `SGPla.DbMigrator/database/migrations/0017_aprobacion_programacion_planea.sql` (UTF-8, idempotente, mismo estilo que `0016`; el `.csproj` ya incluye `database\migrations\**\*.sql`):

```sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/*
    Aprobación DGAA de la programación PLANEA.
    Cada copia (NRC) queda Pendiente hasta que DGAA la aprueba (se crea su Oferta)
    o la descarta. Solo la programación aprobada es visible para la entidad académica.
*/

IF COL_LENGTH(N'dbo.ExperienciaEducativaPeriodo', N'estadoAprobacion') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD [estadoAprobacion] [varchar](10) NOT NULL
            CONSTRAINT [DF_ExperienciaEducativaPeriodo_estadoAprobacion] DEFAULT ('Pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ExperienciaEducativaPeriodo_estadoAprobacion')
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD CONSTRAINT [CK_ExperienciaEducativaPeriodo_estadoAprobacion]
            CHECK ([estadoAprobacion] IN ('Pendiente', 'Aprobada', 'Descartada'));
GO

IF COL_LENGTH(N'dbo.ExperienciaEducativaPeriodo', N'idOferta') IS NULL
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD [idOferta]      [int] NULL,
            [fechaRevision] [datetime2](0) NULL,
            [revisadoPor]   [nvarchar](150) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ExperienciaEducativaPeriodo_Oferta')
    ALTER TABLE [dbo].[ExperienciaEducativaPeriodo]
        ADD CONSTRAINT [FK_ExperienciaEducativaPeriodo_Oferta]
            FOREIGN KEY ([idOferta]) REFERENCES [dbo].[Oferta] ([idOferta]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ExperienciaEducativaPeriodo_idOferta' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ExperienciaEducativaPeriodo_idOferta]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idOferta] ASC)
        WHERE [idOferta] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ExperienciaEducativaPeriodo_plan_periodo_estado' AND object_id = OBJECT_ID(N'dbo.ExperienciaEducativaPeriodo'))
    CREATE NONCLUSTERED INDEX [IX_ExperienciaEducativaPeriodo_plan_periodo_estado]
        ON [dbo].[ExperienciaEducativaPeriodo] ([idPlanEstudios] ASC, [idPeriodo] ASC, [estadoAprobacion] ASC);
GO
```

Las copias existentes quedan `Pendiente` (no hay aprobación retroactiva).

**Checkpoint 1:** `dotnet build SGPla.DbMigrator/SGPla.DbMigrator.csproj`. Si hay Docker: `docker compose up -d db` y `docker compose run --rm db-init` **dos veces** (idempotencia).

---

## FASE 2 — Modelo EF

1. `SGPla/Commons/PlaneaConstantes.cs`: agregar
   ```csharp
   public const string APROBACION_PENDIENTE = "Pendiente";
   public const string APROBACION_APROBADA = "Aprobada";
   public const string APROBACION_DESCARTADA = "Descartada";
   ```
2. `SGPla/Models/ExperienciaEducativaPeriodo.cs`: agregar
   ```csharp
   public string EstadoAprobacion { get; set; } = null!;
   public int? IdOferta { get; set; }
   public DateTime? FechaRevision { get; set; }
   public string? RevisadoPor { get; set; }
   public virtual Oferta? IdOfertaNavigation { get; set; }
   ```
3. `SGPla/Models/Oferta.cs`: `public virtual ICollection<ExperienciaEducativaPeriodo> ExperienciaEducativaPeriodo { get; set; } = new List<ExperienciaEducativaPeriodo>();`
4. `SGPla/Data/GestionDePlazasDbContext.cs`, bloque de `ExperienciaEducativaPeriodo` (~línea 1039):
   - `EstadoAprobacion`: `HasMaxLength(10).IsUnicode(false).HasDefaultValue("Pendiente").HasColumnName("estadoAprobacion")`.
   - `IdOferta` → `"idOferta"`; `FechaRevision` → `HasColumnType("datetime2(0)")`, `"fechaRevision"`; `RevisadoPor` → `HasMaxLength(150)`, `"revisadoPor"`.
   - Índices: `HasIndex(e => e.IdOferta, "UX_ExperienciaEducativaPeriodo_idOferta").IsUnique().HasFilter("([idOferta] IS NOT NULL)")` y `HasIndex(e => new { e.IdPlanEstudios, e.IdPeriodo, e.EstadoAprobacion }, "IX_ExperienciaEducativaPeriodo_plan_periodo_estado")`.
   - Relación: `HasOne(d => d.IdOfertaNavigation).WithMany(p => p.ExperienciaEducativaPeriodo).HasForeignKey(d => d.IdOferta).OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_ExperienciaEducativaPeriodo_Oferta")`.
   - **Ojo:** `EliminarOfertaAsync` en `ProgramacionAcademicaRepository` borra la oferta con EF; con `SetNull` EF necesita la copia cargada o deja que la BD la ponga en `NULL`. Verifica que eliminar una oferta aprobada no falle (si EF se queja, agrega `.Include(o => o.ExperienciaEducativaPeriodo)` en esa consulta).

**Checkpoint 2:** build + tests sin fallos nuevos.

---

## FASE 3 — Repositorio y servicio de aprobación

### 3.1 DTOs (`SGPla/Models/DTOs/Planea/ProgramacionPlaneaDtos.cs`)
- Agregar a `CopiaProgramacionPlaneaDTO` (o crear un record nuevo `CopiaAprobacionPlaneaDTO` si cambiar el existente rompe usos) los campos: `int IdExperienciaEducativaPeriodo`, `string EstadoAprobacion`.
- Nuevo `record ResumenAprobacionPlaneaDTO(int Pendientes, int Aprobadas, int Descartadas, DateTime? UltimaRevision, string? RevisadoPor)`.
- Nuevo `record ResultadoAprobacionPlaneaDTO(int OfertasCreadas, int Descartadas, int Enlazadas)`.

### 3.2 `IProgramacionPlaneaRepository` / `ProgramacionPlaneaRepository`
- `ObtenerCopiasParaAprobarAsync(int idPlanEstudios, int idPeriodo, string? busqueda, ct)`: **todas** las copias enlazadas del plan × periodo (sin paginar), ordenadas por estado (Pendiente primero) y NRC, con horarios y docentes. Reutiliza la proyección de `ObtenerCopiasAsync` (extrae un método privado común si conviene).
- `ObtenerResumenAprobacionAsync(int idPlanEstudios, int idPeriodo, ct)` → `ResumenAprobacionPlaneaDTO`.
- `AprobarAsync(int idPlanEstudios, int idPeriodo, IReadOnlyCollection<int> idsAprobados, int idArticulo, string revisadoPor, ct)` → `ResultadoAprobacionPlaneaDTO`. En **una transacción** (`_context.Database.BeginTransactionAsync`):
  1. Cargar las copias `Pendiente`, enlazadas, de ese plan × periodo, con `Horarios`, `Docentes` y `IdExperienciaEducativaNavigation` + `IdPlanEstudiosNavigation` (para `IdProgramaEducativo`).
  2. Cargar `Docente` por los `NumeroPersonal` de esos docentes (una sola consulta) → diccionario.
  3. Cargar `Oferta` existentes del periodo con esos NRC (una consulta) → si una copia aprobada ya tiene `Oferta` con el mismo `(IdPeriodo, Nrc)` e igual `IdExperienciaEducativa`, **enlazar** (`IdOferta = existente`) en lugar de crear (cuenta en `Enlazadas`).
  4. Para cada copia cuyo id esté en `idsAprobados` y no tenga oferta existente: crear `Oferta` (ver §1.3) con:
     - `Nrc`, `IdExperienciaEducativa`, `IdProgramaEducativo = IdPlanEstudiosNavigation.IdProgramaEducativo`, `IdPeriodo`, `IdArticulo`, `Hsm`, docente, `Incluida`.
     - `Horario`: uno por fila de `ExperienciaEducativaPeriodoHorario` con `Dia`, `HoraInicio`, `HoraFin`, `Salon = string.Join("/", [Edificio, Aula] no vacíos)` o `null`. Descarta duplicados exactos (día+horas+salón).
       Nota: `MapHorario` del repositorio de programación solo muestra el **primer** horario por día; no lo cambies en este alcance, pero anótalo.
     - `Log { Mensaje = Constantes.HISTORIAL_APROBADA_PLANEA, Fecha = DateTime.UtcNow }` (agrega la constante en `SGPla/Commons/Constantes.cs` junto a las `HISTORIAL_*`, p. ej. `"Programación aprobada por DGAA desde PLANEA."`).
     - Copia → `EstadoAprobacion = Aprobada`, `IdOfertaNavigation = oferta`.
  5. Las demás copias pendientes → `Descartada`.
  6. A todas las procesadas: `FechaRevision = DateTime.UtcNow`, `RevisadoPor = revisadoPor`.
  7. `SaveChangesAsync` + `CommitAsync`.
  - Ids de `idsAprobados` que no pertenezcan al plan × periodo o que ya no estén `Pendiente` (doble submit) se **ignoran**.
- `RestaurarAsync(int idExperienciaEducativaPeriodo, ct)` → `bool`: solo `Descartada → Pendiente` (limpia `FechaRevision`/`RevisadoPor`).

### 3.3 `IProgramacionPlaneaService` / `ProgramacionPlaneaService`
- `ObtenerParaAprobarAsync(idPlanEstudios, idPeriodo, busqueda, ct)` → copias + resumen.
- `AprobarAsync(...)`:
  - Valida que `idArticulo` exista (inyecta `IArticuloService` de `SGPla/Services/Interfaces/IArticuloService.cs`, método `ObtenerArticuloPorIdAsync`); si no → `(false, "Selecciona un artículo válido.")`.
  - Valida que haya al menos una copia pendiente; si no → `(false, "No hay NRC pendientes por confirmar.")`.
  - Permite confirmar con `idsAprobados` vacío (equivale a descartar todo) — el front pide confirmación explícita en ese caso.
  - Devuelve `(bool Exito, string Mensaje, ResultadoAprobacionPlaneaDTO?)`.
- `RestaurarAsync(id, ct)`.
- Helper **público estático** `CalcularHsm(IEnumerable<HorarioPlaneaDTO o (TimeOnly, TimeOnly)> horarios, string? horasExperiencia)` con la regla de §1.3 (los minutos de cada bloque se suman; `14:00–14:59` = 59 min → se trata como 60: suma `(fin - inicio).TotalMinutes + 1` cuando `fin.Minute == 59`, o simplemente redondea la suma total de horas al entero más cercano; documenta la elección con un comentario corto).

**Checkpoint 3:** build + pruebas de la FASE 6 relativas al servicio (puedes escribirlas ya).

---

## FASE 4 — Visibilidad en el resumen (Index)

1. `SGPla/Models/DTOs/ProgramacionAcademica/BuscarProgramacionAcademicaDTO.cs`: `public bool IncluirPlanea { get; set; }`.
2. `ProgramacionAcademicaRepository.ObtenerResumenPorProgramaPeriodoAsync`: la consulta y la mezcla de filas PLANEA (bloque «También cuentan como programados…») **solo corre si `filtro?.IncluirPlanea == true`**. Además, para DGAA calcula por plan × periodo `NrcPendientes` y `NrcAprobados` (agrúpalos en la misma consulta de `ConsultarProgramacionPlanea`).
3. `ResumenOfertaProgramacionAcademicaDTO`: agregar `int NrcPendientes`, `int NrcAprobados`.
4. `ObtenerPeriodoMasRecienteConProgramacionAsync`: recibe `bool incluirPlanea`; si es `false` solo considera `Oferta`. Propaga el parámetro por `IProgramacionAcademicaService.ObtenerPeriodoActualAsync` e `IProgramacionAcademicaRepository`.
5. Resultado: para la EA el resumen sale solo de `Oferta`, que solo existe tras aprobar → «le aparece» la programación.

---

## FASE 5 — Controlador y vistas

### 5.1 `SGPla/Controllers/ProgramacionesAcademicasController.cs`
- Inyectar `IArticuloService` (para el select de artículos).
- `Index`: `var esDgaa = User.IsInRole(Constantes.COORDINADOR_DGAA);` → `IncluirPlanea = esDgaa` en el filtro y en `ObtenerPeriodoActualAsync(esDgaa)`.
- `LlenarTablaResumen`:
  - Acción «planea» **solo para DGAA**. `AriaLabel = r.NrcPendientes > 0 ? "Revisar programación PLANEA" : "Ver programación PLANEA"`.
  - Para DGAA agrega una columna «Estado PLANEA» (header solo si es DGAA): `Pendiente (N)` si no hay aprobados, `Aprobada` si no hay pendientes, `Aprobada · N nuevos` si hay ambos, `—` si la fila no tiene PLANEA.
- `ProgramacionPlanea` (GET):
  - Cambiar a `[Authorize(Policy = PoliticasAutorizacion.Dgaa)]` y quitar el bloque `Forbid` de EA.
  - Usar `ObtenerParaAprobarAsync`; la tabla pasa a paginación **client** (todas las filas en el DOM para que el form envíe todos los checkboxes). Mantén el `Buscador` (filtra en servidor) y elimina `cambiarPaginaPlanea` si deja de usarse.
  - Llenar en el ViewModel: `Articulos` (`List<OptionModel>` desde `IArticuloService.ObtenerTodosAsync`, texto `"Art. {Numero}"`), `Resumen` (`ResumenAprobacionPlaneaDTO`), `HayPendientes`.
- Nuevo POST:
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize(Policy = PoliticasAutorizacion.Dgaa)]
  public async Task<IActionResult> AprobarProgramacionPlanea(int idPlanEstudios, int idPeriodo, int idArticulo, int[]? idsAprobados)
  ```
  `revisadoPor` = `User.Identity?.Name` (o el claim de nombre/correo que use `LoginController`; revísalo). Éxito → `TempData["Success"] = $"Programación confirmada: {creadas} NRC aprobados, {descartados} descartados."` y `RedirectToAction(nameof(Index), new { idPeriodo })`. Error → `TempData["Error"]` y volver a `ProgramacionPlanea`. (Confirma las claves de TempData que usa el `Toast` del layout DGAA.)
- Nuevo POST `RestaurarCopiaPlanea(int idExperienciaEducativaPeriodo, int idPlanEstudios, int idPeriodo)` con las mismas anotaciones → redirect a `ProgramacionPlanea`.
- `LlenarTablaPlanea` (modo aprobación): headers `["", "NRC", "Experiencia educativa", "Horario y espacio", "Imparte", "Docente", "Estado"]`.
  - Celda 0: checkbox con `CheckboxName = "idsAprobados"`, `CheckboxValue = IdExperienciaEducativaPeriodo`, `Checked = true` y habilitado si `Pendiente`; deshabilitado (Aprobada → marcado, Descartada → desmarcado) en otro caso.
  - Celda «Estado»: texto del estado; en `Descartada` agrega una acción «Restaurar» (usa un `Accion` existente del `BotonViewComponent`, p. ej. `"izquierda"` o el que mejor encaje; revisa `SGPla/ViewComponents/BotonViewComponent.cs`) que haga POST a `RestaurarCopiaPlanea` vía un form oculto + JS.

### 5.2 Componente tabla (checkbox editable)
- `SGPla/Models/Components/TableModel.cs` → `TableCellModel`: agregar `string? CheckboxName`, `string? CheckboxValue`, `bool CheckboxDisabled = true` (el default conserva el comportamiento actual de `DocentesController`).
- `SGPla/Models/Components/CheckboxFieldModel.cs`: agregar `string? Value`.
- `SGPla/Views/Shared/Components/Table/Default.cshtml` (líneas ~21-28): pasar `Name`, `Value`, `Disabled = cell.CheckboxDisabled`.
- Vista del `CheckboxField` (`Views/Shared/Components/CheckboxField/Default.cshtml`): renderizar `name` y `value` si vienen. Verifica que los usos actuales no cambien.
- Revisa el JS de paginación client (`data-pagination-mode="client"`, en `wwwroot/js`): si oculta filas con CSS los checkboxes ocultos **sí** se envían; si las quita del DOM, en el submit hay que reinsertarlas o serializar los ids marcados en hidden inputs.

### 5.3 `ProgramacionPlaneaViewModel` y `Views/ProgramacionesAcademicas/ProgramacionPlanea.cshtml`
- ViewModel: `List<OptionModel> Articulos`, `ResumenAprobacionPlaneaDTO Resumen`, `bool HayPendientes`.
- Vista:
  - Bajo los campos de encabezado, una franja con el resumen: «Pendientes: N · Aprobados: N · Descartados: N» y, si hay `UltimaRevision`, «Última confirmación: dd/MM/yyyy HH:mm por X» (convierte a hora de México como en `VerHistorialExperienciaEducativa`).
  - Si `HayPendientes`: `<form id="formAprobarPlanea" method="post" asp-action="AprobarProgramacionPlanea">` con `@Html.AntiForgeryToken()`, hidden `idPlanEstudios`/`idPeriodo`, `SelectField` de artículo (requerido), checkbox «Seleccionar todos los pendientes», contador «N de M seleccionados» y botón «Confirmar programación».
    - El botón abre `abrirModalConfirmacion('Se crearán N ofertas y se descartarán M NRC. ¿Confirmar?', () => form.submit())` (la función ya existe y se usa en `VerProgramacionAcademica`). Si N = 0, mensaje explícito «Se descartarán todos los NRC pendientes».
  - La tabla debe quedar **dentro** del form (o los checkboxes deben usar `form="formAprobarPlanea"`).
  - Si no hay pendientes: sin form; solo el resumen y la tabla en solo lectura.

### 5.4 Permisos
- `SGPla/Helpers/Acciones.cs`: `Acciones.ProgramacionAcademica.AprobarPlanea`.
- `SGPla/Helpers/MatrizPermisos.cs`: agregarla a `COORDINADOR_DGAA`.

**Checkpoint 5:** build + tests sin fallos nuevos.

---

## FASE 6 — Pruebas

- `SGPla.Tests/Security/AutorizacionControllersTests.cs`: `ProgramacionPlanea`, `AprobarProgramacionPlanea` y `RestaurarCopiaPlanea` → policy `Dgaa`; los POST con `ValidateAntiForgeryToken`.
- `SGPla.Tests/Services/ProgramacionPlaneaServiceTests.cs` (Moq sobre el repositorio, como las pruebas existentes):
  - artículo inexistente → error, no llama al repositorio;
  - sin pendientes → error;
  - lote válido → delega en `AprobarAsync` con los ids y `revisadoPor`;
  - `CalcularHsm`: 5 bloques de 14:00–14:59 → 5; sin horarios y `Horas = "6"` → 6; sin nada → 0.
- Si el proyecto de pruebas ya usa un `DbContext` en memoria/SQLite para repositorios, agrega pruebas de `ProgramacionPlaneaRepository.AprobarAsync`: aprobados → `Oferta` + `Horario` + `Log`; resto → `Descartada`; docente con NP en catálogo → `IdDocente`; docente desconocido → campos importados; sin docente → vacante con `Incluida = true`; NRC con oferta existente → enlazado sin duplicar; ids ajenos ignorados. Si no existe esa infraestructura, **no la crees**: anótalo y cúbrelo en la verificación manual.
- `SGPla.Tests/Services/ProgramacionAcademicaServiceTests.cs`: ajustar firmas cambiadas (`ObtenerPeriodoActualAsync(bool)`).

---

## FASE 7 — Seed y verificación manual

- `SGPla.DbMigrator/database/seed/development.sql` **tiene cambios sin commitear del usuario**: lee el archivo completo antes de editarlo y conserva esos cambios. Asegura que exista al menos un plan × periodo con copias PLANEA enlazadas en estado `Pendiente` (con horarios y algún docente) para probar. Si el seed ya las crea, no agregues nada.
- Verificación (con Docker):
  1. `docker compose run --rm db-init` y levantar la app.
  2. Login **DGAA** → Index: fila con «Estado PLANEA: Pendiente (N)» y acción «Revisar».
  3. Abrir la revisión, desmarcar 2 NRC, elegir artículo, confirmar → toast con conteos; la fila pasa a «Aprobada».
  4. Login **EA** de esa entidad: **antes** de aprobar no veía la fila; ahora aparece con «Ver». Asignadas = NRC con docente PLANEA; vacantes = el resto; horarios con salón `EDIFICIO/AULA`; H/S/M calculado.
  5. Como DGAA: los 2 NRC desmarcados aparecen «Descartada» y deshabilitados; «Restaurar» los regresa a «Pendiente» y se pueden confirmar en una segunda ronda sin tocar lo ya aprobado.
  6. Como EA, intentar abrir `/ProgramacionesAcademicas/ProgramacionPlanea?...` → 403.

---

## Archivos

**Crear**
- `SGPla.DbMigrator/database/migrations/0017_aprobacion_programacion_planea.sql`

**Modificar**
- `SGPla/Models/ExperienciaEducativaPeriodo.cs`, `SGPla/Models/Oferta.cs`, `SGPla/Data/GestionDePlazasDbContext.cs`
- `SGPla/Commons/PlaneaConstantes.cs`, `SGPla/Commons/Constantes.cs`
- `SGPla/Models/DTOs/Planea/ProgramacionPlaneaDtos.cs`, `SGPla/Models/DTOs/ProgramacionAcademica/BuscarProgramacionAcademicaDTO.cs`, `ResumenProgramacionAcademicaDTO.cs` (donde viva `ResumenOfertaProgramacionAcademicaDTO`)
- `SGPla/Repositories/{Interfaces,Implementations}/ProgramacionPlaneaRepository.cs`, `ProgramacionAcademicaRepository.cs`
- `SGPla/Services/{Interfaces,Implementations}/ProgramacionPlaneaService.cs`, `ProgramacionAcademicaService.cs`
- `SGPla/Controllers/ProgramacionesAcademicasController.cs`
- `SGPla/Models/ViewModels/ProgramacionesAcademicas/ProgramacionPlaneaViewModel.cs`
- `SGPla/Views/ProgramacionesAcademicas/ProgramacionPlanea.cshtml`
- `SGPla/Models/Components/TableModel.cs`, `SGPla/Models/Components/CheckboxFieldModel.cs`, `SGPla/Views/Shared/Components/Table/Default.cshtml`, vista de `CheckboxField`
- `SGPla/Helpers/Acciones.cs`, `SGPla/Helpers/MatrizPermisos.cs`
- `SGPla.Tests/Security/AutorizacionControllersTests.cs`, `SGPla.Tests/Services/ProgramacionPlaneaServiceTests.cs`, `SGPla.Tests/Services/ProgramacionAcademicaServiceTests.cs`
- `SGPla.DbMigrator/database/seed/development.sql` (solo si hace falta, conservando los cambios del usuario)

## Fuera de alcance (anotar, no implementar)
- `Index` y `Ver` no restringen a la EA a su propia entidad por claim `EntidadAcademicaId`; es un hueco de autorización independiente de este cambio.
- `MapHorario` muestra solo el primer horario por día.
- La sincronización PLANEA, `SGPla/Modules/` y `sgpla-app/` no se tocan.

## Resumen final esperado del implementador
- Fases completadas y checkpoints (con salida real de build/tests).
- Desviaciones respecto a este plan y por qué.
- Qué quedó pendiente (p. ej. verificación con Docker).
