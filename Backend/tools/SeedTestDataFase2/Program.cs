using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Enums;

// Carga ADITIVA de datos de prueba (fase 2): a diferencia de SeedTestData (que borra y recrea
// todo desde cero), este script solo AGREGA registros nuevos sobre la base real ya existente,
// sin tocar ni un Id de lo que ya está cargado. Pensado para sumar volumen realista para pruebas
// funcionales (filtros, reportes, facturación parcial, historial) sin perder nada.
var rnd = new Random(20261027);

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=CoreSysHM_Dev;Trusted_Connection=True;TrustServerCertificate=True;");
using var context = new ApplicationDbContext(optionsBuilder.Options);

static string Slug(string s) => new string(s.Normalize(NormalizationForm.FormD)
    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
    .ToArray()).ToLowerInvariant();

static T Weighted<T>(Random rnd, params (T value, int weight)[] options)
{
    int total = options.Sum(o => o.weight);
    int r = rnd.Next(total);
    int acc = 0;
    foreach (var (value, weight) in options) { acc += weight; if (r < acc) return value; }
    return options[^1].value;
}

DateTime FechaAleatoria(Random rnd, DateTime desde, DateTime hasta) =>
    desde.AddDays(rnd.Next(0, Math.Max(1, (hasta - desde).Days + 1))).AddHours(rnd.Next(8, 19)).AddMinutes(rnd.Next(0, 60));

Console.WriteLine("Cargando estado actual de la base...");

var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@coresyshm.com");
var categorias = await context.Categorias.ToDictionaryAsync(c => c.Nombre);
var proveedoresExistentes = await context.Proveedores.Where(p => p.Activo).ToListAsync();
var productosExistentes = await context.Productos.Where(p => p.Activo).ToListAsync();
var clientesExistentes = await context.Clientes.Where(c => c.Activo).ToListAsync();
var condFiscal = await context.CondicionesFiscales.ToDictionaryAsync(c => c.Descripcion);
var tiposComprobante = await context.TiposComprobante.ToDictionaryAsync(t => t.Descripcion);
var puntoVenta = await context.PuntosVenta.FirstAsync();
var numeraciones = await context.NumeracionesComprobante.ToDictionaryAsync(n => n.TipoComprobanteId);
var estadosCompra = await context.EstadosCompra.ToDictionaryAsync(e => e.Descripcion);

var dniUsados = clientesExistentes.Where(c => c.Dni != null).Select(c => c.Dni!).ToHashSet();
var cuitClienteUsados = clientesExistentes.Where(c => c.Cuit != null).Select(c => c.Cuit!).ToHashSet();
var cuitProveedorUsados = proveedoresExistentes.Select(p => p.Cuit).ToHashSet();
var numeroVentaUsados = (await context.Ventas.Select(v => v.NumeroVenta).ToListAsync()).ToHashSet();
var numeroCompraUsados = (await context.Compras.Select(c => c.NumeroCompra).ToListAsync()).ToHashSet();

string NumeroVentaUnico(DateTime fecha)
{
    string numero;
    do { numero = $"V-{fecha:yyyyMMdd}-{rnd.Next(1000, 9999)}"; } while (!numeroVentaUsados.Add(numero));
    return numero;
}
string NumeroCompraUnico(DateTime fecha)
{
    string numero;
    do { numero = $"C-{fecha:yyyyMMdd}-{rnd.Next(1000, 9999)}"; } while (!numeroCompraUsados.Add(numero));
    return numero;
}
string AsignarNumeroFactura(TipoComprobante tipo)
{
    var contador = numeraciones[tipo.Id];
    contador.UltimoNumero++;
    return $"{puntoVenta.Descripcion.PadLeft(4, '0')}-{contador.UltimoNumero.ToString().PadLeft(8, '0')}";
}

var hoy = DateTime.UtcNow;
var inicioVentana = hoy.AddMonths(-2);

