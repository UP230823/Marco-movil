using PRODUCTOSAPI.Data;
using PRODUCTOSAPI.Endpoints;
using PRODUCTOSAPI.Repositories;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Conexión: una sola instancia que solo guarda la cadena de conexión.
builder.Services.AddSingleton<IConexionDb, ConexionDb>();

// Repositorio: Scoped = una instancia nueva por cada petición HTTP.
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<IProductoRepository>(sp => sp.GetRequiredService<ProductoRepository>());
builder.Services.AddScoped<IProductoBusqueda>(sp => sp.GetRequiredService<ProductoRepository>());

var app = builder.Build();

// Crea la base de datos y la tabla si no existen (y mete datos de ejemplo).
try
{
    InicializadorDb.Inicializar(app.Configuration);
}
catch (Exception ex)
{
    app.Logger.LogError("No se pudo preparar la base de datos: {Mensaje}. " +
        "Revisa que MySQL esté encendido y la cadena de conexión en appsettings.json.", ex.Message);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ProductosAPI v1");
    });
}

// Welcome message to check that the API is running
app.MapGet("/", () => Results.Ok(new { mensaje = "ProductosAPI funcionando. Documentación en /swagger" }))
    .ExcludeFromDescription();

// Map the Conexion endpoints
app.MapConexionEndpoints();

// Map the Producto endpoints
app.MapProductoEndpoints();

app.Run();
