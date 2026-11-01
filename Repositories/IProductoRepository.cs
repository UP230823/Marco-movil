using PRODUCTOSAPI.Models;
namespace PRODUCTOSAPI.Repositories;

// Interface a implementar (tal cual la pide la actividad).
public interface IProductoRepository
{
    List<Producto> ObtenerTodos();
    Producto? ObtenerPorId(int id);
    int Insertar(Producto producto);
    bool Actualizar(Producto producto);
    bool Eliminar(int id);
}
