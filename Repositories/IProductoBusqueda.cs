using PRODUCTOSAPI.Models;
namespace PRODUCTOSAPI.Repositories;

// Punto extra "Buscar". Se deja en su propia interface para no modificar
// IProductoRepository, que debe quedar exactamente como la pide la actividad.
public interface IProductoBusqueda
{
    List<Producto> Buscar(string? termino, string? categoria);
}
