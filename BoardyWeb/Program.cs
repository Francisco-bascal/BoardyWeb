using BoardyWeb.Autorizacion;
using BoardyWeb.Data;
using BoardyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PoliticasTablero.VerTablero, politica => politica.AddRequirements(
        new RolTableroRequirement(RolesTablero.Propietario, RolesTablero.Administrador, RolesTablero.Operario, RolesTablero.Lector)));

    options.AddPolicy(PoliticasTablero.ColaborarEnTablero, politica => politica.AddRequirements(
        new RolTableroRequirement(RolesTablero.Propietario, RolesTablero.Administrador, RolesTablero.Operario)));

    options.AddPolicy(PoliticasTablero.AdministrarTablero, politica => politica.AddRequirements(
        new RolTableroRequirement(RolesTablero.Propietario, RolesTablero.Administrador)));

    options.AddPolicy(PoliticasTablero.GestionarTablero, politica => politica.AddRequirements(
        new RolTableroRequirement(RolesTablero.Propietario)));
});

builder.Services.AddScoped<IAuthorizationHandler, RolTableroHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();