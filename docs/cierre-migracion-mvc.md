# Cierre de migración MVC

## Clasificación de pruebas legacy que fallaban

Las 17 fallas de partida ejercitaban controladores o servicios MVC marcados
`[NonController]` y contratos de código retirado. Se decidió retirar esas pruebas
con sus componentes legacy, con las siguientes sustituciones y alcances:

| Pruebas retiradas | Decisión | Cobertura normalizada vigente |
| --- | --- | --- |
| 7 `PlanEstudiosValidatorTests` | Se elimina el validador legacy. Crear el plan sin experiencias ya no es una condición de negocio; las claves/duplicados y la importación se validan en el flujo normalizado. | `PlanEstudiosMvcServiceTests`, `PlanEstudiosExcelImportadorTests`, `NormalizedPlanEstudiosCrudSqlServerTests` |
| `PlanEstudiosServiceTests.ProcesarArchivoDePlanDeEstudios` | Se elimina junto con el servicio e importación legacy que esperaba encabezados obsoletos. | `PlanEstudiosExcelImportadorTests`, `PlanEstudiosMvcServiceTests`, `NormalizedPlanEstudiosCrudSqlServerTests` |
| 5 casos `AvisosRevisionTests.Dgaa_ConservaAccesoAlDocumentoTrasRevision` | Se elimina con `AvisosController`. Se conserva y prueba el acceso para el ámbito DGAA a documentos del aviso a lo largo de los estados de revisión. | `NormalizedDocumentoAvisoSqlServerTests.Carga_versiona_original_y_rechaza_entidad_ajena` ampliada con la comprobación de acceso DGAA |
| 2 `AspiranteValidatorTest` | Se elimina con el flujo legacy de aspirantes; la migración MVC vigente solo ofrece el directorio normalizado de docentes. No hay operación MVC de aspirantes que deba recibir estas reglas. | No aplica al alcance MVC actual |
| `IntegranteCtServiceTest.ObtenerTodosLosIntegrantes` | Se elimina con el agregado legacy de integrantes CT, reemplazado por el flujo versionado de Consejo Técnico. | `IntegranteConsejoTecnicoMvcServiceTests`, `NormalizedIntegranteConsejoTecnicoSqlServerTests` |
| `ProgramaEducativoValidatorTest.EditarProgramaEducativoConNombreInvalido` | Se elimina el validador legacy y se prueba la regla de nombre en el servicio MVC normalizado. | `ProgramaEducativoMvcServiceTests.Guardar_rechaza_nombre_invalido_antes_de_consultar_catalogos` |

Ninguna prueba se deshabilitó con `Skip`. Las pruebas de SQL Server y LDAP
conservan sus omisiones condicionadas exclusivamente a la falta de infraestructura.

## Validación de dependencias y datos

- Los módulos de `SGPla/Modules` conservan sus repositorios y el contexto legacy;
  son consumidos por sus controladores REST. Las implementaciones MVC legacy en
  `SGPla/Repositories` no eran usadas por esos módulos y se retiraron.
- El contexto `GestionDePlazasDbContext` permanece registrado para REST. Fuera de
  `Modules`, `Data` y la composición de DI en `Program.cs`, los componentes MVC
  deben usar `SgplaDbContext`.
- Las entidades se seleccionan por ID. Se retiraron el DTO de programación con
  IDs predeterminados y la sesión con entidad predeterminada `1`.
- No queda la resolución temporal por primer campus de región ni el parseo de
  etiquetas de entidad a partir de texto `clave-nombre` en el código MVC activo.
- Los enlaces a Estadísticas y Notificaciones se quitaron de los menús porque no
  hay controladores MVC para esas rutas.

## Decisiones de negocio pendientes

Estas decisiones se documentan sin cambiar el comportamiento:

1. Confirmar la regla de fechas de periodo: `AAAA51` = febrero–julio y `AAAA01`
   = agosto–enero, incluyendo el criterio para año calendario y cruces de año.
2. Decidir si Artículo requiere baja lógica; la tabla actual no tiene
   `fecha_eliminacion`.
3. Actas siguen fuera del alcance MVC. Las tablas existen desde `0026`, pero no
   hay flujo MVC de actas ni de su revisión.
4. Revisar la política de contraseña del superusuario y los parámetros/rotación
   del hash Argon2id.

## PLANEA: autenticación confirmada

