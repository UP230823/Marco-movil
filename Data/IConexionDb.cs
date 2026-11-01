using MySqlConnector;
namespace PRODUCTOSAPI.Data;

// Punto 1: CONEXIÓN.
// Quien necesite hablar con la base de datos pide una conexión aquí.
public interface IConexionDb
{
    MySqlConnection CrearConexion();
    string ProbarConexion();
}
