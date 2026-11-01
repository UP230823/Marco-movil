using MySqlConnector;
using PRODUCTOSAPI.Data;
using PRODUCTOSAPI.Models;
namespace PRODUCTOSAPI.Repositories;

// Aquí vive TODO el SQL del proyecto. Cada método sigue los mismos pasos de ADO.NET:
//   1. Pedir una conexión            → _conexion.CrearConexion()
//   2. Abrirla                        → conexion.Open()
//   3. Preparar el comando SQL        → new MySqlCommand(sql, conexion)
//   4. Pasar los valores como PARÁMETROS (@id, @nombre...) → evita inyección SQL
//   5. Ejecutar:
//        ExecuteReader()    → cuando el SQL devuelve renglones (SELECT)
//        ExecuteNonQuery()  → cuando solo modifica (INSERT, UPDATE, DELETE)
//        ExecuteScalar()    → cuando devuelve un solo valor
//   6. `using` cierra la conexión automáticamente al salir del método.
public class ProductoRepository : IProductoRepository, IProductoBusqueda
{
    private const string ColumnasSelect =
        "id, nombre, descripcion, categoria, precio, stock, activo, fecha_registro";

    private readonly IConexionDb _conexion;

    public ProductoRepository(IConexionDb conexion)
    {
        _conexion = conexion;
    }

    // Punto 2: CONSULTAS — todos los productos.
    public List<Producto> ObtenerTodos()
    {
        var productos = new List<Producto>();

        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        using var comando = new MySqlCommand(
            $"SELECT {ColumnasSelect} FROM productos ORDER BY id;", conexion);

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(Mapear(lector));
        }

        return productos;
    }

    // Punto 2: CONSULTAS — un producto por su Id. Devuelve null si no existe.
    public Producto? ObtenerPorId(int id)
    {
        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        using var comando = new MySqlCommand(
            $"SELECT {ColumnasSelect} FROM productos WHERE id = @id;", conexion);
        comando.Parameters.AddWithValue("@id", id);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    // Punto 3: INSERT. Devuelve el Id que MySQL generó (AUTO_INCREMENT).
    public int Insertar(Producto producto)
    {
        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        const string sql = """
            INSERT INTO productos (nombre, descripcion, categoria, precio, stock, activo)
            VALUES (@nombre, @descripcion, @categoria, @precio, @stock, @activo);
            """;

        using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, producto);
        comando.ExecuteNonQuery();

        // Equivale a hacer SELECT LAST_INSERT_ID();
        return (int)comando.LastInsertedId;
    }

    // Punto 4: ACTUALIZAR. true si se encontró y modificó el renglón.
    public bool Actualizar(Producto producto)
    {
        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        const string sql = """
            UPDATE productos
            SET nombre = @nombre,
                descripcion = @descripcion,
                categoria = @categoria,
                precio = @precio,
                stock = @stock,
                activo = @activo
            WHERE id = @id;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        AgregarParametros(comando, producto);
        comando.Parameters.AddWithValue("@id", producto.Id);

        // ExecuteNonQuery devuelve cuántos renglones coincidieron con el WHERE.
        return comando.ExecuteNonQuery() > 0;
    }

    // Punto 5: ELIMINAR. true si existía y se borró.
    public bool Eliminar(int id)
    {
        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        using var comando = new MySqlCommand("DELETE FROM productos WHERE id = @id;", conexion);
        comando.Parameters.AddWithValue("@id", id);

        return comando.ExecuteNonQuery() > 0;
    }

    // Punto 6: BUSCAR por texto (en nombre o descripción) y/o por categoría.
    // Si un filtro viene vacío, simplemente no se aplica.
    public List<Producto> Buscar(string? termino, string? categoria)
    {
        var productos = new List<Producto>();

        using var conexion = _conexion.CrearConexion();
        conexion.Open();

        const string sql = """
            SELECT id, nombre, descripcion, categoria, precio, stock, activo, fecha_registro
            FROM productos
            WHERE (@termino = '' OR nombre LIKE CONCAT('%', @termino, '%')
                                 OR descripcion LIKE CONCAT('%', @termino, '%'))
              AND (@categoria = '' OR categoria = @categoria)
            ORDER BY nombre;
            """;

        using var comando = new MySqlCommand(sql, conexion);
        comando.Parameters.AddWithValue("@termino", termino?.Trim() ?? string.Empty);
        comando.Parameters.AddWithValue("@categoria", categoria?.Trim() ?? string.Empty);

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            productos.Add(Mapear(lector));
        }

        return productos;
    }

    // Convierte el renglón actual del lector en un objeto Producto.
    private static Producto Mapear(MySqlDataReader lector)
    {
        return new Producto
        {
            Id = lector.GetInt32("id"),
            Nombre = lector.GetString("nombre"),
            Descripcion = lector.GetString("descripcion"),
            Categoria = lector.GetString("categoria"),
            Precio = lector.GetDecimal("precio"),
            Stock = lector.GetInt32("stock"),
            Activo = lector.GetBoolean("activo"),
            FechaRegistro = lector.GetDateTime("fecha_registro")
        };
    }

    // Parámetros que comparten INSERT y UPDATE.
    private static void AgregarParametros(MySqlCommand comando, Producto producto)
    {
        comando.Parameters.AddWithValue("@nombre", producto.Nombre);
        comando.Parameters.AddWithValue("@descripcion", producto.Descripcion);
        comando.Parameters.AddWithValue("@categoria", producto.Categoria);
        comando.Parameters.AddWithValue("@precio", producto.Precio);
        comando.Parameters.AddWithValue("@stock", producto.Stock);
        comando.Parameters.AddWithValue("@activo", producto.Activo);
    }
}
