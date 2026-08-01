-- ============================================================
-- CoreSys HM - Script 02: Creación de Tablas
-- ============================================================

USE CoreSysHM;
GO

-- ==================== AUTH ====================

CREATE TABLE Usuarios (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Nombre      NVARCHAR(100) NOT NULL,
    Apellido    NVARCHAR(100) NOT NULL,
    Rol         NVARCHAR(50)  NOT NULL DEFAULT 'Operador',
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1,
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Administrador', 'Supervisor', 'Operador'))
);
GO

-- ==================== STOCK ====================

CREATE TABLE Categorias (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(300) NULL,
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1
);
GO

CREATE TABLE Productos (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Codigo      NVARCHAR(50)  NOT NULL UNIQUE,
    Nombre      NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    PrecioCompra DECIMAL(18,2) NOT NULL DEFAULT 0,
    PrecioVenta  DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockActual  INT          NOT NULL DEFAULT 0,
    StockMinimo  INT          NOT NULL DEFAULT 5,
    CategoriaId  INT          NOT NULL,
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    CONSTRAINT CK_Productos_PrecioCompra CHECK (PrecioCompra >= 0),
    CONSTRAINT CK_Productos_PrecioVenta  CHECK (PrecioVenta >= 0)
);
GO

CREATE TABLE MovimientosStock (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    ProductoId  INT          NOT NULL,
    Cantidad    INT          NOT NULL,
    TipoMovimiento NVARCHAR(10) NOT NULL,
    Observacion NVARCHAR(300) NULL,
    StockAnterior INT        NOT NULL,
    StockPosterior INT       NOT NULL,
    FechaCreacion DATETIME2  NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Movimientos_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id),
    CONSTRAINT CK_Movimientos_Tipo CHECK (TipoMovimiento IN ('ENTRADA', 'SALIDA'))
);
GO

-- ==================== VENTAS ====================

CREATE TABLE Clientes (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL,
    Apellido    NVARCHAR(100) NOT NULL,
    Dni         NVARCHAR(20)  NULL,
    Cuit        NVARCHAR(20)  NULL,
    Email       NVARCHAR(200) NULL,
    Telefono    NVARCHAR(30)  NULL,
    Direccion   NVARCHAR(300) NULL,
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1
);
GO

CREATE TABLE Ventas (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    NumeroVenta NVARCHAR(20)  NOT NULL UNIQUE,
    Fecha       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ClienteId   INT           NOT NULL,
    Subtotal    DECIMAL(18,2) NOT NULL DEFAULT 0,
    Descuento   DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total       DECIMAL(18,2) NOT NULL DEFAULT 0,
    Estado      NVARCHAR(20)  NOT NULL DEFAULT 'Pendiente',
    Observaciones NVARCHAR(500) NULL,
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    CONSTRAINT CK_Ventas_Estado CHECK (Estado IN ('Pendiente', 'Confirmada', 'Anulada'))
);
GO

CREATE TABLE DetallesVenta (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    VentaId     INT           NOT NULL,
    ProductoId  INT           NOT NULL,
    Cantidad    INT           NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal    DECIMAL(18,2) NOT NULL,
    FechaCreacion DATETIME2   NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo      BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Detalles_Ventas    FOREIGN KEY (VentaId)    REFERENCES Ventas(Id),
    CONSTRAINT FK_Detalles_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id),
    CONSTRAINT CK_Detalles_Cantidad  CHECK (Cantidad > 0)
);
GO

-- ==================== FACTURACIÓN ====================

CREATE TABLE Facturas (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    NumeroFactura NVARCHAR(20)  NOT NULL UNIQUE,
    FechaEmision  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    FechaVencimiento DATETIME2  NULL,
    ClienteId     INT           NOT NULL,
    VentaId       INT           NOT NULL UNIQUE,
    Subtotal      DECIMAL(18,2) NOT NULL DEFAULT 0,
    Iva           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total         DECIMAL(18,2) NOT NULL DEFAULT 0,
    Estado        NVARCHAR(20)  NOT NULL DEFAULT 'Emitida',
    Observaciones NVARCHAR(500) NULL,
    FechaCreacion DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2 NULL,
    Activo        BIT           NOT NULL DEFAULT 1,
    CONSTRAINT FK_Facturas_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    CONSTRAINT FK_Facturas_Ventas   FOREIGN KEY (VentaId)   REFERENCES Ventas(Id),
    CONSTRAINT CK_Facturas_Estado   CHECK (Estado IN ('Emitida', 'Pagada', 'Vencida', 'Anulada'))
);
GO

-- Índices para mejorar performance de consultas frecuentes
CREATE INDEX IX_Productos_CategoriaId    ON Productos(CategoriaId);
CREATE INDEX IX_Productos_StockActual    ON Productos(StockActual);
CREATE INDEX IX_Ventas_ClienteId         ON Ventas(ClienteId);
CREATE INDEX IX_Ventas_Fecha             ON Ventas(Fecha);
CREATE INDEX IX_Facturas_ClienteId       ON Facturas(ClienteId);
CREATE INDEX IX_Facturas_FechaVencimiento ON Facturas(FechaVencimiento);
GO

PRINT 'Tablas creadas exitosamente.';
GO
