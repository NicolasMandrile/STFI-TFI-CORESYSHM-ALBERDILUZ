using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.UnitTests.TestInfrastructure;
using Xunit;

namespace CoreSysHM.UnitTests.Facturacion;

public class FacturaServiceTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public FacturaServiceTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    /// <summary>Arma Categoria/Producto/Cliente/Venta confirmada (cantidad 10) + catálogos de Facturación.</summary>
    private static async Task<(int VentaId, int DetalleVentaId, int TipoComprobanteId, int PuntoVentaId, decimal PrecioUnitario)>
        SeedVentaConfirmadaAsync(ApplicationDbContext db, int cantidadVendida = 10)
    {
        var categoria = new Categoria { Nombre = "Cat-" + Guid.NewGuid().ToString("N")[..8], Descripcion = "x" };
        var producto = new Producto
        {
            Codigo = "P-" + Guid.NewGuid().ToString("N")[..8], Nombre = "Producto test",
            PrecioCompra = 100, PrecioVenta = 200, StockActual = 100, StockMinimo = 5,
            Categoria = categoria
        };
        var cliente = new Cliente { Nombre = "Cliente", Apellido = "Test-" + Guid.NewGuid().ToString("N")[..6] };
        var tipoComprobante = new TipoComprobante { Descripcion = "TC-" + Guid.NewGuid().ToString("N")[..8], AfectaStock = true, SignoContable = "+" };
        var puntoVenta = new PuntoVenta { Descripcion = "0001" };

        var venta = new Venta
        {
            NumeroVenta = "V-" + Guid.NewGuid().ToString("N")[..8],
            ClienteId = 0,
            Cliente = cliente,
            Estado = EstadoVenta.Confirmada,
            Subtotal = 200 * cantidadVendida,
            Total = 200 * cantidadVendida
        };
        var detalle = new DetalleVenta { Producto = producto, Cantidad = cantidadVendida, PrecioUnitario = 200, Subtotal = 200 * cantidadVendida };
        venta.Detalles.Add(detalle);

        db.AddRange(categoria, producto, cliente, tipoComprobante, puntoVenta, venta);
        await db.SaveChangesAsync();

        db.NumeracionesComprobante.Add(new NumeracionComprobante { PuntoVentaId = puntoVenta.Id, TipoComprobanteId = tipoComprobante.Id, UltimoNumero = 0 });
        await db.SaveChangesAsync();

        return (venta.Id, detalle.Id, tipoComprobante.Id, puntoVenta.Id, 200m);
    }

    [Fact]
    public async Task Emitir_ConDatosValidos_CalculaTotalesYNoTocaStockDeNuevo()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, precio) = await SeedVentaConfirmadaAsync(db);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var result = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId,
            TipoComprobanteId = tipoComprobanteId,
            PuntoVentaId = puntoVentaId,
            IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 4, Impuesto = 21, Descuento = 0 } }
        }, usuarioId: null);

        result.Exitoso.Should().BeTrue();
        result.Data!.Subtotal.Should().Be(800m);           // 4 * 200
        result.Data!.Iva.Should().Be(168m);                 // 800 * 21%
        result.Data!.Total.Should().Be(968m);
        result.Data!.NumeroFactura.Should().Be("0001-00000001");

        // El stock ya se descontó al confirmar la Venta (fuera de este test, en VentaService) --
        // la Factura A (signo "+") solo documenta fiscalmente, no vuelve a tocar stock.
        var producto = await db.Productos.FirstAsync(p => p.Id == result.Data!.Detalles[0].ProductoId);
        producto.StockActual.Should().Be(100);
    }

    [Fact]
    public async Task Emitir_DosVecesConMismaIdempotencyKey_NoDuplicaFactura()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();
        var key = Guid.NewGuid().ToString();
        var dto = new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = key,
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 2, Impuesto = 21 } }
        };

        var primera = await svc.EmitirFacturaAsync(dto, null);
        var segunda = await svc.EmitirFacturaAsync(dto, null);

        primera.Exitoso.Should().BeTrue();
        segunda.Exitoso.Should().BeTrue();
        segunda.Data!.Id.Should().Be(primera.Data!.Id);

        (await db.Facturas.CountAsync(f => f.IdempotencyKey == key)).Should().Be(1);
    }

    [Fact]
    public async Task Emitir_NumeracionEsCorrelativaPorPuntoVentaYTipo()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db, cantidadVendida: 20);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var f1 = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 5, Impuesto = 21 } }
        }, null);
        var f2 = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 5, Impuesto = 21 } }
        }, null);

        f1.Data!.NumeroFactura.Should().Be("0001-00000001");
        f2.Data!.NumeroFactura.Should().Be("0001-00000002");
    }

    [Fact]
    public async Task Emitir_FacturacionParcial_PermiteCubrirElRestoLuego()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db, cantidadVendida: 10);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var primera = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 6, Impuesto = 21 } }
        }, null);
        primera.Exitoso.Should().BeTrue();

        var saldo = await svc.GetSaldoFacturarAsync(ventaId);
        saldo.Data!.Lineas.Single().CantidadPendiente.Should().Be(4);

        var segunda = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 4, Impuesto = 21 } }
        }, null);
        segunda.Exitoso.Should().BeTrue();

        var saldoFinal = await svc.GetSaldoFacturarAsync(ventaId);
        saldoFinal.Data!.Lineas.Single().CantidadPendiente.Should().Be(0);
    }

    [Fact]
    public async Task Emitir_CantidadMayorAlSaldoPendiente_RetornaError()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db, cantidadVendida: 5);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var result = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 6, Impuesto = 21 } } // vendidas: 5
        }, null);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task Anular_FacturaNormal_NoTocaStock()
    {
        // La Factura A (signo "+") documenta una venta cuyo stock ya se descontó al confirmarse
        // -- ni emitirla ni anularla debe volver a tocar stock.
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, tipoComprobanteId, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db);
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var emitida = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = tipoComprobanteId, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 3, Impuesto = 21 } }
        }, null);
        var productoId = emitida.Data!.Detalles[0].ProductoId;
        (await db.Productos.FirstAsync(p => p.Id == productoId)).StockActual.Should().Be(100);

        var anulada = await svc.AnularAsync(emitida.Data!.Id, usuarioId: null);

        anulada.Exitoso.Should().BeTrue();
        (await db.Productos.FirstAsync(p => p.Id == productoId)).StockActual.Should().Be(100);

        var saldo = await svc.GetSaldoFacturarAsync(ventaId);
        saldo.Data!.Lineas.Single().CantidadPendiente.Should().Be(10); // vuelve a estar disponible para facturar
    }

    [Fact]
    public async Task Emitir_NotaDeCredito_ReponeStockYAnularLoVuelveADescontar()
    {
        // Nota de Crédito (signo "-"): a diferencia de la Factura, sí genera su propio movimiento
        // de stock -- repone unidades por una devolución que todavía no estaba registrada.
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (ventaId, detalleVentaId, _, puntoVentaId, _) = await SeedVentaConfirmadaAsync(db);
        var notaCredito = new TipoComprobante { Descripcion = "NC-" + Guid.NewGuid().ToString("N")[..8], AfectaStock = true, SignoContable = "-" };
        db.TiposComprobante.Add(notaCredito);
        await db.SaveChangesAsync();
        db.NumeracionesComprobante.Add(new NumeracionComprobante { PuntoVentaId = puntoVentaId, TipoComprobanteId = notaCredito.Id, UltimoNumero = 0 });
        await db.SaveChangesAsync();
        var svc = scope.ServiceProvider.GetRequiredService<IFacturaService>();

        var emitida = await svc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = ventaId, TipoComprobanteId = notaCredito.Id, PuntoVentaId = puntoVentaId, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = detalleVentaId, Cantidad = 3, Impuesto = 21 } }
        }, null);
        var productoId = emitida.Data!.Detalles[0].ProductoId;
        (await db.Productos.FirstAsync(p => p.Id == productoId)).StockActual.Should().Be(103); // 100 + 3 repuestos

        var anulada = await svc.AnularAsync(emitida.Data!.Id, usuarioId: null);

        anulada.Exitoso.Should().BeTrue();
        (await db.Productos.FirstAsync(p => p.Id == productoId)).StockActual.Should().Be(100); // se vuelve a descontar
    }
}
