# ProductosAPI

Backend para administrar productos (CRUD) con **.NET 10**, **Minimal APIs**,
**ADO.NET** y **MySQL**. Se prueba con **Swagger**.

Autor: Marco Isai Martinez Moreno — UPA, Ingeniería en Sistemas Computacionales, 8C.

---

## Puntos a resolver y dónde están

| Punto        | Archivo / método                                             | Endpoint                         |
| ------------ | ------------------------------------------------------------ | -------------------------------- |
| Conexión     | `Data/ConexionDb.cs` → `CrearConexion()`, `ProbarConexion()` | `GET /conexion`                  |
| Consultas    | `ProductoRepository.ObtenerTodos()` / `ObtenerPorId(id)`     | `GET /productos`, `GET /productos/{id}` |
| Insert       | `ProductoRepository.Insertar(producto)`                      | `POST /productos`                |
| Actualizar   | `ProductoRepository.Actualizar(producto)`                    | `PUT /productos/{id}`            |
| Eliminar     | `ProductoRepository.Eliminar(id)`                            | `DELETE /productos/{id}`         |
| Buscar       | `ProductoRepository.Buscar(termino, categoria)`              | `GET /productos/buscar?termino=&categoria=` |

La interface `IProductoRepository` está **exactamente** como la pide la
actividad. "Buscar" va en una interface aparte (`IProductoBusqueda`) para no
modificarla; la misma clase `ProductoRepository` implementa las dos.

### Respuestas HTTP

| Código | Cuándo                                              |
| ------ | --------------------------------------------------- |
| `200`  | Consulta o búsqueda correcta                        |
| `201`  | Producto creado (devuelve el producto con su `id`)  |
| `204`  | Actualizado o eliminado                             |
| `400`  | Datos inválidos (nombre vacío, precio o stock < 0)  |
| `404`  | El `id` no existe                                   |
| `503`  | `/conexion` no pudo llegar a MySQL                  |

---

## Estructura

```
ProductosAPI/
├── Program.cs                      ← Arranca la app, registra servicios y rutas
├── Models/
│   └── Producto.cs                 ← Qué datos tiene un producto (= columnas de la tabla)
├── Data/
│   ├── IConexionDb.cs / ConexionDb.cs  ← CONEXIÓN a MySQL (lee appsettings.json)
│   └── InicializadorDb.cs          ← Crea BD + tabla + datos de ejemplo al arrancar
├── Repositories/
│   ├── IProductoRepository.cs      ← Interface de la actividad
│   ├── IProductoBusqueda.cs        ← Interface extra para Buscar
│   └── ProductoRepository.cs       ← TODO el SQL (ADO.NET)
├── Endpoints/
│   ├── ProductosEndpoints.cs       ← Rutas /productos (GET, POST, PUT, DELETE, buscar)
│   └── ConexionEndpoints.cs        ← Ruta /conexion
├── Database/
│   └── productos.sql               ← Script SQL por si quieres crear la BD a mano
├── Properties/launchSettings.json  ← Puerto 5135 y abre /swagger
├── appsettings.json                ← Cadena de conexión a MySQL
├── ProductosAPI.http               ← Peticiones listas para REST Client
├── Dockerfile / compose.yaml       ← Levantar MySQL + API con Docker
└── ProductosAPI.csproj             ← .NET 10 + MySqlConnector + Swagger
```

---

## Cómo ejecutarla

### Opción A — Con MySQL instalado en tu PC (Workbench, XAMPP, etc.)

1. Abre `appsettings.json` y pon tu usuario y contraseña de MySQL:
   ```json
   "MySql": "Server=localhost;Port=3306;Database=productos_db;User ID=root;Password=TU_PASSWORD;"
   ```
   > XAMPP normalmente usa `User ID=root;Password=;` (vacío).
2. Ejecuta:
   ```bash
   dotnet run
   ```
