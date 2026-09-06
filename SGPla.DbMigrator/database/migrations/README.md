# Migraciones de la base de datos

Los archivos `.sql` de esta carpeta son migraciones incrementales ejecutadas por
`SGPla.DbMigrator` en orden alfabético. El baseline `0001_baseline_schema.sql`
corresponde al esquema existente en `../baseline_schema.sql`.

Reglas:

- Cada migración debe tener un nombre ordenable, por ejemplo
  `0002_agregar_columna_aviso.sql`.
- Una migración aplicada es inmutable. Para corregirla, agrega otra migración.
- Incluye los cambios de datos necesarios en la misma migración que el cambio de
  esquema y hazla segura para reintentos.
- Revisa y prueba el SQL antes de aplicarlo en producción.

La tabla `dbo.SchemaVersions` es el journal de DbUp. No debe editarse a mano,
salvo durante la adopción del baseline en una base ya existente y después de
validar que su esquema coincide.
