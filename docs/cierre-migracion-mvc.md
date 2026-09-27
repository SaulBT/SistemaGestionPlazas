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

## PLANEA: confirmación requerida

La búsqueda de documentación disponible no confirmó si el endpoint acepta una
credencial en header o query string, ni el nombre del campo. La implementación
deja ambos configurables mediante `Planea:ModoAutenticacion` y
`Planea:NombreParametro`. Se debe confirmar el mecanismo con el responsable o
validarlo en una llamada manual antes de declarar operativa la sincronización.

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
añadió el mínimo catálogo faltante para los casos que lo necesitaban. Resultado
de `dotnet test SGPla.Tests/SGPla.Tests.csproj --no-restore`: **267 correctas,
0 con error, 1 omitida, 268 total**. La única omitida es
`LdapStartTlsIntegrationTests`, porque no se proporcionaron
`SGPLA_LDAP_STARTTLS_HOST` y `SGPLA_LDAP_STARTTLS_PORT`.

La credencial PLANEA está configurada en User Secrets y la aplicación compila y
prueba los caminos de encabezado y query con credenciales falsas. No se pudo
confirmar en documentación disponible ni contactar de forma autorizada el
mecanismo que acepta el servicio remoto. Por eso no se envió la clave real con
un mecanismo supuesto y no se ejecutó la sincronización real de `202701`; los
registros de `integracion.sincronizacion_planea`,
`academico.horario_programacion` y `academico.asignacion_docente` quedan por
verificar cuando el responsable confirme el mecanismo. La configuración deja
seleccionable el modo y nombre del parámetro.

No se hizo un recorrido manual end-to-end por la interfaz. El inicio LDAP
depende de infraestructura no disponible en esta ejecución; el usuario
superusuario existente y sus credenciales no se modificaron. Las pruebas
automatizadas cubren los cambios MVC, catálogos y documentos indicados en este
cierre, pero no sustituyen la revisión manual de login, importaciones y flujos
completos de oferta, aviso, solicitud y Consejo Técnico.
