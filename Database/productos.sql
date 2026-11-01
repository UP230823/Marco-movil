-- ============================================================================
--  productos.sql  —  Crea la base de datos, la tabla y datos de ejemplo
-- ============================================================================
--  La API ya hace esto sola al arrancar (Data/InicializadorDb.cs).
--  Este archivo sirve si prefieres crearla a mano desde MySQL Workbench,
--  phpMyAdmin o la terminal:   mysql -u root -p < Database/productos.sql
-- ============================================================================

CREATE DATABASE IF NOT EXISTS productos_db
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE productos_db;

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

INSERT INTO productos (nombre, descripcion, categoria, precio, stock, activo) VALUES
('Teclado mecánico', 'Switches rojos, retroiluminado RGB', 'Periféricos', 899.00, 15, 1),
('Mouse inalámbrico', 'Sensor óptico 16000 DPI', 'Periféricos', 549.50, 30, 1),
('Monitor 27"', 'IPS 144 Hz, resolución 2K', 'Pantallas', 4599.00, 5, 1);
