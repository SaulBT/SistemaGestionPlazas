# Configuración de Entity Framework Core con Database First

Este documento explica cómo configurar la conexión a la base de datos **GestionDePlazasBD** en el proyecto **Gestión de Plazas** usando:

- **ASP.NET Core MVC**
- **SQL Server LocalDB**
- **Entity Framework Core**
- **Database First**
- **User Secrets** para no subir la cadena de conexión al repositorio

## Objetivo

Permitir que cualquier integrante del equipo clone el repositorio, configure su entorno local y genere correctamente los modelos y el contexto de Entity Framework sin exponer la cadena de conexión.

## Migraciones de base de datos

El esquema se versiona con **DbUp** porque este proyecto utiliza Database First:
el SQL es la fuente de verdad y el contexto de EF Core se regenera cuando cambia
la base de datos. El ejecutable `SGPla.DbMigrator` aplica el baseline existente y
las migraciones nuevas de `SGPla.DbMigrator/database/migrations` en orden alfabético.

Para ejecutar el migrador localmente:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=GestionDePlazasBD;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project SGPla.DbMigrator
```

También acepta `--connection=<cadena>`. En Docker Compose, el servicio
`db-init` construye y ejecuta el migrador una sola vez antes de iniciar el
backend. Compose activa `MIGRATOR_APPLY_DEVELOPMENT_SEED=true` para cargar los
datos ficticios de `SGPla.DbMigrator/database/seed/development.sql`; producción debe dejarlo en
`false`. El script `GestionDePlazasBD.sql` queda como respaldo histórico.

Para cada cambio de esquema, agregar un archivo como
`SGPla.DbMigrator/database/migrations/0002_descripcion.sql`. Una migración aplicada es inmutable:
para corregirla se agrega otra. El journal de DbUp es `dbo.SchemaVersions`.

## Requisitos previos

Antes de comenzar, verificar lo siguiente:

- Tener instalado **Visual Studio** con soporte para desarrollo en **ASP.NET Core**.
- Tener instalado **SQL Server / LocalDB**.
- Tener creada la base de datos **GestionDePlazasBD** en la instancia local correspondiente.
- Abrir el proyecto web correcto, es decir, el que contiene:
  - `Program.cs`
  - `appsettings.json`
  - el archivo `.csproj`

## 1. Instalar los paquetes de Entity Framework

Abrir en Visual Studio:

**Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes**

Instalar los paquetes necesarios para SQL Server, herramientas de EF Core y diseño:

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
```

Estos paquetes permiten:

- conectarse a SQL Server;
- ejecutar el scaffolding;
- generar entidades y contexto desde la base de datos.

## 2. Inicializar User Secrets

Cada integrante debe hacerlo en su propia máquina.

### Opción A: desde Visual Studio

- Clic derecho sobre el proyecto.
- Seleccionar **Manage User Secrets**.

### Opción B: desde terminal

Abrir una terminal en la carpeta donde está el archivo `.csproj` del proyecto y ejecutar:

```powershell
dotnet user-secrets init
```

Esto habilita el uso de secretos locales para ese proyecto.

## 3. Configurar la cadena de conexión local

Cada integrante debe guardar su propia cadena de conexión en su equipo.

Ejecutar en la carpeta del proyecto:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=GestionDePlazasBD;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Importante

- Este valor **no se sube al repositorio**.
- Cada integrante debe ejecutar este paso en su equipo.
- Si alguien usa otra instancia de SQL Server, debe cambiar el valor de `Server`.

## 4. Revisar `appsettings.json`

En el proyecto debe existir la clave de conexión, pero **sin la cadena real**.

Debe quedar así:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

La aplicación tomará el valor real desde User Secrets.

## 5. Verificar la configuración de la consola

Antes de ejecutar el scaffolding, revisar en Visual Studio:

- **Default project**: debe ser el proyecto donde se generarán `Models` y `Data`.
- **Startup project**: debe ser el proyecto web MVC.

Si esto está mal, pueden aparecer errores aunque la configuración sea correcta.

## 6. Generar las clases con Database First

Abrir la **Consola del Administrador de paquetes** y ejecutar el comando de scaffolding:

```powershell
Scaffold-DbContext Name=ConnectionStrings:DefaultConnection Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context GestionDePlazasDbContext -NoOnConfiguring -NoPluralize
```

Este comando hace lo siguiente:

- lee la cadena de conexión desde User Secrets;
- genera las entidades en la carpeta `Models`;
- genera el contexto en la carpeta `Data`;
- crea el contexto con nombre `GestionDePlazasDbContext`;
- evita guardar la cadena de conexión dentro del código;
- evita pluralizaciones incorrectas como `Actum` o `Asistencium`;
- respeta los nombres de la base de datos.

## 7. Configurar `Program.cs`

**Este paso debe hacerse después del scaffolding.**

La razón es que si se intenta registrar el contexto antes de que exista la clase `GestionDePlazasDbContext`, el proyecto marcará error de compilación y eso puede impedir el scaffolding.

Una vez generado el contexto, verificar que `Program.cs` tenga una configuración similar a esta:

```csharp
using Microsoft.EntityFrameworkCore;
using TuProyecto.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena 'DefaultConnection'.");

builder.Services.AddDbContext<GestionDePlazasDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Nota

Cambiar `TuProyecto.Data` por el namespace real del proyecto.

## 8. Revisar los archivos generados

Después del scaffolding deben existir, como mínimo:

- carpeta `Models` con las entidades;
- carpeta `Data` con `GestionDePlazasDbContext`.

## 9. Probar que la conexión funciona

Ejecutar el proyecto y hacer una prueba simple con alguna consulta usando el contexto.

El objetivo es confirmar que:

- la conexión funciona correctamente;
- la base de datos responde;
- las entidades y el contexto fueron generados correctamente.

## 10. Qué sí se sube al repositorio

Sí deben subirse:

- `Program.cs`;
- `appsettings.json`;
- `.csproj`;
- carpeta `Models`;
- carpeta `Data`.

## 11. Qué no se debe subir al repositorio

No debe subirse:

- la cadena de conexión real;
- archivos de secretos locales;
- usuario y contraseña de base de datos, si en algún momento se usan.

## 12. Si la base de datos cambia

Si se agregan, eliminan o modifican tablas en SQL Server:

1. agregar una migración nueva en `SGPla.DbMigrator/database/migrations`;
2. aplicarla con `SGPla.DbMigrator`;
3. volver a ejecutar el scaffolding para regenerar el modelo.

## Resumen del flujo

1. Instalar paquetes de Entity Framework.
2. Inicializar User Secrets.
3. Guardar la cadena local.
4. Revisar `appsettings.json`.
5. Verificar `Default project` y `Startup project`.
6. Ejecutar `Scaffold-DbContext`.
7. Configurar `Program.cs`.
8. Verificar archivos generados.
9. Probar la conexión.