// ========== 1) Proveedores nuevos ==========
var proveedorNuevoDatos = new (string Razon, string Cuit, string Tel, string Mail, string Dir, string Contacto)[]
{
    ("Electroimport S.A.", "30-50111122-3", "0341-455-1111", "ventas@electroimport.com.ar", "Av. Alberdi 2800, Rosario", "Lautaro Ferreira"),
    ("Herramientas Record S.R.L.", "30-50122233-4", "0341-455-2222", "comercial@recordherramientas.com.ar", "Bv. 27 de Febrero 1450, Rosario", "Patricia Luján"),
};
var proveedoresNuevos = proveedorNuevoDatos
    .Where(p => !cuitProveedorUsados.Contains(p.Cuit))
    .Select(p => new Proveedor
    {
        RazonSocial = p.Razon, Cuit = p.Cuit, Telefono = p.Tel, Email = p.Mail, Direccion = p.Dir, Contacto = p.Contacto,
        CondicionFiscalId = condFiscal["Responsable Inscripto"].Id
    }).ToList();
context.Proveedores.AddRange(proveedoresNuevos);
var proveedores = proveedoresExistentes.Concat(proveedoresNuevos).ToList();
var provPorNombre = proveedores.ToDictionary(p => p.RazonSocial);

// ========== 2) Productos nuevos (completan categorías sin stock real) ==========
var productoNuevoSeeds = new (string Codigo, string Nombre, string Categoria, string Proveedor, decimal PcMin, decimal PcMax, decimal MkMin, decimal MkMax, int StMin, int StMax, int SmMin, int SmMax)[]
{
    ("AL-E01", "Extensión Eléctrica 3 Bocas x 5m",        "Electrónica",  "Electroimport S.A.",          4200, 5600, 1.5m, 1.8m, 30, 70, 8, 15),
    ("AL-E02", "Zapatilla Eléctrica 6 Bocas con Llave",   "Electrónica",  "Electroimport S.A.",          3800, 5000, 1.5m, 1.8m, 35, 80, 8, 18),
    ("AL-E03", "Cargador USB Pared 2 Puertos",             "Electrónica",  "Electroimport S.A.",          2200, 3000, 1.6m, 1.9m, 40, 90, 10, 20),
    ("AL-H01", "Destornillador Phillips N°2",              "Herramientas", "Herramientas Record S.R.L.",  1800, 2400, 1.6m, 1.9m, 40, 90, 10, 20),
    ("AL-H02", "Pinza Universal 8 Pulgadas",                "Herramientas", "Herramientas Record S.R.L.",  3200, 4200, 1.5m, 1.8m, 25, 60, 6, 15),
    ("AL-H03", "Martillo Carpintero 20oz",                  "Herramientas", "Herramientas Record S.R.L.",  5200, 6800, 1.4m, 1.7m, 20, 45, 5, 12),
    ("AL-I01", "Guantes de Látex x 100 un",                 "Insumos",      "Distribuidora Ferretera del Sur", 3600, 4600, 1.4m, 1.7m, 30, 70, 8, 18),
    ("AL-R01", "Repuesto Cilindro Candado Sekur",           "Repuestos",    "Sekur Argentina S.A.",        2800, 3600, 1.5m, 1.8m, 25, 55, 6, 15),
};
var productosNuevos = new List<Producto>();
var codigosExistentes = productosExistentes.Select(p => p.Codigo).ToHashSet();
foreach (var s in productoNuevoSeeds)
{
    if (codigosExistentes.Contains(s.Codigo)) continue;
    var precioCompra = Math.Round(s.PcMin + (decimal)rnd.NextDouble() * (s.PcMax - s.PcMin), 2);
    var markup = s.MkMin + (decimal)rnd.NextDouble() * (s.MkMax - s.MkMin);
    var precioVenta = Math.Round(precioCompra * markup, 2);
    var stock = rnd.Next(s.StMin, s.StMax + 1);
    var stockMinimo = rnd.Next(s.SmMin, s.SmMax + 1);
    productosNuevos.Add(new Producto
    {
        Codigo = s.Codigo, Nombre = s.Nombre,
        Descripcion = "Producto incorporado en el lote de datos de prueba (fase 2).",
        PrecioCompra = precioCompra, PrecioVenta = precioVenta,
        StockActual = stock, StockMinimo = stockMinimo,
        Categoria = categorias[s.Categoria], Proveedor = provPorNombre[s.Proveedor],
    });
}
context.Productos.AddRange(productosNuevos);
var productos = productosExistentes.Concat(productosNuevos).ToList();

