# Probar SGPLA desde cero y volver a tus datos

Se preparó un segundo proyecto Docker llamado **sgpla-pruebas**. No reemplaza ni combina el docker-compose.yml habitual. La aplicación normal y la de pruebas pueden estar abiertas simultáneamente.

| | Sistema habitual | Sistema de pruebas |
| --- | --- | --- |
| Dirección | http://localhost:8080 | http://localhost:8081/Login |
| Proyecto Docker | sistema_sgpla | sgpla-pruebas |
| Servidor SQL | contenedor db habitual | otro contenedor db, en otra red |
| Volumen SQL | volumen habitual existente | sgpla-pruebas_sql-pruebas |
| Documentos | SGPla/Archivos | sgpla-pruebas_archivos-pruebas |
| Plantillas | SGPla/Archivos/plantillas | misma carpeta, solo lectura |
| Sesión | cookies habituales | cookies SGpla.Pruebas separadas |

Ambos servidores tienen una base llamada GestionDePlazasBD. **No es la misma base**: están en contenedores, redes y volúmenes separados. No se publica el puerto SQL del servidor de pruebas al equipo.

## Qué ejecutar

Abra PowerShell en `C:\Users\axell\Downloads\Sistema_SGPLA` y tenga Docker Desktop iniciado.

### Iniciar o volver a abrir pruebas

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Iniciar
```

La primera ejecución pide un correo si aún no existe configuración. Para proporcionarlo expresamente:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Iniciar -CorreoAdministrador zs22013696@estudiantes.uv.mx
```

El script construye las imágenes de pruebas, espera a SQL, ejecuta las migraciones, registra el acceso inicial si no hay administrador e inicia la aplicación. Comprueba que la página de Login responda y muestre el rótulo Entorno de pruebas.

Entre en http://localhost:8081/Login con ese correo y su contraseña institucional habitual. La contraseña se valida con LDAP: no se cambió, no se guarda en el script y no se creó una contraseña alternativa. Se necesita acceso de red al LDAP de la universidad.

El correo configurado es **SuperUsuario únicamente en pruebas**. Desde esa cuenta cree las áreas, entidades y coordinadores necesarios. La aplicación da prioridad a SuperUsuario cuando un correo tiene varios roles; use cuentas institucionales distintas para probar cada rol. No se habilitó un acceso que omita LDAP.

### Volver a los registros anteriores

Simplemente abra **http://localhost:8080**. La aplicación normal sigue usando su base de siempre; no hay que cambiar cadenas de conexión.

Si también quiere detener pruebas para liberar memoria:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Volver
```

Primero detiene sgpla-pruebas conservando sus volúmenes. Después inicia o recupera los contenedores habituales con las imágenes existentes y el volumen original. No ejecuta el migrador habitual ni reconstruye sus imágenes.

Se utiliza docker-compose.volver.yml, que declara el volumen original como externo. Si falta ese volumen, el comando falla en lugar de crear una base vacía. También sirve si ejecutó docker compose down **sin** -v y ya no existen los contenedores habituales.

Este paso de recuperación está configurado para el proyecto original de este equipo: sistema_sgpla y el volumen sistema_sgpla_sgpla-sqlserver-data.

### Consultar el estado de pruebas

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Estado
```

### Detener pruebas sin borrar nada

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Detener
```

### Validar la configuración sin iniciar servicios

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Entorno-Pruebas.ps1 -Accion Validar
```

ExecutionPolicy Bypass se aplica solo a ese proceso de PowerShell; no cambia permanentemente la política del equipo.

## Qué significa empezar de cero

En el primer inicio se crean las tablas y se aplican las migraciones actuales. El seed de ejemplos está desactivado. No se copian sus avisos, documentos, usuarios, entidades, programas ni otros registros de la base habitual.

Se crea solamente el administrador inicial. Algunas migraciones insertan catálogos estructurales necesarios, como modalidades; esos registros forman parte del esquema inicial. Debe registrar el resto desde la aplicación.

**Iniciar de nuevo no vacía la base.** Reutiliza los mismos volúmenes y conserva lo que vaya capturando en pruebas. No se incluyó un comando de borrado o reinicio destructivo.

## Configuración privada

La primera ejecución crea `.local/sgpla-pruebas/entorno.env` con una contraseña SQL aleatoria exclusiva para pruebas y el correo del administrador. Está excluido de Git y del contexto de construcción de Docker.

Conserve ese archivo para volver a iniciar el mismo volumen. No cambie la contraseña del archivo dejando intacto el volumen SQL, porque la contraseña dentro del servidor no se actualizaría automáticamente. No lo comparta ni lo suba al repositorio.

La contraseña SQL es distinta de su contraseña institucional. El script no necesita ni solicita su contraseña de LDAP.

## Separación y seguridad de los datos

- Se mantienen intactos docker-compose.yml y sus volúmenes.
- Las operaciones de pruebas fijan `--project-name sgpla-pruebas` y usan solamente docker-compose.pruebas.yml. Volver inicia explícitamente sistema_sgpla con su volumen externo existente.
- No se usan redes ni volúmenes externos del proyecto original.
- No se ejecutan DELETE, TRUNCATE, DROP ni down -v.
- Los documentos se guardan en `/datos-pruebas`, evitando copiar los documentos incluidos en la imagen a un volumen nuevo.
- Las plantillas se comparten en modo de solo lectura; no se copian los documentos antiguos.
- Las cookies de autenticación, sesión y antifalsificación son distintas cuando EntornoPruebas está activado en Development.
- La base original no recibe migraciones ni consultas de estos scripts.

## Si falla el inicio

El script se detiene ante un fallo de Docker o migraciones. Lo que alcance a crear queda dentro de sgpla-pruebas; no intenta arreglar el entorno habitual ni borrar información.

Logs de pruebas:

```powershell
docker compose -p sgpla-pruebas --env-file .\.local\sgpla-pruebas\entorno.env -f .\docker-compose.pruebas.yml logs --tail 80
```

Si el puerto 8081 está ocupado, detenga únicamente la aplicación que lo esté usando o solicite ajustar el puerto del entorno de pruebas. No necesita detener el 8080.

Si el equipo se queda corto de memoria, puede detener pruebas con el comando Detener. Tener dos servidores SQL y dos aplicaciones consume más recursos que una sola instancia.

Los cambios futuros de código se incorporan al volver a ejecutar Iniciar porque reconstruye las imágenes usando la caché. En la base de pruebas se aplican las migraciones pendientes sin borrar sus datos de pruebas.
