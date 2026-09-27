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

- `Planea:TamanoMaximoMb` tiene default `200` y acepta de `1` a `2048`. El
  cliente verifica el primer token para rechazar una raíz que no sea arreglo
  antes de consumir el resto; los registros se deserializan desde el stream.
  El `SocketsHttpHandler` activa descompresión automática para gzip/deflate/br;
  el límite de lectura aplica a los bytes entregados al cliente después de esa
  descompresión. Se conserva el rechazo temprano por `Content-Length` declarado.
- Medición de la prueba generada de 100 000 registros: body de `1 900 001`
  bytes; heap administrado antes `5 440 744` bytes; pico observado por muestras
  durante lectura/procesamiento `71 134 784` bytes; heap retenido después de GC
  `72 174 912` bytes. Es una medida del heap administrado con DTOs retenidos,
  no un muestreo del pico RSS del proceso ni una comparación con un payload real
  de 50–70 MB.
- La prueba SQL reemplazó 20 000 sesiones: `AplicarSnapshotAsync` reportó
  `412 ms`; el ciclo completo desde el servicio tomó `461 ms` en el contenedor
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
  **280 correctas, 0 con error, 1 omitida, 281 total**. La omitida es LDAP
  STARTTLS por falta de host/puerto. `dotnet build` termina con 0 errores
  (238 advertencias).
- No se hizo ninguna llamada real a PLANEA. El payload de producción y el
  soporte gzip del servidor real no se pudieron verificar; la prueba gzip usa
  un servidor local que devuelve una respuesta comprimida y el mismo tipo de
  `SocketsHttpHandler` configurado en la aplicación. La sincronización real aún
  requiere red institucional.

No se hizo un recorrido manual end-to-end por la interfaz. El inicio LDAP
depende de infraestructura no disponible en esta ejecución; el usuario
superusuario existente y sus credenciales no se modificaron. Las pruebas
automatizadas cubren los cambios MVC, catálogos y documentos indicados en este
cierre, pero no sustituyen la revisión manual de login, importaciones y flujos
completos de oferta, aviso, solicitud y Consejo Técnico.