// ========== 3) Clientes nuevos ==========
var nombresNuevos   = new[] { "Pablo", "Valeria", "Maximiliano", "Belén", "Facundo", "Candela", "Leandro", "Josefina", "Ramiro", "Abril",
    "Santiago", "Victoria", "Lucas", "Marina", "Esteban", "Carolina", "Gastón", "Natasha" };
var apellidosNuevos = new[] { "Bazán", "Castro", "Peralta", "Giménez", "Suárez", "Domínguez", "Carrizo", "Ledesma", "Quiroga", "Maidana",
    "Montenegro", "Sotelo", "Arce", "Juárez", "Zanetti", "Campos", "Figueroa", "Bravo" };
var localidades = new[] { "Rosario", "Funes", "Roldán", "Pérez", "Villa Gobernador Gálvez", "Granadero Baigorria",
    "Capitán Bermúdez", "San Lorenzo", "Fray Luis Beltrán", "Ibarlucea", "Soldini", "Zavalla" };
var calles = new[] { "San Martín", "Córdoba", "Mitre", "Pellegrini", "Corrientes", "Urquiza", "Sarmiento", "Moreno",
    "Rivadavia", "Entre Ríos", "Tucumán", "Santa Fe", "Buenos Aires", "España", "Italia" };

var clientesNuevos = new List<Cliente>();
for (int i = 0; i < nombresNuevos.Length; i++)
{
    var nombre = nombresNuevos[i];
    var apellido = apellidosNuevos[i];
    var dni = (27000000 + i * 47123).ToString();
    if (!dniUsados.Add(dni)) continue;
    var cuitPrefijo = i % 2 == 0 ? "20" : "27";
    var cuit = $"{cuitPrefijo}-{dni}-{(i % 9) + 1}";
    if (!cuitClienteUsados.Add(cuit)) continue;

    var condicionId = Weighted(rnd,
        (condFiscal["Consumidor Final"].Id, 60),
        (condFiscal["Responsable Inscripto"].Id, 25),
        (condFiscal["Monotributo"].Id, 10),
        (condFiscal["Exento"].Id, 5));

    clientesNuevos.Add(new Cliente
    {
        Nombre = nombre,
        Apellido = apellido,
        Dni = dni,
        Cuit = cuit,
        Email = $"{Slug(nombre)}.{Slug(apellido)}@gmail.com",
        Telefono = $"341-{4500000 + i * 151}",
        Direccion = $"{calles[i % calles.Length]} {100 + i * 41}",
        Localidad = localidades[i % localidades.Length],
        CondicionFiscalId = condicionId,
    });
}
context.Clientes.AddRange(clientesNuevos);
var clientesPool = clientesExistentes.Concat(clientesNuevos).ToList();

