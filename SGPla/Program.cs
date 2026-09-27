using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Data.NewModel;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Validations.Interfaces;
using SGPla.Modules.SolicitudesApertura;
using SGPla.Modules.Articulos;
using SGPla.Modules.PeriodosEscolares;
using SGPla.Modules.DireccionesAreaAcademica;
using SGPla.Modules.EntidadesAcademicas;
using SGPla.Modules.ProgramasEducativos;
using SGPla.Commons;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Solo el Compose aislado activa este modo; no cambia las cookies del entorno habitual.
var entornoPruebas = builder.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("EntornoPruebas:Enabled");
if (entornoPruebas)
    builder.Services.AddAntiforgery(options => options.Cookie.Name = "SGPla.Pruebas.Antiforgery");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    if (entornoPruebas) options.Cookie.Name = "SGPla.Pruebas.Session";
    options.IdleTimeout = TimeSpan.FromHours(8); // antes: FromMinutes(30)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//ConectionString
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena 'DefaultConnection'.");

builder.Services.AddDbContext<GestionDePlazasDbContext>(options =>
    options.UseSqlServer(connectionString));

// El MVC nuevo usa exclusivamente los esquemas normalizados. El contexto
// Database-First anterior se conserva para los módulos REST legacy.
builder.Services.AddDbContext<SgplaDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
builder.Services.AddSingleton(TimeProvider.System);

//Clases
builder.Services.AddScoped<ICoordinadorEaRepository, NormalizedCoordinadorEaRepository>();
builder.Services.AddScoped<ICoordinadorDgaaRepository, NormalizedCoordinadorDgaaRepository>();
builder.Services.AddScoped<IUsuarioConsultaRepository, NormalizedUsuarioConsultaRepository>();
builder.Services.AddScoped<IUsuarioValidator, UsuarioValidator>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ISuperusuarioAdminService, SuperusuarioAdminService>();
builder.Services.AddScoped<ICatalogosMvcService, CatalogosMvcService>();
builder.Services.AddScoped<IAdministracionCatalogosMvcService, AdministracionCatalogosMvcService>();
builder.Services.AddScoped<IRegionCampusMvcRepository, NormalizedRegionCampusMvcRepository>();
builder.Services.AddScoped<IRegionCampusMvcService, RegionCampusMvcService>();
builder.Services.AddScoped<IProgramacionAcademicaMvcRepository, NormalizedProgramacionAcademicaMvcRepository>();
builder.Services.AddScoped<IProgramacionAcademicaMvcService, ProgramacionAcademicaMvcService>();
builder.Services.AddScoped<IDocenteDirectorioMvcRepository, NormalizedDocenteDirectorioMvcRepository>();
builder.Services.AddScoped<IDocenteDirectorioMvcService, DocenteDirectorioMvcService>();
builder.Services.AddScoped<IOfertaMvcRepository, NormalizedOfertaMvcRepository>();
builder.Services.AddScoped<IOfertaMvcService, OfertaMvcService>();
builder.Services.AddScoped<IAvisoMvcRepository, NormalizedAvisoMvcRepository>();
builder.Services.AddScoped<IAvisoMvcService, AvisoMvcService>();
builder.Services.AddSingleton<IAlmacenDocumentos, AlmacenDocumentosLocal>();
builder.Services.AddScoped<IDocumentoAvisoMvcRepository, NormalizedDocumentoAvisoMvcRepository>();
builder.Services.AddScoped<IDocumentoAvisoMvcService, DocumentoAvisoMvcService>();
builder.Services.AddScoped<IIntegranteConsejoTecnicoMvcRepository, NormalizedIntegranteConsejoTecnicoMvcRepository>();
builder.Services.AddScoped<IIntegranteConsejoTecnicoMvcService, IntegranteConsejoTecnicoMvcService>();
builder.Services.AddScoped<ISolicitudMvcRepository, NormalizedSolicitudMvcRepository>();
builder.Services.AddScoped<ISolicitudMvcService, SolicitudMvcService>();
builder.Services.AddHttpClient<IPlaneaClient, PlaneaClient>(cliente => cliente.Timeout = TimeSpan.FromMinutes(2));
builder.Services.AddScoped<IPlaneaSnapshotValidator, PlaneaSnapshotValidator>();
builder.Services.AddScoped<ISincronizacionPlaneaService, SincronizacionPlaneaService>();

