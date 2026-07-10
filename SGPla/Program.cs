using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Repositories.Implementations;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Validations.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEstadoNavegacion, EstadoNavegacion>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
#if DEBUG
        options.LoginPath = "/DevLogin";
#else
        options.LoginPath = "/Account/Login";
#endif
        options.AccessDeniedPath = "/Account/AccesoDenegado";
    });

builder.Services.AddAuthorization(); 

builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddScoped<IAvisoRepository, AvisoRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();       // 1. primero rutea

app.UseSession();       // 2. sesión

app.UseAuthentication(); // 3. ¿quién eres?

app.UseAuthorization();  // 4. ¿qué puedes hacer?

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DevLogin}/{action=Index}/{id?}")
    .WithStaticAssets();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

app.Run();