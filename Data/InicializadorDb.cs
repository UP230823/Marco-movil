using MySqlConnector;
namespace PRODUCTOSAPI.Data;

// Se ejecuta UNA vez al arrancar la API.
// Crea la base de datos y la tabla si todavía no existen, y mete 3 productos
// de ejemplo si la tabla está vacía. Así no hay que correr el script a mano.
// (El mismo SQL está en Database/productos.sql por si prefieres usar Workbench.)
public static class InicializadorDb
{
    public static void Inicializar(IConfiguration configuracion)
    {
        var cadena = configuracion.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'MySql'");

        // Separamos el nombre de la base de datos para poder crearla:
        // primero nos conectamos al servidor SIN base de datos.
        var constructor = new MySqlConnectionStringBuilder(cadena);
        var nombreBd = constructor.Database;
        constructor.Database = string.Empty;

        using (var conexion = new MySqlConnection(constructor.ConnectionString))
        {
            conexion.Open();
            using var crearBd = new MySqlCommand(
                $"CREATE DATABASE IF NOT EXISTS `{nombreBd}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
                conexion);
            crearBd.ExecuteNonQuery();
        }

        // Ahora sí, nos conectamos a la base de datos para crear la tabla.
        using var conexionBd = new MySqlConnection(cadena);
        conexionBd.Open();

        const string crearTabla = """
            CREATE TABLE IF NOT EXISTS productos (
                id             INT AUTO_INCREMENT PRIMARY KEY,
                nombre         VARCHAR(100)  NOT NULL,
                descripcion    VARCHAR(255)  NOT NULL DEFAULT '',
                categoria      VARCHAR(50)   NOT NULL DEFAULT '',
                precio         DECIMAL(10,2) NOT NULL DEFAULT 0,
                stock          INT           NOT NULL DEFAULT 0,
                activo         TINYINT(1)    NOT NULL DEFAULT 1,
                fecha_registro DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        using (var comando = new MySqlCommand(crearTabla, conexionBd))
        {
            comando.ExecuteNonQuery();
        }

        using var contar = new MySqlCommand("SELECT COUNT(*) FROM productos;", conexionBd);
        var total = Convert.ToInt32(contar.ExecuteScalar());
        if (total > 0) return;

        const string datosEjemplo = """
            INSERT INTO productos (nombre, descripcion, categoria, precio, stock, activo) VALUES
            ('Teclado mecánico', 'Switches rojos, retroiluminado RGB', 'Periféricos', 899.00, 15, 1),
            ('Mouse inalámbrico', 'Sensor óptico 16000 DPI', 'Periféricos', 549.50, 30, 1),
            ('Monitor 27"', 'IPS 144 Hz, resolución 2K', 'Pantallas', 4599.00, 5, 1);
            """;
        using var insertar = new MySqlCommand(datosEjemplo, conexionBd);
        insertar.ExecuteNonQuery();
    }
}