builder.Services.AddScoped<IAreaAcademicaRepository, NormalizedAreaAcademicaRepository>();
builder.Services.AddScoped<IAreaAcademicaValidator, AreaAcademicaValidator>();
builder.Services.AddScoped<IAreaAcademicaService, AreaAcademicaService>();

builder.Services.AddScoped<IEntidadAcademicaRepository, EntidadAcademicaRepository>();
builder.Services.AddScoped<IEntidadAcademicaMvcRepository, NormalizedEntidadAcademicaMvcRepository>();
builder.Services.AddScoped<IEntidadAcademicaValidator, EntidadAcademicaValidator>();
builder.Services.AddScoped<IEntidadAcademicaService, EntidadAcademicaService>();
builder.Services.AddScoped<IEntidadAcademicaMvcService, EntidadAcademicaMvcService>();
builder.Services.AddScoped<IProgramaEducativoMvcRepository, NormalizedProgramaEducativoMvcRepository>();
builder.Services.AddScoped<IProgramaEducativoMvcService, ProgramaEducativoMvcService>();
builder.Services.AddScoped<IPlanEstudiosMvcRepository, NormalizedPlanEstudiosMvcRepository>();
builder.Services.AddScoped<IPlanEstudiosMvcService, PlanEstudiosMvcService>();
builder.Services.AddScoped<IPlanEstudiosExcelImportador, PlanEstudiosExcelImportador>();

builder.Services.AddScoped<IArticuloRepository, NormalizedArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<IArticuloValidator, ArticuloValidator>();

builder.Services.AddScoped<IProgramaEducativoRepository, ProgramaEducativoRepository>();
builder.Services.AddScoped<IProgramaEducativoService,  ProgramaEducativoService>();
builder.Services.AddScoped<IProgramaEducativoValidator, ProgramaEducativoValidator>();
builder.Services.AddScoped<IPlanEstudiosRepository, PlanEstudiosRepository>();
builder.Services.AddScoped<IExperienciaEducativaRepository, ExperienciaEducativaRepository>();
builder.Services.AddScoped<IPlanEstudiosValidator, PlanEstudiosValidator>();
builder.Services.AddScoped<IPlanEstudiosService, PlanEstudiosService>();

builder.Services.AddScoped<IPeriodoEscolarRepository, NormalizedPeriodoEscolarRepository>();
builder.Services.AddScoped<IPeriodoEscolarService, PeriodoEscolarService>();
builder.Services.AddScoped<IPeriodoEscolarValidator, PeriodoEscolarValidator>();

builder.Services.AddScoped<IArchivoRepository, ArchivoRepository>();
builder.Services.AddScoped<IArchivoService, ArchivoService>();

builder.Services.AddScoped<IIntegranteCtService, IntegranteCtService>();
builder.Services.AddScoped<IIntegranteCtRepository, IntegranteCtRepository>();
builder.Services.AddScoped<IIntegranteCtValidator, IntegranteCtValidator>();

builder.Services.AddScoped<IDocenteService, DocenteService>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IDocenteValidator, DocenteValidator>();
builder.Services.AddScoped<IAspiranteService, AspiranteService>();
builder.Services.AddScoped<IAspiranteRepository, AspiranteRepository>();
builder.Services.AddScoped<IAspiranteValidator, AspiranteValidator>();
builder.Services.AddScoped<IGradoRepository, GradoRepository>();

builder.Services.AddScoped<IProgramacionAcademicaRepository, ProgramacionAcademicaRepository>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();

builder.Services.AddScoped<IProgramacionAcademicaValidator, ProgramacionAcademicaValidator>();
builder.Services.AddScoped<IProgramacionAcademicaService, ProgramacionAcademicaService>();

