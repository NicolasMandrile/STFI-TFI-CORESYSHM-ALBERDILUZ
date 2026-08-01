-- ============================================================
-- CoreSys HM - Script 04: Datos de Prueba (Seed)
-- ============================================================

USE CoreSysHM;
GO

-- Usuario Administrador (password: Admin123!)
-- Hash generado con BCrypt rounds=10
INSERT INTO Usuarios (NombreUsuario, Email, PasswordHash, Nombre, Apellido, Rol)
VALUES
    ('admin', 'admin@coresyshm.com', '$2a$10$PLACEHOLDER_HASH_ADMIN', 'Administrador', 'Sistema', 'Administrador'),
    ('supervisor1', 'supervisor@coresyshm.com', '$2a$10$PLACEHOLDER_HASH_SUPER', 'Juan', 'Pérez', 'Supervisor'),
    ('operador1', 'operador@coresyshm.com', '$2a$10$PLACEHOLDER_HASH_OPER', 'María', 'González', 'Operador');
GO

-- Categorías
INSERT INTO Categorias (Nombre, Descripcion)
VALUES
    ('Electrónica',     'Dispositivos y componentes electrónicos'),
    ('Herramientas',    'Herramientas de trabajo y mantenimiento'),
    ('Insumos',         'Materiales e insumos de oficina'),
    ('Indumentaria',    'Ropa y accesorios de trabajo'),
    ('Mobiliario',      'Muebles y equipamiento de oficina');
GO

-- Productos
INSERT INTO Productos (Codigo, Nombre, Descripcion, PrecioCompra, PrecioVenta, StockActual, StockMinimo, CategoriaId)
VALUES
    ('ELEC-001', 'Monitor 24" Full HD',    'Monitor LED 24 pulgadas 1920x1080',   45000, 65000, 15,  5, 1),
    ('ELEC-002', 'Teclado Inalámbrico',    'Teclado USB inalámbrico ergonómico',   3500,  5500,  30, 10, 1),
    ('ELEC-003', 'Mouse Óptico',            'Mouse óptico 1600 DPI',               2000,  3200,  25, 10, 1),
    ('HERR-001', 'Set Destornilladores',   'Set 8 piezas Philips y plano',         1800,  2900,   8,  5, 2),
    ('HERR-002', 'Llave Inglesa 12"',      'Llave ajustable 12 pulgadas',          2500,  4000,  12,  5, 2),
    ('INSU-001', 'Resma A4 500 hojas',     'Papel bond 75gr blanco',                800,  1200,  50, 20, 3),
    ('INSU-002', 'Cartuchos Tinta Negro',  'Cartucho compatible HP 664',           1500,  2400,   3, 10, 3),  -- Stock bajo
    ('INSU-003', 'Lapiceras Bic x12',      'Pack 12 unidades azul/negro',           600,   950,  40, 15, 3),
    ('INDU-001', 'Casco de Seguridad',     'Casco ABS clase E blanco',             1200,  1900,   2,  5, 4),  -- Stock bajo
    ('MOBI-001', 'Silla Ergonómica',       'Silla escritorio con apoyabrazos',    18000, 27000,   6,  3, 5);
GO

-- Clientes
INSERT INTO Clientes (Nombre, Apellido, Dni, Cuit, Email, Telefono, Direccion)
VALUES
    ('Carlos',  'Rodríguez', '28456789', '20-28456789-3', 'carlos.r@email.com', '011-4523-6789', 'Av. Corrientes 1234, CABA'),
    ('Laura',   'Martínez',  '32567890', '27-32567890-1', 'laura.m@email.com',  '011-4789-1234', 'Belgrano 567, Rosario'),
    ('Empresa', 'TechSol SA','',         '30-71234567-8', 'compras@techsol.com','0341-445-6789', 'Pellegrini 890, Rosario'),
    ('Miguel',  'López',     '25678901', '20-25678901-5', 'miguel.l@email.com', '0351-456-7890', 'San Martín 345, Córdoba');
GO

-- Movimientos de stock de ejemplo
EXEC sp_AjustarStock @ProductoId = 1, @Cantidad = 5, @TipoMovimiento = 'ENTRADA', @Observacion = 'Compra inicial';
EXEC sp_AjustarStock @ProductoId = 6, @Cantidad = 10, @TipoMovimiento = 'ENTRADA', @Observacion = 'Reposición mensual';
GO

PRINT 'Datos de prueba insertados exitosamente.';
GO

-- NOTA: Los hashes de contraseñas deben generarse desde la aplicación .NET
-- usando BCrypt. Los valores PLACEHOLDER deben reemplazarse ejecutando:
-- dotnet run --project CoreSysHM.API -- seed
-- o usando la endpoint POST /api/auth/seed (solo en Development)