El responsable confirmó que el endpoint recibe la API key mediante el header
`X-API-KEY`. `Planea:ModoAutenticacion` está configurado como `Header` y
`Planea:NombreParametro` como `X-API-KEY`; ambos caminos (header y query) siguen
siendo configurables para mantener el cliente adaptable. La llamada real para
el periodo `202701` requiere conectividad con la red institucional y se valida
desde allí.

## Estado de validación real

La cadena configurada para desarrollo apunta a `GestionDePlazasBD` en el
contenedor local `sistemagestionplazas-db-1`. El preflight de DbUp confirmó 26
scripts en el journal y que `0020`–`0027` ya estaban aplicados. La ejecución del
migrador terminó correctamente con “No new scripts need to be executed”. La
base tiene un superusuario previo, por lo que no se ejecutó el bootstrap: el
comando está diseñado para crear el primero únicamente cuando no exista uno.

La base compartida no tiene los catálogos completos que requieren las pruebas
de integración (por ejemplo, no había municipios ni sistemas educativos). Para
no sembrar datos de prueba en ella, se levantó un SQL Server desechable aparte,
se aplicó la cadena completa de migraciones con semilla de desarrollo y se
añadió el mínimo catálogo faltante para los casos que lo necesitaban. En la
validación inicial de esta entrega, `dotnet test` terminó con **267 correctas,
0 con error, 1 omitida, 268 total**. La única omitida es
`LdapStartTlsIntegrationTests`, porque no se proporcionaron
`SGPLA_LDAP_STARTTLS_HOST` y `SGPLA_LDAP_STARTTLS_PORT`.

La credencial PLANEA está configurada en User Secrets y la aplicación compila y
prueba el header confirmado `X-API-KEY` y el modo query configurable con claves
falsas. La llamada real para `202701` no se ejecutó porque esta sesión no tenía
conectividad a la red institucional; los registros de
`integracion.sincronizacion_planea`,
`academico.horario_programacion` y `academico.asignacion_docente` quedan por
verificar al ejecutar la sincronización desde esa red.

## Dimensionamiento de sincronización PLANEA

- El ejemplo local `Endpoint.json` de `202701` devuelve un objeto JSON con
  `periodo`, `total`, `resultado` y `horarios`. `resultado` tiene 18 203 filas
  administrativas que no se usan por ahora; `horarios` contiene 22 623 filas
  con sesiones/docentes. El archivo mide 68 129 596 bytes sin comprimir. El
  lector busca `periodo` y `horarios` sin depender del orden observado, recorre
  `resultado` token a token sin guardar sus valores y materializa cada objeto de
  `horarios` al terminar de leerlo. `total` se ignora: en el ejemplo corresponde
  a `resultado`, no al número de filas de `horarios`, y no se conoce un contrato
  adicional para validarlo.
- Estas reglas del validador son **pendientes de confirmación de negocio**:
  omitir NRC sin programación MVC local, elegir docente por `IND_PRINCIPAL`,
  menor `IND_DOCENTE` y menor `ID_DOCENTE`, mantener sesiones aunque la
  identidad docente venga incompleta y elegir el nombre más frecuente cuando
  un identificador presenta variantes. También queda **pendiente de confirmar
  con negocio** omitir el registro con sesión válida pero fechas vacías,
  contarlo como ignorado y sumar una advertencia. Una fecha presente con formato
  distinto de `yyyy-MM-dd` y los rangos inversos siguen rechazándose. El periodo,
  formato de NRC, horas y pares inicio/fin mantienen validación estricta.
- No se agregó columna al esquema. El validador devuelve el contador
  `NrcSinProgramacion`, pero la bitácora SQL lo incluye en
  `RegistrosIgnorados`, junto con filas sin sesiones y filas con sesión pero
  fechas vacías. `RegistrosIgnorados = NrcSinProgramacion + registros sin
  sesiones + registros con sesión sin fechas`; el esquema no persiste esos
  componentes por separado. `Advertencias` suma sesiones fuera del periodo,
  traslapes, co-docencia (una por NRC), variantes de nombre (una por ID),
  registros con sesión y fechas vacías (una por registro) y discrepancias de
  asignación detectadas durante la sincronización.
- `Planea:TamanoMaximoMb` tiene default `200` y acepta de `1` a `2048`. El
  límite cuenta bytes descomprimidos; se conserva el rechazo temprano por
  `Content-Length`. El cliente rechaza una raíz que no sea objeto tras leer solo
  el primer token y procesa la sección `horarios` registro por registro.
  El `SocketsHttpHandler` activa descompresión automática para gzip/deflate/br;
  el límite de lectura aplica a los bytes entregados al cliente después de esa
  descompresión. Se conserva el rechazo temprano por `Content-Length` declarado.