builder.Services.AddScoped<IPlantillaService, PlantillaService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEstadoNavegacion, EstadoNavegacion>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        if (entornoPruebas) options.Cookie.Name = "SGPla.Pruebas.Auth";
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            // El MVC normalizado revalida rol y ámbito contra usuarios.* en cada
            // solicitud. Se conserva la validación anterior del API legacy.
            if (context.Request.Path.StartsWithSegments("/api"))
                return;

            var validator = context.HttpContext.RequestServices.GetRequiredService<IUsuarioSesionValidator>();
            var vigente = await validator.EsSesionVigenteAsync(
                context.Principal,
                context.HttpContext.RequestAborted);
            if (vigente)
                return;

            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Toda ruta queda cerrada salvo que se marque explícitamente con AllowAnonymous.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy(PoliticasAutorizacion.SuperUsuario,
        policy => policy.RequireRole(Constantes.SUPERUSUARIO));
    options.AddPolicy(PoliticasAutorizacion.Dgaa,
        policy => policy.RequireRole(Constantes.COORDINADOR_DGAA));
    options.AddPolicy(PoliticasAutorizacion.EntidadAcademica,
        policy => policy.RequireRole(Constantes.COORDINADOR_EA));
    options.AddPolicy(PoliticasAutorizacion.OperadorAcademico,
        policy => policy.RequireRole(Constantes.COORDINADOR_DGAA, Constantes.COORDINADOR_EA));
});
builder.Services.AddSolicitudesAperturaModule();
builder.Services.AddArticulosModule();
builder.Services.AddPeriodosEscolaresModule();
builder.Services.AddDireccionesAreaAcademicaModule();
builder.Services.AddEntidadesAcademicasModule();
builder.Services.AddProgramasEducativosModule();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILdapAuthService, LdapAuthService>();
builder.Services.AddSingleton<IArgon2idPasswordHasher, Argon2idPasswordHasher>();
builder.Services.AddScoped<IUsuarioSesionValidator, NormalizedUsuarioSesionValidator>();

builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddScoped<IAvisoRepository, AvisoRepository>();
builder.Services.AddScoped<IOfertaRepository, OfertaRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IAvisoValidator, AvisoValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();       // 1. primero rutea

app.UseSession();       // 2. sesión

app.UseAuthentication(); // 3. ¿quién eres?

if (app.Environment.IsDevelopment()
    && app.Configuration.GetValue<bool>("DevelopmentAuthentication:Enabled"))
{
    app.Use(async (context, next) =>
    {
        var esSolicitudesApertura = context.Request.Path
            .StartsWithSegments("/api/v1/solicitudes-apertura");
        var esArticulos = context.Request.Path
            .StartsWithSegments("/api/v1/articulos");
        var esPeriodosEscolares = context.Request.Path
            .StartsWithSegments("/api/v1/periodos-escolares");
        var esAreasAcademicas = context.Request.Path
            .StartsWithSegments("/api/v1/areas-academicas");
        var esEntidadesAcademicas = context.Request.Path
            .StartsWithSegments("/api/v1/entidades-academicas");
        var esProgramasEducativos = context.Request.Path
            .StartsWithSegments("/api/v1/programas-educativos");

        if (esSolicitudesApertura
            || esArticulos
            || esPeriodosEscolares
            || esAreasAcademicas
            || esEntidadesAcademicas
            || esProgramasEducativos)
        {
            var correo = app.Configuration["DevelopmentAuthentication:Email"];
            var claveRol = esArticulos
                ? "DevelopmentAuthentication:ArticulosRole"
                : esPeriodosEscolares
                    ? "DevelopmentAuthentication:PeriodosEscolaresRole"
                    : esAreasAcademicas
                        ? "DevelopmentAuthentication:AreasAcademicasRole"
                        : esEntidadesAcademicas
                            ? "DevelopmentAuthentication:EntidadesAcademicasRole"
                            : esProgramasEducativos
                                ? "DevelopmentAuthentication:ProgramasEducativosRole"
                                : "DevelopmentAuthentication:Role";
            var rol = app.Configuration[claveRol]
                ?? (esArticulos
                    || esPeriodosEscolares
                    || esAreasAcademicas
                    || esEntidadesAcademicas
                    || esProgramasEducativos
                    ? Constantes.SUPERUSUARIO
                    : Constantes.COORDINADOR_EA);

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new InvalidOperationException(
                    "DevelopmentAuthentication:Email es obligatorio para probar el endpoint.");
            }

            var identidad = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "Usuario de desarrollo"),
                    new Claim(ClaimTypes.Email, correo),
                    new Claim(ClaimTypes.Role, rol)
                },
                authenticationType: "DevelopmentAuthentication");

            context.User = new ClaimsPrincipal(identidad);
        }

        await next();
    });
}

app.UseMiddleware<CambioContrasenaObligatorioMiddleware>();
app.UseAuthorization();  // 4. ¿qué puedes hacer?

app.MapStaticAssets().AllowAnonymous();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

app.Run();
