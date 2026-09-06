using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Validations.Interfaces;
using SGPla.Modules.SolicitudesApertura;
using SGPla.Commons;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // antes: FromMinutes(30)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//ConectionString
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena 'DefaultConnection'.");

builder.Services.AddDbContext<GestionDePlazasDbContext>(options =>
    options.UseSqlServer(connectionString));

//Clases
builder.Services.AddScoped<ICoordinadorEaRepository, CoordinadorEaRepository>();
builder.Services.AddScoped<ICoordinadorDgaaRepository, CoordinadorDgaaRepository>();
builder.Services.AddScoped<IUsuarioValidator, UsuarioValidator>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IAreaAcademicaRepository, AreaAcademicaRepository>();
builder.Services.AddScoped<IAreaAcademicaValidator, AreaAcademicaValidator>();
builder.Services.AddScoped<IAreaAcademicaService, AreaAcademicaService>();

builder.Services.AddScoped<IEntidadAcademicaRepository, EntidadAcademicaRepository>();
builder.Services.AddScoped<IEntidadAcademicaValidator, EntidadAcademicaValidator>();
builder.Services.AddScoped<IEntidadAcademicaService, EntidadAcademicaService>();

builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<IArticuloValidator, ArticuloValidator>();

builder.Services.AddScoped<IProgramaEducativoRepository, ProgramaEducativoRepository>();
builder.Services.AddScoped<IProgramaEducativoService,  ProgramaEducativoService>();
builder.Services.AddScoped<IProgramaEducativoValidator, ProgramaEducativoValidator>();
builder.Services.AddScoped<IPlanEstudiosRepository, PlanEstudiosRepository>();
builder.Services.AddScoped<IExperienciaEducativaRepository, ExperienciaEducativaRepository>();
builder.Services.AddScoped<IPlanEstudiosValidator, PlanEstudiosValidator>();
builder.Services.AddScoped<IPlanEstudiosService, PlanEstudiosService>();

builder.Services.AddScoped<IPeriodoEscolarRepository, PeriodoEscolarRepository>();
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
        options.LoginPath = "/Login";
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
    });

builder.Services.AddAuthorization();
builder.Services.AddSolicitudesAperturaModule();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILdapAuthService, LdapAuthService>();

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
        if (context.Request.Path.StartsWithSegments("/api/v1/solicitudes-apertura"))
        {
            var correo = app.Configuration["DevelopmentAuthentication:Email"];
            var rol = app.Configuration["DevelopmentAuthentication:Role"]
                ?? Constantes.COORDINADOR_EA;

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

app.UseAuthorization();  // 4. ¿qué puedes hacer?

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

app.Run();