- Medición de la muestra local de 22 623 registros: `Endpoint.json` ocupa
  `68 129 596` bytes; parser en `442–500 ms`; heap administrado antes de leer
  `5.5–6.8 MB` y después de GC con los DTOs retenidos `48.2–49.2 MB`.
  No es una medición de RSS ni se ejecutó `AplicarSnapshotAsync` con esa muestra.
  La prueba sintética de 100 000 registros (3 500 048 bytes) observó heap antes
  `5 861 848`, pico muestreado `86 579 440` y heap después de GC `87 806 000`
  bytes; el conjunto de DTOs retenido domina esa cifra. La prueba adicional
  recorrió `46 200 135` bytes de `resultado` y asignó `198 728` bytes en el
  proceso, sin almacenar esas filas. La prueba está en una colección xUnit sin
  paralelismo y usa `GC.GetTotalAllocatedBytes`.
- La prueba SQL reemplazó 20 000 sesiones: `AplicarSnapshotAsync` reportó
  `399 ms`; el ciclo completo desde el servicio tomó `405 ms` en el contenedor
  local. El fixture es sintético y pequeño fuera de las sesiones (un solo
  programa destino); no predice latencia de producción.
- El POST ahora registra y encola en un canal acotado de capacidad 10. El worker
  ejecuta en un scope propio y el GET
  `/ProgramacionesAcademicas/EstadoSincronizacionPlanea/{id}` devuelve la
  bitácora. Al arranque se cierran como `FALLIDA` las filas `EN_PROCESO` que
  hayan sobrevivido un reinicio. El canal es en memoria; la aplicación debe
  correr como instancia única para no invalidar trabajos de otra instancia.
- La aplicación SQL configura timeout de comandos de 300 segundos únicamente
  durante la transacción y restaura la configuración previa. Los horarios se
  escriben con `SqlBulkCopy` y un `IDataReader` sobre la lista validada, sin
  materializar una segunda tabla completa. Se conserva `Serializable`: además
  del índice único de bitácoras en curso, evita cambios concurrentes/phantom en
  el conjunto de programaciones durante el reemplazo. La descarga y validación
  permanecen antes de abrir la transacción; el error revierte el snapshot.
- Validación final después de estos cambios en el SQL Server desechable:
  **290 correctas, 0 con error, 1 omitida, 291 total**. La omitida es LDAP
  STARTTLS por falta de host/puerto. `dotnet build` termina con 0 errores y
  sin advertencias en los archivos de PLANEA.
- No se hizo ninguna llamada real a PLANEA. El archivo local es un ejemplo real
  aportado por el proyecto, no una respuesta obtenida en esta ejecución. No se
  confirmó que el servidor negocie gzip; la prueba gzip local solo verifica el
  cliente. Tampoco se confirmó que el orden de propiedades sea estable (el
  parser no depende de ese orden). `total` se interpreta únicamente como el
  conteo de `resultado` en el ejemplo. La sincronización real aún requiere red
  institucional.
- La prueba de humo con el archivo real y mapeo sintético de todos los NRC
  procesó 22 623 filas, 17 991 NRC distintos y 38 801 franjas informadas. El
  validador produjo 38 800 sesiones, 11 526 docentes, 1 145 ignorados, 0 NRC
  sin programación, 0 duplicados y 119 advertencias. Respecto a la referencia
  sin el NRC `24155` (38 798 sesiones, 11 525 docentes y 1 146 ignorados), el
  registro válido adicional de ese NRC aporta dos sesiones y un docente; su
  registro con sesión y fechas vacías se ignora con una advertencia. Las
  advertencias netas quedan en 119 porque al contar solo candidatos a docente
  se excluye también una variante de nombre que aparecía únicamente en filas
  sin sesión. Esta simulación verifica la regla sobre todos los NRC, no su
  coincidencia con programas locales. Las bases Docker disponibles no tienen
  un periodo `202701`, por lo que queda pendiente el recuento contra datos
  locales. Esta ejecución tardó `495 ms`; el heap fue `5 572 528` bytes antes
  y `48 277 272` bytes después de GC.

No se hizo un recorrido manual end-to-end por la interfaz. El inicio LDAP
depende de infraestructura no disponible en esta ejecución; el usuario
superusuario existente y sus credenciales no se modificaron. Las pruebas
automatizadas cubren los cambios MVC, catálogos y documentos indicados en este
cierre, pero no sustituyen la revisión manual de login, importaciones y flujos
completos de oferta, aviso, solicitud y Consejo Técnico.