// ========== 4) Compras nuevas ==========
var compras = new List<Compra>();
for (int i = 0; i < 8; i++)
{
    var proveedor = proveedores[rnd.Next(proveedores.Count)];
    var fecha = FechaAleatoria(rnd, inicioVentana, hoy.AddDays(-20));
    var estadoDescripcion = Weighted(rnd, ("Confirmada", 75), ("Pendiente", 15), ("Anulada", 10));
    var productosProveedor = productos.Where(p => p.Proveedor == proveedor).ToList();
    if (productosProveedor.Count == 0) productosProveedor = productos;

    var detalles = new List<DetalleCompra>();
    var cantidadLineas = rnd.Next(1, 4);
    var productosElegidos = productosProveedor.OrderBy(_ => rnd.Next()).Take(Math.Min(cantidadLineas, productosProveedor.Count));
    decimal total = 0;
    foreach (var producto in productosElegidos)
    {
        var cantidad = rnd.Next(10, 51);
        var precioUnitario = producto.PrecioCompra;
        var subtotal = Math.Round(precioUnitario * cantidad, 2);
        total += subtotal;
        detalles.Add(new DetalleCompra
        {
            Producto = producto, Cantidad = cantidad, PrecioUnitario = precioUnitario, Subtotal = subtotal, FechaCreacion = fecha,
        });
    }

    compras.Add(new Compra
    {
        NumeroCompra = NumeroCompraUnico(fecha),
        Fecha = fecha,
        Proveedor = proveedor,
        Total = total,
        EstadoCompra = estadosCompra[estadoDescripcion],
        RegistradoPorId = adminUser?.Id,
        Observaciones = estadoDescripcion == "Anulada" ? "Anulada por error de carga (dato de prueba)." : null,
        Detalles = detalles,
        FechaCreacion = fecha,
    });
}
context.Compras.AddRange(compras);

// ========== 5) Ventas nuevas + Facturas/Notas de Crédito ==========
// Reglas replicadas de VentaService/FacturaService reales:
//  - Toda Venta nueva descuenta stock al crearse (sin importar si queda Pendiente o Confirmada).
//  - Si termina Anulada, el stock se repone (neto cero, pero quedan los dos movimientos, como en la app real).
//  - Solo las Ventas Confirmadas son facturables.
//  - La Factura (signo "+") NO vuelve a tocar stock (ya lo hizo la Venta). Solo la Nota de Crédito
//    (signo "-") genera su propio movimiento, reponiendo lo que se factura de más como devolución.
var facturaA = tiposComprobante["Factura A"];
var facturaB = tiposComprobante["Factura B"];
var notaCreditoA = tiposComprobante["Nota de Crédito A"];
var notaCreditoB = tiposComprobante["Nota de Crédito B"];

var ventas = new List<Venta>();
var facturas = new List<Factura>();
// Acumula movimientos de stock en memoria; se reproducen cronológicamente por producto al final
// (mismo patrón que SeedTestData) para que StockAnterior/StockPosterior queden consistentes entre sí.
var eventosPorProducto = new Dictionary<Producto, List<(DateTime Fecha, string Tipo, int Cantidad, string Origen)>>();
void AgregarEvento(Producto p, DateTime fecha, string tipo, int cantidad, string origen)
{
    if (!eventosPorProducto.TryGetValue(p, out var lista)) eventosPorProducto[p] = lista = new();
    lista.Add((fecha, tipo, cantidad, origen));
}
// Entidades cuyo historial de cambios se completa en una segunda pasada (necesitan el Id ya asignado).
var historialClientesPendiente = new List<Cliente>();
var historialFacturasPendiente = new List<(Factura Factura, string Detalle)>();

