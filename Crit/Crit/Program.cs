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

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Solo para desarrollo
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<QuejaService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<CompraHttpService>();
builder.Services.AddScoped<CuentaPorCobrarHttpService>();
builder.Services.AddScoped<CuentaPorPagarHttpService>();
builder.Services.AddScoped<KardexHttpService>();
builder.Services.AddScoped<ReabastecimientoHttpService>();
builder.Services.AddScoped<TraspasoHttpService>();
builder.Services.AddScoped<InventarioAlmacenHttpService>();
builder.Services.AddScoped<CajaHttpService>();
builder.Services.AddScoped<GastoHttpService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UsuariosHttpService>();

builder.Services.AddScoped<IEmpresaProvider, EmpresaProvider>();
builder.Services.AddScoped<SesionHttpService>();
builder.Services.AddScoped<AlmacenHttpService>();
builder.Services.AddScoped<QuejaPublicaService>();
builder.Services.AddScoped<ArticuloService>();
builder.Services.AddScoped<Dashboardhttpservice>();
builder.Services.AddScoped<ProveedorHttpService>();
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

// ===== SEED DE USUARIOS Y ROLES =====
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
        logger.LogError(ex, "Error al sembrar usuarios y roles");
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

// ===== SEED DE VENTAS DE PRUEBA =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🔍 Verificando ventas de prueba...");

        var hayVentas = await context.Ventas.AnyAsync();

        if (!hayVentas)
        {
            logger.LogInformation("📊 === CREANDO VENTAS DE PRUEBA ===");

            var clientes = await context.Clientes.Where(c => c.Activo).ToListAsync();
            var productos = await context.Productos.Where(p => p.Activo).ToListAsync();

            if (!clientes.Any())
            {
                logger.LogWarning("⚠️ No hay clientes activos. Crea clientes primero.");
            }
            else if (!productos.Any())
            {
                logger.LogWarning("⚠️ No hay productos activos. Crea productos primero.");
            }
            else
            {
                logger.LogInformation($"✅ Usando {clientes.Count} clientes y {productos.Count} productos");

                var random = new Random();
                var ventasCreadas = 0;

                // Crear ventas para los últimos 6 meses
                for (int mes = 5; mes >= 0; mes--)
                {
                    int numVentas = random.Next(4, 7); // 4-6 ventas por mes

                    for (int i = 0; i < numVentas; i++)
                    {
                        var fechaVenta = DateTime.Now.AddMonths(-mes).AddDays(-random.Next(0, 28));
                        var cliente = clientes[random.Next(clientes.Count)];

                        var venta = new Venta
                        {
                            NumeroVenta = $"V-{fechaVenta:yyyyMMdd}-{(ventasCreadas + 1):D6}",
                            ClienteId = cliente.Id,
                            Fecha = fechaVenta,
                            Estado = "Completada",
                            Descuento = 0,
                            Notas = $"Venta de prueba #{ventasCreadas + 1}",
                            Detalles = new List<DetalleVenta>()
                        };

                        // Agregar 1-3 productos aleatorios
                        int numProductos = random.Next(1, 4);
                        decimal subtotalVenta = 0;

                        for (int j = 0; j < numProductos; j++)
                        {
                            var producto = productos[random.Next(productos.Count)];
                            var cantidad = random.Next(1, 3);
                            var subtotal = producto.PrecioVenta * cantidad;

                            var detalle = new DetalleVenta
                            {
                                ProductoId = producto.Id,
                                Cantidad = cantidad,
                                PrecioUnitario = producto.PrecioVenta,
                                Descuento = 0,
                                Subtotal = subtotal
                            };

                            venta.Detalles.Add(detalle);
                            subtotalVenta += subtotal;
                        }

                        venta.Subtotal = subtotalVenta;
                        venta.IVA = venta.Subtotal * 0.16m;
                        venta.Total = venta.Subtotal + venta.IVA - venta.Descuento;

                        context.Ventas.Add(venta);
                        ventasCreadas++;
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation($"✅ {ventasCreadas} ventas creadas exitosamente");
            }
        }
        else
        {
            var totalVentas = await context.Ventas.CountAsync();
            logger.LogInformation($"ℹ️ Ya existen {totalVentas} ventas en la base de datos");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ ERROR al crear ventas: {Message}", ex.Message);
        if (ex.InnerException != null)
        {
            logger.LogError("💥 Inner exception: {InnerMessage}", ex.InnerException.Message);
        }
    }
}
await SeedData.Initialize(app.Services);


app.Run();

// ===== CLASE SEED DATA =====
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //await context.Database.MigrateAsync();

        // Crear roles
        string[] roleNames = { "Admin", "Supervisor", "Usuario" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Crear empresa principal
        var empresaPrincipal = await context.Empresas
            .FirstOrDefaultAsync(x => x.Nombre == "Empresa Principal");

        if (empresaPrincipal == null)
        {
            empresaPrincipal = new Empresa
            {
                Nombre = "Empresa Principal",
                RFC = "XAXX010101000",
                Activa = true,
                FechaRegistro = DateTime.Now
            };

            context.Empresas.Add(empresaPrincipal);
            await context.SaveChangesAsync();
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
                EmailConfirmed = true,
                NombreCompleto = "Administrador General",
                EmpresaId = empresaPrincipal.Id
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (adminUser.EmpresaId == null)
        {
            adminUser.EmpresaId = empresaPrincipal.Id;
            await userManager.UpdateAsync(adminUser);

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await RemoveLegacyRoles(userManager, adminUser);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Crear usuario supervisor
        var supervisorEmail = "supervisor@crit.com";
        var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);

        if (supervisorUser == null)
        {
            supervisorUser = new ApplicationUser
            {
                UserName = supervisorEmail,
                Email = supervisorEmail,
                EmailConfirmed = true,
                NombreCompleto = "Supervisor General",
                EmpresaId = empresaPrincipal.Id
            };

            var result = await userManager.CreateAsync(supervisorUser, "Supervisor123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supervisorUser, "Supervisor");
            }
        }
        else if (supervisorUser.EmpresaId == null)
        {
            supervisorUser.EmpresaId = empresaPrincipal.Id;
            await userManager.UpdateAsync(supervisorUser);

            if (!await userManager.IsInRoleAsync(supervisorUser, "Supervisor"))
            {
                await RemoveLegacyRoles(userManager, supervisorUser);
                await userManager.AddToRoleAsync(supervisorUser, "Supervisor");
            }
        }

        // Crear usuario operativo
        var usuarioEmail = "usuario@crit.com";
        var usuario = await userManager.FindByEmailAsync(usuarioEmail);

        if (usuario == null)
        {
            usuario = new ApplicationUser
            {
                UserName = usuarioEmail,
                Email = usuarioEmail,
                EmailConfirmed = true,
                NombreCompleto = "Usuario Operativo",
                EmpresaId = empresaPrincipal.Id
            };

            var result = await userManager.CreateAsync(usuario, "Usuario123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(usuario, "Usuario");
            }
        }
        else if (usuario.EmpresaId == null)
        {
            usuario.EmpresaId = empresaPrincipal.Id;
            await userManager.UpdateAsync(usuario);

            if (!await userManager.IsInRoleAsync(usuario, "Usuario"))
            {
                await RemoveLegacyRoles(userManager, usuario);
                await userManager.AddToRoleAsync(usuario, "Usuario");
            }
        }
    }

    private static async Task RemoveLegacyRoles(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var legacyRoles = new[] { "Administrador", "Empleado" };

        foreach (var role in legacyRoles)
        {
            if (await userManager.IsInRoleAsync(user, role))
            {
                await userManager.RemoveFromRoleAsync(user, role);
            }
        }
    }
}