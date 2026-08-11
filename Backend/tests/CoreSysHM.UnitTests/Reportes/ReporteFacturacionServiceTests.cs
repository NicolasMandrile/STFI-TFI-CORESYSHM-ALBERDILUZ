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

namespace CoreSysHM.UnitTests.Reportes;

public class ReporteFacturacionServiceTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public ReporteFacturacionServiceTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task FacturacionPorPeriodo_CoincideExactoConLaSumaManualDeFacturas()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var facturaSvc = scope.ServiceProvider.GetRequiredService<IFacturaService>();
        var reporteSvc = scope.ServiceProvider.GetRequiredService<IReporteFacturacionService>();

        // Dos ventas confirmadas independientes, cada una facturada completa.
        var categoria = new Categoria { Nombre = "Cat", Descripcion = "x" };
        var producto = new Producto { Codigo = "P1", Nombre = "Prod", PrecioCompra = 10, PrecioVenta = 50, StockActual = 100, StockMinimo = 1, Categoria = categoria };
        var cliente = new Cliente { Nombre = "C", Apellido = "D" };
        var tipoComprobante = new TipoComprobante { Descripcion = "Factura A", AfectaStock = true, SignoContable = "+" };
        var puntoVenta = new PuntoVenta { Descripcion = "0001" };

        var venta1 = new Venta { NumeroVenta = "V1", ClienteId = 0, Cliente = cliente, Estado = EstadoVenta.Confirmada, Subtotal = 500, Total = 500 };
        venta1.Detalles.Add(new DetalleVenta { Producto = producto, Cantidad = 10, PrecioUnitario = 50, Subtotal = 500 });
        var venta2 = new Venta { NumeroVenta = "V2", ClienteId = 0, Cliente = cliente, Estado = EstadoVenta.Confirmada, Subtotal = 250, Total = 250 };
        venta2.Detalles.Add(new DetalleVenta { Producto = producto, Cantidad = 5, PrecioUnitario = 50, Subtotal = 250 });

        db.AddRange(categoria, producto, cliente, tipoComprobante, puntoVenta, venta1, venta2);
        await db.SaveChangesAsync();
        db.NumeracionesComprobante.Add(new NumeracionComprobante { PuntoVentaId = puntoVenta.Id, TipoComprobanteId = tipoComprobante.Id, UltimoNumero = 0 });
        await db.SaveChangesAsync();

        await facturaSvc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = venta1.Id, TipoComprobanteId = tipoComprobante.Id, PuntoVentaId = puntoVenta.Id, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = venta1.Detalles.First().Id, Cantidad = 10, Impuesto = 21 } }
        }, null);
        await facturaSvc.EmitirFacturaAsync(new CreateFacturaDto
        {
            VentaId = venta2.Id, TipoComprobanteId = tipoComprobante.Id, PuntoVentaId = puntoVenta.Id, IdempotencyKey = Guid.NewGuid().ToString(),
            Detalles = new() { new CreateDetalleFacturaDto { DetalleVentaId = venta2.Detalles.First().Id, Cantidad = 5, Impuesto = 21 } }
        }, null);

        // Fuente de verdad independiente: suma manual directa sobre la tabla Facturas.
        var facturasGuardadas = await db.Facturas.Where(f => f.Estado != EstadoFactura.Anulada).ToListAsync();
        var totalEsperado = facturasGuardadas.Sum(f => f.Total);
        var cantidadEsperada = facturasGuardadas.Count;

        var desde = DateTime.UtcNow.AddDays(-1);
        var hasta = DateTime.UtcNow.AddDays(1);
        var reporte = await reporteSvc.FacturacionPorPeriodoAsync(desde, hasta, "dia", null, null);

        reporte.Exitoso.Should().BeTrue();
        reporte.Data!.Sum(r => r.CantidadComprobantes).Should().Be(cantidadEsperada);
        reporte.Data!.Sum(r => r.Total).Should().Be(totalEsperado);
    }

    [Fact]
    public async Task CarteraPorFacturar_UsaLaMismaFuenteQueElSaldoDeFacturaService()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var facturaSvc = scope.ServiceProvider.GetRequiredService<IFacturaService>();
        var reporteSvc = scope.ServiceProvider.GetRequiredService<IReporteFacturacionService>();

        var categoria = new Categoria { Nombre = "Cat2", Descripcion = "x" };
        var producto = new Producto { Codigo = "P2", Nombre = "Prod2", PrecioCompra = 10, PrecioVenta = 30, StockActual = 50, StockMinimo = 1, Categoria = categoria };
        var cliente = new Cliente { Nombre = "E", Apellido = "F" };
        var venta = new Venta { NumeroVenta = "V3", ClienteId = 0, Cliente = cliente, Estado = EstadoVenta.Confirmada, Subtotal = 90, Total = 90 };
        venta.Detalles.Add(new DetalleVenta { Producto = producto, Cantidad = 3, PrecioUnitario = 30, Subtotal = 90 });

        db.AddRange(categoria, producto, cliente, venta);
        await db.SaveChangesAsync();

        var cartera = await reporteSvc.CarteraPorFacturarAsync(null);
        var saldoDirecto = await facturaSvc.GetSaldoFacturarAsync(venta.Id);

        cartera.Data!.Should().Contain(v => v.VentaId == venta.Id);
        cartera.Data!.First(v => v.VentaId == venta.Id).Lineas.Single().CantidadPendiente
            .Should().Be(saldoDirecto.Data!.Lineas.Single().CantidadPendiente);
    }
}