const int cantidadVentas = 30;
for (int i = 0; i < cantidadVentas; i++)
{
    var cliente = clientesPool[rnd.Next(clientesPool.Count)];
    var fecha = FechaAleatoria(rnd, inicioVentana, hoy);
    var estado = Weighted(rnd, (EstadoVenta.Confirmada, 70), (EstadoVenta.Pendiente, 20), (EstadoVenta.Anulada, 10));
    var numeroVenta = NumeroVentaUnico(fecha);

    var detalles = new List<DetalleVenta>();
    var cantidadLineas = rnd.Next(1, 5);
    var productosElegidos = productos.OrderBy(_ => rnd.Next()).Take(cantidadLineas).ToList();
    decimal subtotalVenta = 0;
    foreach (var producto in productosElegidos)
    {
        var cantidad = rnd.Next(1, 9);
        var precioUnitario = producto.PrecioVenta;
        var subtotalLinea = Math.Round(precioUnitario * cantidad, 2);
        subtotalVenta += subtotalLinea;
        detalles.Add(new DetalleVenta { Producto = producto, Cantidad = cantidad, PrecioUnitario = precioUnitario, Subtotal = subtotalLinea, FechaCreacion = fecha });
    }

    var descuento = rnd.Next(0, 100) < 25 ? Math.Round(subtotalVenta * (decimal)(rnd.Next(5, 16) / 100.0), 2) : 0m;
    var total = subtotalVenta - descuento;

    var venta = new Venta
    {
        NumeroVenta = numeroVenta, Fecha = fecha, Cliente = cliente,
        Subtotal = subtotalVenta, Descuento = descuento, Total = total, Estado = estado,
        Observaciones = estado == EstadoVenta.Anulada ? "Venta anulada a pedido del cliente (dato de prueba)." : null,
        Detalles = detalles, FechaCreacion = fecha,
    };
    ventas.Add(venta);

    // Toda venta nueva descuenta stock al crearse (replica VentaService.CreateAsync).
    foreach (var d in detalles)
        AgregarEvento(d.Producto, fecha, "VENTA", d.Cantidad, $"Venta {numeroVenta}");

    if (estado == EstadoVenta.Anulada)
    {
        // Se anula poco después: repone stock (neto cero, pero quedan ambos movimientos).
        var fechaAnulacion = fecha.AddHours(rnd.Next(1, 72));
        foreach (var d in detalles)
            AgregarEvento(d.Producto, fechaAnulacion, "ANULACION_VENTA", d.Cantidad, $"Anulación de {numeroVenta}");
        continue;
    }
    if (estado != EstadoVenta.Confirmada) continue; // Pendiente: no es facturable todavía.

    // Cuánto de cada línea ya está facturado en ESTE lote (presupuesto local, igual al cálculo
    // real de FacturadoPorLineaAsync, para no dejar pendientes negativos).
    var facturadoPorLinea = detalles.ToDictionary(d => d, _ => 0);

    Factura CrearFactura(TipoComprobante tipo, List<(DetalleVenta Linea, int Cantidad)> lineas, DateTime fechaEmision)
    {
        decimal sub = 0, iva = 0;
        var detallesFactura = new List<DetalleFactura>();
        foreach (var (linea, cantidad) in lineas)
        {
            var neto = linea.PrecioUnitario * cantidad;
            var ivaLinea = Math.Round(neto * 0.21m, 2);
            sub += neto; iva += ivaLinea;
            detallesFactura.Add(new DetalleFactura
            {
                Producto = linea.Producto, DetalleVenta = linea, Cantidad = cantidad,
                PrecioUnitario = linea.PrecioUnitario, Impuesto = 21m, Descuento = 0, Subtotal = neto,
            });
            facturadoPorLinea[linea] += cantidad;
        }
        var estadoFactura = Weighted(rnd, (EstadoFactura.Pagada, 55), (EstadoFactura.Emitida, 30), (EstadoFactura.Vencida, 10), (EstadoFactura.Anulada, 5));
        var f = new Factura
        {
            NumeroFactura = AsignarNumeroFactura(tipo), FechaEmision = fechaEmision, FechaVencimiento = fechaEmision.AddDays(30),
            Cliente = cliente, Venta = venta, TipoComprobante = tipo, PuntoVenta = puntoVenta,
            Subtotal = sub, Iva = iva, Total = sub + iva, Estado = estadoFactura,
            Observaciones = estadoFactura == EstadoFactura.Anulada ? "Factura anulada (dato de prueba)." : null,
            IdempotencyKey = Guid.NewGuid().ToString(), Detalles = detallesFactura, FechaCreacion = fechaEmision,
        };
        facturas.Add(f);
        historialFacturasPendiente.Add((f, $"Factura {f.NumeroFactura} emitida por $ {f.Total:N2} sobre venta {numeroVenta} (dato de prueba)."));
        return f;
    }

    var tipoFactura = Weighted(rnd, (facturaA, 70), (facturaB, 30));
    var bucket = Weighted(rnd, ("sin_facturar", 30), ("completa", 45), ("parcial", 15), ("parcial_luego_completa", 10));
    var fechaFactura1 = fecha.AddHours(rnd.Next(1, 48));

    switch (bucket)
    {
        case "completa":
            CrearFactura(tipoFactura, detalles.Select(d => (d, d.Cantidad)).ToList(), fechaFactura1);
            break;

        case "parcial":
        {
            // Factura solo una porción de UNA línea; el resto (de esa y las demás) queda pendiente
            // de facturar (alimenta realistamente la Cartera por facturar).
            var lineaParcial = detalles[rnd.Next(detalles.Count)];
            var porcion = Math.Max(1, lineaParcial.Cantidad * (rnd.Next(30, 70)) / 100);
            var f = CrearFactura(tipoFactura, new() { (lineaParcial, porcion) }, fechaFactura1);

            // Sobre ~1 de cada 3 facturas parciales, se emite además una Nota de Crédito chica
            // por una devolución (cantidad acotada al remanente para no dejar saldo negativo).
            var remanente = lineaParcial.Cantidad - porcion;
            if (remanente > 0 && rnd.Next(0, 100) < 33)
            {
                var ncCantidad = Math.Min(remanente, rnd.Next(1, 3));
                var tipoNc = Weighted(rnd, (notaCreditoA, 60), (notaCreditoB, 40));
                var fechaNc = fechaFactura1.AddHours(rnd.Next(2, 48));
                var neto = lineaParcial.PrecioUnitario * ncCantidad;
                var iva = Math.Round(neto * 0.21m, 2);
                var estadoNc = Weighted(rnd, (EstadoFactura.Emitida, 60), (EstadoFactura.Pagada, 40)); // nunca Anulada: simplifica la reversión de stock
                var nc = new Factura
                {
                    NumeroFactura = AsignarNumeroFactura(tipoNc), FechaEmision = fechaNc, FechaVencimiento = fechaNc.AddDays(30),
                    Cliente = cliente, Venta = venta, TipoComprobante = tipoNc, PuntoVenta = puntoVenta,
                    Subtotal = neto, Iva = iva, Total = neto + iva, Estado = estadoNc,
                    Observaciones = "Nota de crédito por devolución parcial (dato de prueba).",
                    IdempotencyKey = Guid.NewGuid().ToString(),
                    Detalles = new List<DetalleFactura> { new DetalleFactura { Producto = lineaParcial.Producto, DetalleVenta = lineaParcial, Cantidad = ncCantidad, PrecioUnitario = lineaParcial.PrecioUnitario, Impuesto = 21m, Descuento = 0, Subtotal = neto } },
                    FechaCreacion = fechaNc,
                };
                facturas.Add(nc);
                facturadoPorLinea[lineaParcial] += ncCantidad; // mismo criterio que FacturadoPorLineaAsync real
                historialFacturasPendiente.Add((nc, $"Nota de crédito {nc.NumeroFactura} por $ {nc.Total:N2} sobre venta {numeroVenta} (dato de prueba)."));
                AgregarEvento(lineaParcial.Producto, fechaNc, "ENTRADA", ncCantidad, $"{tipoNc.Descripcion} {nc.NumeroFactura} (sobre {numeroVenta})");
            }
            break;
        }

        case "parcial_luego_completa":
        {
            // Dos facturas sucesivas sobre la MISMA línea: cubre primero una parte y, días
            // después, el resto -- ejercita de punta a punta la facturación parcial.
            var lineaParcial = detalles[rnd.Next(detalles.Count)];
            var porcion1 = Math.Max(1, lineaParcial.Cantidad * (rnd.Next(30, 60)) / 100);
            CrearFactura(tipoFactura, new() { (lineaParcial, porcion1) }, fechaFactura1);

            var otrasLineas = detalles.Where(d => d != lineaParcial).Select(d => (d, d.Cantidad)).ToList();
            var porcion2 = lineaParcial.Cantidad - porcion1;
            var lineas2 = otrasLineas.Prepend((lineaParcial, porcion2)).ToList();
            var fechaFactura2 = fechaFactura1.AddDays(rnd.Next(2, 10));
            CrearFactura(tipoFactura, lineas2, fechaFactura2);
            break;
        }

        default: // "sin_facturar": queda íntegramente pendiente de facturar
            break;
    }
}
context.Ventas.AddRange(ventas);
context.Facturas.AddRange(facturas);

