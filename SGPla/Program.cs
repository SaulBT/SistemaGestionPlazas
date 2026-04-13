using Microsoft.EntityFrameworkCore;
using SGPla.Data;
using SGPla.Repositories;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Services.Interfaces;
using SGPla.Validations.Implementations;
using SGPla.Validations.Interfaces;
//using SGPla.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//ConectionString
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? throw new InvalidOperationException("No se encontró la cadena 'DefaultConnection'.");

builder.Services.AddDbContext<GestionDePlazasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDbContext<GestionDePlazasDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<IArticuloValidator, ArticuloValidator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