3. Se abre **http://localhost:5135/swagger**. La base de datos `productos_db`
   y la tabla `productos` se crean solas la primera vez.

### Opción B — Todo con Docker

```bash
docker compose up --build
```

Levanta un contenedor con MySQL 8.4 y otro con la API. Abre
**http://localhost:5135/swagger**.

### Probar en Swagger

1. `GET /conexion` → debe decir `"conectado": true` y la versión de MySQL.
2. `GET /productos` → los 3 productos de ejemplo.
3. `POST /productos` → **Try it out**, pega un JSON y **Execute**.
4. `PUT`, `DELETE` y `GET /productos/buscar` igual.

---

## Flujo del proyecto

> Explicación general de cómo viaja una petición dentro de la API.

**1. Arranque (`Program.cs`).** Al correr `dotnet run`, `Program.cs` crea la
aplicación y registra en el contenedor de dependencias las piezas que se van a
usar: `ConexionDb` (que guarda la cadena de conexión leída de
`appsettings.json`) y `ProductoRepository` (que es donde está el SQL). Después
llama a `InicializadorDb`, que se conecta a MySQL y crea la base de datos y la
tabla si no existen. Por último activa Swagger y conecta las rutas con
`MapConexionEndpoints()` y `MapProductoEndpoints()`.

**2. Llega una petición (`Endpoints/`).** Cuando alguien entra, por ejemplo, a
`GET /productos/1`, ASP.NET busca qué ruta coincide y ejecuta esa función del
archivo `ProductosEndpoints.cs`. La función no sabe nada de SQL: solo pide el
repositorio por medio de la interface `IProductoRepository` (inyección de
dependencias) y llama al método que corresponde, en este caso
`ObtenerPorId(1)`. En `POST` y `PUT` primero se valida el JSON con `Validar()`;
si el nombre viene vacío o el precio/stock son negativos se responde `400` sin
tocar la base de datos.

**3. El repositorio habla con MySQL (`ProductoRepository.cs`).** Todos los
métodos siguen la misma receta de ADO.NET:

1. `CrearConexion()` da un `MySqlConnection` y se abre con `Open()`.
2. Se arma un `MySqlCommand` con el SQL (`SELECT`, `INSERT`, `UPDATE` o `DELETE`).
3. Los valores se mandan como **parámetros** (`@id`, `@nombre`…) con
   `Parameters.AddWithValue()`. Así nunca se pega texto del usuario dentro del
   SQL y se evita la inyección SQL.
4. Se ejecuta según lo que se espera de vuelta:
   - `ExecuteReader()` cuando regresa renglones (consultas y búsqueda). Se
     recorre con `Read()` y cada renglón se convierte en `Producto` con `Mapear()`.
   - `ExecuteNonQuery()` cuando solo modifica (insert, update, delete). Regresa
     cuántos renglones se afectaron; si es `0` significa que el `id` no existía,
     por eso `Actualizar` y `Eliminar` devuelven `false`.
   - `ExecuteScalar()` cuando regresa un solo valor (`SELECT VERSION()` en la prueba de conexión).
5. En `Insertar`, después del `INSERT` se lee `LastInsertedId` para saber qué
   `id` le asignó MySQL con `AUTO_INCREMENT`.
6. Como todo está dentro de `using`, la conexión se cierra sola al terminar.

**4. Regresa la respuesta.** El endpoint recibe lo que devolvió el repositorio
(una lista, un producto, `null`, un `id` o `true/false`) y lo convierte en una
respuesta HTTP: `200` con el JSON, `201` al crear, `204` al actualizar o
borrar, y `404` si no se encontró. ASP.NET convierte el objeto `Producto` a
JSON automáticamente.

En resumen: **Swagger → Endpoint → Repositorio (ADO.NET) → MySQL**, y la
respuesta regresa por el mismo camino.

---

## Requisitos

- .NET 10 SDK (`dotnet --version` → `10.0.x`)
- MySQL 8 (o MariaDB) **o** Docker Desktop