// ========== 6) Movimientos de stock: reproducir eventos nuevos en orden cronológico ==========
foreach (var compra in compras.Where(c => c.EstadoCompra.Descripcion == "Confirmada"))
    foreach (var detalle in compra.Detalles)
        AgregarEvento(detalle.Producto, compra.Fecha, "ENTRADA", detalle.Cantidad, $"Compra {compra.NumeroCompra}");

var movimientos = new List<MovimientoStock>();
foreach (var (producto, eventos) in eventosPorProducto)
{
    var actual = producto.StockActual; // parte del stock REAL actual, no se pisa nada existente
    foreach (var evento in eventos.OrderBy(e => e.Fecha))
    {
        var anterior = actual;
        actual = evento.Tipo switch
        {
            "ENTRADA" or "ANULACION_VENTA" => actual + evento.Cantidad,
            _ => Math.Max(0, actual - evento.Cantidad), // "VENTA"
        };
        movimientos.Add(new MovimientoStock
        {
            Producto = producto, Cantidad = evento.Cantidad, TipoMovimiento = evento.Tipo,
            Observacion = evento.Origen, StockAnterior = anterior, StockPosterior = actual, FechaCreacion = evento.Fecha,
        });
    }
    producto.StockActual = actual;
}
context.MovimientosStock.AddRange(movimientos);

