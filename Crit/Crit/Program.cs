using Crit.Client.Pages;
using Crit.Client.Services;
using Crit.Components;
using Crit.Components.Account;
using Crit.Data;
using Crit.Server.Data;
using Crit.Server.Hubs;
using Crit.Server.Services;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();
//por mientras
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});
builder.Services.AddSignalR();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<QuejaService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<QuejaPublicaService>();
builder.Services.AddScoped<ArticuloService>();
builder.Services.AddScoped<VentaHttpService>();
builder.Services.AddScoped<ProductoHttpService>();
builder.Services.AddScoped<PdfHttpService>();

builder.Services.AddScoped<ClienteHttpService>();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar correctamente el AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies(options =>
{
    options.ApplicationCookie?.Configure(cookieOptions =>
    {
        cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(8);
        cookieOptions.SlidingExpiration = true;

        // IMPORTANTE: Configurar respuestas para APIs
        cookieOptions.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        cookieOptions.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
});
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Solo para desarrollo
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configuración de Identity
builder.Services.AddIdentityCore<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Configurar lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configurar CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sembrar la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();
app.UseStaticFiles();
//  Orden correcto de middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapHub<NotificationHub>("/notificationhub");
app.UseCors();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Crit.Client._Imports).Assembly)
    .AllowAnonymous();

app.MapAdditionalIdentityEndpoints();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Solo si no hay datos
        if (!context.Clientes.Any())
        {
            logger.LogInformation("Creando datos de prueba...");

            // Clientes
            var clientes = new List<Cliente>
            {
                new() { Nombre = "Juan Pérez", Email = "juan@test.com", Telefono = "555-1234", RFC = "PEPJ850101XXX", Activo = true, FechaRegistro = DateTime.Now },
                new() { Nombre = "María García", Email = "maria@test.com", Telefono = "555-5678", RFC = "GARM900202XXX", Activo = true, FechaRegistro = DateTime.Now },
                new() { Nombre = "Carlos López", Email = "carlos@test.com", Telefono = "555-9012", RFC = "LOPC880303XXX", Activo = true, FechaRegistro = DateTime.Now }
            };
            context.Clientes.AddRange(clientes);
            context.SaveChanges();

            // Productos
            var productos = new List<Producto>
            {
                new() { Codigo = "P001", Nombre = "Laptop", Descripcion = "Laptop Dell", PrecioCompra = 8000, PrecioVenta = 12000, Stock = 15, StockMinimo = 5, Categoria = "Electrónica", Unidad = "Pieza", Activo = true, FechaCreacion = DateTime.Now },
                new() { Codigo = "P002", Nombre = "Mouse", Descripcion = "Mouse Logitech", PrecioCompra = 200, PrecioVenta = 350, Stock = 50, StockMinimo = 10, Categoria = "Accesorios", Unidad = "Pieza", Activo = true, FechaCreacion = DateTime.Now },
                new() { Codigo = "P003", Nombre = "Teclado", Descripcion = "Teclado mecánico", PrecioCompra = 600, PrecioVenta = 950, Stock = 30, StockMinimo = 8, Categoria = "Accesorios", Unidad = "Pieza", Activo = true, FechaCreacion = DateTime.Now },
                new() { Codigo = "P004", Nombre = "Monitor", Descripcion = "Monitor 24\"", PrecioCompra = 2000, PrecioVenta = 3200, Stock = 20, StockMinimo = 5, Categoria = "Electrónica", Unidad = "Pieza", Activo = true, FechaCreacion = DateTime.Now }
            };
            context.Productos.AddRange(productos);
            context.SaveChanges();

            // Ventas de los últimos 6 meses
            var random = new Random();
            for (int mes = 5; mes >= 0; mes--)
            {
                for (int i = 0; i < 5; i++) // 5 ventas por mes
                {
                    var fecha = DateTime.Now.AddMonths(-mes).AddDays(-random.Next(0, 28));
                    var cliente = clientes[random.Next(clientes.Count)];
                    var producto = productos[random.Next(productos.Count)];

                    var venta = new Venta
                    {
                        NumeroVenta = $"V-{fecha:yyyyMMdd}-{(mes * 5 + i + 1):D6}",
                        ClienteId = cliente.Id,
                        Fecha = fecha,
                        Estado = "Completada",
                        Descuento = 0
                    };

                    var detalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = random.Next(1, 4),
                        PrecioUnitario = producto.PrecioVenta,
                        Descuento = 0
                    };
                    detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                    venta.Detalles = new List<DetalleVenta> { detalle };
                    venta.Subtotal = detalle.Subtotal;
                    venta.IVA = venta.Subtotal * 0.16m;
                    venta.Total = venta.Subtotal + venta.IVA;

                    context.Ventas.Add(venta);
                }
            }
            context.SaveChanges();

            logger.LogInformation("✅ Datos de prueba creados");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al crear datos de prueba");
    }
}
app.Run();

// Clase SeedData 
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Crear roles
        string[] roleNames = { "Administrador", "Empleado", "Usuario" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Crear usuario admin
        var adminEmail = "admin@crit.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        // Crear usuario empleado de ejemplo
        var empleadoEmail = "empleado@crit.com";
        var empleadoUser = await userManager.FindByEmailAsync(empleadoEmail);
        if (empleadoUser == null)
        {
            empleadoUser = new ApplicationUser
            {
                UserName = empleadoEmail,
                Email = empleadoEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(empleadoUser, "Empleado123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(empleadoUser, "Empleado");
            }
        }
    }
}