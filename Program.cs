using Bingoo.Models; // Asegúrate de que BingoContext está en la carpeta Models
using Microsoft.EntityFrameworkCore; // Para EF Core
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Para MySQL
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de controladores y vistas
builder.Services.AddControllersWithViews();

// Configurar la autenticación basada en cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });


builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

// Configurar el contexto de la base de datos con MySQL
builder.Services.AddDbContext<BingoContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 25))));

// **Agregar los servicios de SignalR**
builder.Services.AddSignalR();

var app = builder.Build();

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

// **Mapear el Hub de SignalR**
app.MapHub<BingoHub>("/bingohub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