historialClientesPendiente.AddRange(clientesNuevos);

Console.WriteLine("Guardando datos nuevos (fase 1/2)...");
await context.SaveChangesAsync();

// ========== 7) Historial de cambios (requiere los Id ya asignados por el SaveChanges anterior) ==========
var historial = new List<HistorialCambio>();
foreach (var c in historialClientesPendiente)
    historial.Add(new HistorialCambio { Entidad = "Cliente", EntidadId = c.Id, Accion = "Alta", UsuarioId = adminUser?.Id, Fecha = c.FechaCreacion, Detalle = $"Cliente {c.Nombre} {c.Apellido} creado (dato de prueba)." });
foreach (var (f, detalle) in historialFacturasPendiente)
    historial.Add(new HistorialCambio { Entidad = "Factura", EntidadId = f.Id, Accion = "Emision", UsuarioId = adminUser?.Id, Fecha = f.FechaEmision, Detalle = detalle });
context.Set<HistorialCambio>().AddRange(historial);

Console.WriteLine("Guardando historial de cambios (fase 2/2)...");
await context.SaveChangesAsync();

Console.WriteLine();
Console.WriteLine("=== Resumen de registros nuevos agregados ===");
Console.WriteLine($"Proveedores:        {proveedoresNuevos.Count}");
Console.WriteLine($"Productos:          {productosNuevos.Count}");
Console.WriteLine($"Clientes:           {clientesNuevos.Count}");
Console.WriteLine($"Compras:            {compras.Count} (detalles: {compras.Sum(c => c.Detalles.Count)})");
Console.WriteLine($"Ventas:             {ventas.Count} (detalles: {ventas.Sum(v => v.Detalles.Count)})");
Console.WriteLine($"Facturas/NC:        {facturas.Count}");
Console.WriteLine($"Movimientos stock:  {movimientos.Count}");
Console.WriteLine($"Historial cambios:  {historial.Count}");
var totalRegistrosPrincipales = proveedoresNuevos.Count + productosNuevos.Count + clientesNuevos.Count + compras.Count + ventas.Count + facturas.Count;
Console.WriteLine($"TOTAL (entidades principales): {totalRegistrosPrincipales}");
Console.WriteLine("Listo.");
