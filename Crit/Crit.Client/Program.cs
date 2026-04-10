using Crit.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddHttpClient("CritAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("CritAPI"));

builder.Services.AddScoped<QuejaService>();
builder.Services.AddScoped<ClienteHttpService>();
builder.Services.AddScoped<VentaHttpService>();
builder.Services.AddScoped<ProductoHttpService>();
builder.Services.AddScoped<PdfHttpService>();
builder.Services.AddScoped<CompraHttpService>();
builder.Services.AddScoped<SesionHttpService>();
builder.Services.AddScoped<UsuariosHttpService>();
builder.Services.AddScoped<CuentaPorCobrarHttpService>();
builder.Services.AddScoped<CuentaPorPagarHttpService>();
builder.Services.AddScoped<TraspasoHttpService>();
builder.Services.AddScoped<KardexHttpService>();
builder.Services.AddScoped<ReabastecimientoHttpService>();
builder.Services.AddScoped<InventarioAlmacenHttpService>();
builder.Services.AddScoped<CajaHttpService>();
builder.Services.AddScoped<GastoHttpService>();
builder.Services.AddScoped<Dashboardhttpservice>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<QuejaPublicaService>();
builder.Services.AddScoped<ArticuloService>();
builder.Services.AddScoped<ProveedorHttpService>();
builder.Services.AddScoped<AlmacenHttpService>();

await builder.Build().RunAsync();
