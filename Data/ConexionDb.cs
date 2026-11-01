using MySqlConnector;
namespace PRODUCTOSAPI.Data;

public class ConexionDb : IConexionDb
{
    private readonly string _cadenaConexion;

    // La cadena de conexión se lee de appsettings.json → "ConnectionStrings:MySql".
    public ConexionDb(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'MySql' en appsettings.json");
    }

    // Devuelve una conexión NUEVA y CERRADA. Quien la usa la abre con Open()
    // y la cierra al terminar (con `using`). ADO.NET reutiliza las conexiones
    // por debajo (connection pooling), así que crear una por consulta es lo normal.
    public MySqlConnection CrearConexion()
    {
        return new MySqlConnection(_cadenaConexion);
    }

    // Abre la conexión y pregunta la versión del servidor para comprobar que todo funciona.
    public string ProbarConexion()
    {
        using var conexion = CrearConexion();
        conexion.Open();

        using var comando = new MySqlCommand("SELECT VERSION();", conexion);
        return comando.ExecuteScalar()?.ToString() ?? "desconocida";
    }
}
