using PRODUCTOSAPI.Models;
using PRODUCTOSAPI.Repositories;

namespace PRODUCTOSAPI.Endpoints;

// Las rutas HTTP. Cada endpoint recibe la petición, llama al repositorio
// y traduce el resultado a una respuesta HTTP (200, 201, 204, 400, 404).
public static class ProductosEndpoints
{
    public static void MapProductoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/productos").WithTags("Productos");

        // GET /productos → lista completa
        grupo.MapGet("/", (IProductoRepository repositorio) =>
            Results.Ok(repositorio.ObtenerTodos()));

        // GET /productos/buscar?termino=mouse&categoria=Periféricos
        // (va antes de "/{id}" para que "buscar" no se confunda con un Id)
        grupo.MapGet("/buscar", (string? termino, string? categoria, IProductoBusqueda busqueda) =>
            Results.Ok(busqueda.Buscar(termino, categoria)));

        // GET /productos/5 → un producto
        grupo.MapGet("/{id:int}", (int id, IProductoRepository repositorio) =>
        {
            var producto = repositorio.ObtenerPorId(id);
            return producto is null
                ? Results.NotFound(new { mensaje = "Producto no encontrado" })
                : Results.Ok(producto);
        });

        // POST /productos → crear
        grupo.MapPost("/", (Producto nuevo, IProductoRepository repositorio) =>
        {
            var errores = Validar(nuevo);
            if (errores.Count > 0) return Results.ValidationProblem(errores);

            var id = repositorio.Insertar(nuevo);
            var creado = repositorio.ObtenerPorId(id);
            return Results.Created($"/productos/{id}", creado);
        });

        // PUT /productos/5 → reemplazar los datos
        grupo.MapPut("/{id:int}", (int id, Producto producto, IProductoRepository repositorio) =>
        {
            var errores = Validar(producto);
            if (errores.Count > 0) return Results.ValidationProblem(errores);

            // El Id manda la URL, no el cuerpo JSON.
            producto.Id = id;
            return repositorio.Actualizar(producto)
                ? Results.NoContent()
                : Results.NotFound(new { mensaje = "Producto no encontrado" });
        });

        // DELETE /productos/5 → borrar
        grupo.MapDelete("/{id:int}", (int id, IProductoRepository repositorio) =>
            repositorio.Eliminar(id)
                ? Results.NoContent()
                : Results.NotFound(new { mensaje = "Producto no encontrado" }));
    }

    // Reglas mínimas antes de tocar la base de datos.
    private static Dictionary<string, string[]> Validar(Producto producto)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(producto.Nombre))
            errores["nombre"] = ["El nombre es obligatorio."];
        else if (producto.Nombre.Length > 100)
            errores["nombre"] = ["El nombre no puede pasar de 100 caracteres."];

        if (producto.Precio < 0)
            errores["precio"] = ["El precio no puede ser negativo."];

        if (producto.Stock < 0)
            errores["stock"] = ["El stock no puede ser negativo."];

        return errores;
    }
}
