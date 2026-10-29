using BoardyWeb.Autorizacion;
using BoardyWeb.Comun;
using BoardyWeb.Data;
using BoardyWeb.Models;
using BoardyWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelMetadataDetailsProviders.Add(new TextoVacioProvider());
    options.ModelMetadataDetailsProviders.Add(new MensajesValidacionEsProvider());
    MensajesBindingEs.Aplicar(options.ModelBindingMessageProvider);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<Usuario>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddErrorDescriber<DescriptorErroresIdentity>()
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

    options.AddPolicy(PoliticasTarea.TrabajarEnTarea, politica => politica.AddRequirements(
        new TrabajoEnTareaRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, RolTableroHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TrabajoEnTareaHandler>();

builder.Services.AddScoped<IEstadoTareaService, EstadoTareaService>();
builder.Services.AddScoped<IRolMiembroService, RolMiembroService>();
builder.Services.AddScoped<ITableroService, TableroService>();
builder.Services.AddScoped<IMiembroTableroService, MiembroTableroService>();
builder.Services.AddScoped<IInvitacionService, InvitacionService>();
builder.Services.AddScoped<IListaTareaService, ListaTareaService>();
builder.Services.AddScoped<IEtiquetaService, EtiquetaService>();
builder.Services.AddScoped<ITareaService, TareaService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();

builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

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

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("es-AR")
    .AddSupportedCultures("es-AR")
    .AddSupportedUICultures("es-AR"));
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();