-- ============================================================
-- CoreSys HM - Script 03: Stored Procedures
-- ============================================================

USE CoreSysHM;
GO

-- SP: Ajustar stock de producto
CREATE OR ALTER PROCEDURE sp_AjustarStock
    @ProductoId     INT,
    @Cantidad       INT,
    @TipoMovimiento NVARCHAR(10),
    @Observacion    NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @StockActual INT;
        SELECT @StockActual = StockActual FROM Productos WHERE Id = @ProductoId AND Activo = 1;

        IF @StockActual IS NULL
            THROW 50001, 'Producto no encontrado.', 1;

        IF @TipoMovimiento = 'SALIDA' AND @StockActual < @Cantidad
            THROW 50002, 'Stock insuficiente para realizar la operación.', 1;

        DECLARE @StockNuevo INT =
            CASE WHEN @TipoMovimiento = 'ENTRADA' THEN @StockActual + @Cantidad
                 ELSE @StockActual - @Cantidad END;

        UPDATE Productos SET StockActual = @StockNuevo, FechaModificacion = GETUTCDATE()
        WHERE Id = @ProductoId;

        INSERT INTO MovimientosStock (ProductoId, Cantidad, TipoMovimiento, Observacion, StockAnterior, StockPosterior)
        VALUES (@ProductoId, @Cantidad, @TipoMovimiento, @Observacion, @StockActual, @StockNuevo);

        COMMIT TRANSACTION;
        SELECT @StockNuevo AS StockNuevo;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Reporte de ventas por período
CREATE OR ALTER PROCEDURE sp_ReporteVentas
    @FechaDesde DATETIME2,
    @FechaHasta DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        v.NumeroVenta,
        v.Fecha,
        CONCAT(c.Nombre, ' ', c.Apellido) AS Cliente,
        v.Subtotal,
        v.Descuento,
        v.Total,
        v.Estado,
        COUNT(dv.Id) AS CantidadItems
    FROM Ventas v
    INNER JOIN Clientes c ON v.ClienteId = c.Id
    LEFT JOIN DetallesVenta dv ON v.Id = dv.VentaId
    WHERE v.Fecha BETWEEN @FechaDesde AND @FechaHasta
      AND v.Activo = 1
    GROUP BY v.Id, v.NumeroVenta, v.Fecha, c.Nombre, c.Apellido, v.Subtotal, v.Descuento, v.Total, v.Estado
    ORDER BY v.Fecha DESC;
END;
GO

-- SP: Productos con stock bajo
CREATE OR ALTER PROCEDURE sp_ProductosStockBajo
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        p.Id,
        p.Codigo,
        p.Nombre,
        cat.Nombre AS Categoria,
        p.StockActual,
        p.StockMinimo,
        (p.StockMinimo - p.StockActual) AS FaltaCantidad
    FROM Productos p
    INNER JOIN Categorias cat ON p.CategoriaId = cat.Id
    WHERE p.StockActual <= p.StockMinimo AND p.Activo = 1
    ORDER BY (p.StockMinimo - p.StockActual) DESC;
END;
GO

PRINT 'Stored Procedures creados exitosamente.';
GO
