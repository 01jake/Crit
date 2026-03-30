using Crit.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<HttpClient>(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

builder.Services.AddScoped<QuejaService>();
builder.Services.AddScoped<ClienteHttpService>();
builder.Services.AddScoped<VentaHttpService>();
builder.Services.AddScoped<ProductoHttpService>();
builder.Services.AddScoped<PdfHttpService>();
builder.Services.AddScoped<CompraHttpService>();
builder.Services.AddScoped<CuentaPorCobrarHttpService>();
builder.Services.AddScoped<CuentaPorPagarHttpService>();
//builder.Services.AddScoped<KardexHttpService>(); 
builder.Services.AddScoped<CajaHttpService>();
builder.Services.AddScoped<GastoHttpService>();

builder.Services.AddScoped<Dashboardhttpservice>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<QuejaPublicaService>();
builder.Services.AddScoped<ArticuloService>();
builder.Services.AddScoped<ProveedorHttpService>();

await builder.Build().RunAsync();
