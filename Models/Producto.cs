namespace PRODUCTOSAPI.Models;

// Representa un renglón de la tabla `productos` en MySQL.
// Cada propiedad corresponde a una columna de la tabla.
public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; } = true;

    // La pone la base de datos al insertar (DEFAULT CURRENT_TIMESTAMP).
    public DateTime FechaRegistro { get; set; }
}
