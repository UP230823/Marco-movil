using PRODUCTOSAPI.Data;

namespace PRODUCTOSAPI.Endpoints;

// Endpoint para comprobar desde Swagger que la API sí llega a MySQL.
public static class ConexionEndpoints
{
    public static void MapConexionEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/conexion").WithTags("Conexión");

        grupo.MapGet("/", (IConexionDb conexion) =>
        {
            try
            {
                var version = conexion.ProbarConexion();
                return Results.Ok(new { conectado = true, servidor = version });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "No se pudo conectar a MySQL",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });
    }
}
