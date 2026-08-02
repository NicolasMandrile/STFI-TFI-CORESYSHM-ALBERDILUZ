using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Enums;

var rnd = new Random(20260802);

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=CoreSysHM_Dev;Trusted_Connection=True;TrustServerCertificate=True;");
using var context = new ApplicationDbContext(optionsBuilder.Options);

Console.WriteLine("Borrando datos de prueba anteriores...");
foreach (var table in new[]
{
    "DetallesVenta", "Facturas", "DetallesCompra", "Ventas", "Compras",
    "MovimientosStock", "Productos", "Proveedores", "Categorias", "Clientes"
})
{
    await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
}
foreach (var (table, seed) in new[]
{
    ("Categorias", 0), ("Proveedores", 0), ("Productos", 0), ("Clientes", 0),
    ("Compras", 0), ("DetallesCompra", 0), ("Ventas", 0), ("DetallesVenta", 0),
    ("Facturas", 0), ("MovimientosStock", 0)
})
{
    await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{table}', RESEED, {seed})");
}

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

var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@coresyshm.com");

// ---------- Categorias (rubros reales de Alberdi Luz) ----------
var categoriaNombres = new[]
{
    ("Candados y Cerraduras", "Candados, cerraduras y llaves de seguridad"),
    ("Iluminación", "Luminarias LED, lámparas y accesorios de iluminación"),
    ("Ferretería General", "Pilas, precintos, adaptadores y accesorios varios"),
    ("Aceites y Lubricantes", "Aceites, grasas y lubricantes multiuso"),
    ("Pinturas", "Pinturas en aerosol para uso general"),
    ("Adhesivos y Selladores", "Adhesivos, siliconas y espumas de poliuretano"),
    ("Insecticidas y Plagas", "Insecticidas, raticidas y productos para control de plagas"),
};
var categorias = categoriaNombres.Select(c => new Categoria { Nombre = c.Item1, Descripcion = c.Item2 }).ToList();
context.Categorias.AddRange(categorias);
var cat = categorias.ToDictionary(c => c.Nombre);

// ---------- Proveedores (marcas reales relevadas del sitio) ----------
var proveedorDatos = new[]
{
    ("Sekur Argentina S.A.", "30-50011122-3", "0341-455-1010", "ventas@sekur.com.ar", "Parque Industrial, Rosario", "Diego Alonso"),
    ("Nine Seguridad", "30-50022233-4", "0341-455-2020", "info@nineseguridad.com.ar", "Av. Circunvalación 4500, Rosario", "Marcela Ponce"),
    ("Nova Electricity S.R.L.", "30-50033344-5", "0341-455-3030", "ventas@novaelectricity.com.ar", "Ovidio Lagos 2100, Rosario", "Sergio Beltrán"),
    ("Philips Argentina", "30-50044455-6", "011-4555-4040", "comercial@philips.com.ar", "Av. del Libertador 6810, CABA", "Cecilia Fontana"),
    ("Distribuidora Ferretera del Sur", "30-50055566-7", "0341-455-5050", "pedidos@ferreterasur.com.ar", "Bv. Oroño 3200, Rosario", "Hugo Peralta"),
    ("Unipega S.A.", "30-50066677-8", "011-4555-6060", "ventas@unipega.com.ar", "Parque Industrial Pilar, Bs. As.", "Natalia Ríos"),
    ("Aceitex Lubricantes", "30-50077788-9", "0341-455-7070", "info@aceitex.com.ar", "Zona Industrial, Villa Gob. Gálvez", "Pablo Iturbe"),
    ("Kuwait Pinturería", "30-50088899-0", "0341-455-8080", "ventas@kuwaitpinturas.com.ar", "Av. Uriburu 5400, Rosario", "Gisela Marchetti"),
    ("Geltex / Allemandi Distribuidora", "30-50099900-1", "0341-455-9090", "comercial@geltexallemandi.com.ar", "Ruta 34 Km 3, Funes", "Alberto Cortez"),
};
var proveedores = proveedorDatos.Select(p => new Proveedor
{
    RazonSocial = p.Item1, Cuit = p.Item2, Telefono = p.Item3, Email = p.Item4, Direccion = p.Item5, Contacto = p.Item6
}).ToList();
context.Proveedores.AddRange(proveedores);
var prov = proveedores.ToDictionary(p => p.RazonSocial);

// ---------- Productos (catálogo real relevado de alberdiluz.com.ar) ----------
// (codigoSitio, nombre, categoria, proveedor, precioCompraMin, precioCompraMax, markupMin, markupMax, stockMin, stockMax, stockMinimoMin, stockMinimoMax)
var productoSeeds = new (int Codigo, string Nombre, string Categoria, string Proveedor, decimal PcMin, decimal PcMax, decimal MkMin, decimal MkMax, int StMin, int StMax, int SmMin, int SmMax)[]
{
    // Candados y Cerraduras
    (1, "Llave Sekur 26 Fresada", "Candados y Cerraduras", "Sekur Argentina S.A.", 2500, 3800, 1.4m, 1.6m, 60, 120, 10, 20),
    (2, "Llave Sekur 31 Fresada", "Candados y Cerraduras", "Sekur Argentina S.A.", 2800, 4200, 1.4m, 1.6m, 60, 120, 10, 20),
    (3, "Llave Sekur 40 Fresada", "Candados y Cerraduras", "Sekur Argentina S.A.", 3200, 4800, 1.4m, 1.6m, 50, 100, 10, 20),
    (121, "Candado Taurus 25 Doble T.", "Candados y Cerraduras", "Sekur Argentina S.A.", 4500, 6000, 1.5m, 1.7m, 40, 90, 8, 18),
    (126, "Candado Taurus 38 Doble T", "Candados y Cerraduras", "Sekur Argentina S.A.", 6000, 8000, 1.5m, 1.7m, 35, 80, 8, 15),
    (128, "Candado Taurus 63 Doble T", "Candados y Cerraduras", "Sekur Argentina S.A.", 8500, 11000, 1.5m, 1.7m, 25, 60, 6, 12),
    (262, "Candado Nine 25mm", "Candados y Cerraduras", "Nine Seguridad", 2200, 3000, 1.6m, 1.8m, 60, 130, 12, 25),
    (265, "Candado Nine 50mm", "Candados y Cerraduras", "Nine Seguridad", 4200, 5600, 1.6m, 1.8m, 40, 90, 10, 20),
    (267, "Candado Nine Discus 70mm", "Candados y Cerraduras", "Nine Seguridad", 6500, 8500, 1.6m, 1.8m, 25, 60, 6, 15),
    (269, "Candado Nine Combinación", "Candados y Cerraduras", "Nine Seguridad", 5500, 7200, 1.5m, 1.7m, 20, 50, 5, 12),

    // Iluminación
    (1037, "Panel LED Redondo Embutir 24W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 6500, 9000, 1.5m, 1.8m, 25, 60, 6, 15),
    (1040, "Panel LED Redondo Sobreponer 24W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 6800, 9400, 1.5m, 1.8m, 25, 60, 6, 15),
    (201, "Luminaria Embutir Akai Cuadrada 12W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 3800, 5200, 1.6m, 1.9m, 30, 70, 8, 18),
    (205, "Luminaria Embutir Akai Circular 24W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 6200, 8200, 1.6m, 1.9m, 25, 55, 6, 15),
    (207, "Luminaria Sobreponer Akai Cuadrado 18W Luz Cálida", "Iluminación", "Nova Electricity S.R.L.", 4800, 6400, 1.6m, 1.9m, 20, 50, 5, 12),
    (210, "Luminaria Sobreponer Akai Circular 18W Luz Cálida", "Iluminación", "Philips Argentina", 5200, 7000, 1.6m, 1.9m, 20, 45, 5, 12),
    (225, "Lámpara Miniperfume 15W E14 Clara", "Iluminación", "Philips Argentina", 3600, 4800, 1.5m, 1.8m, 40, 90, 10, 20),
    (229, "Lámpara Baby Luz T16 5W E14", "Iluminación", "Philips Argentina", 2400, 3200, 1.5m, 1.8m, 40, 90, 10, 20),
    (1011, "Lámpara Intercom Bulb 10W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 3200, 4400, 1.5m, 1.8m, 30, 70, 8, 18),
    (1014, "Lámpara Intercom Bulb 15W Luz Día", "Iluminación", "Nova Electricity S.R.L.", 3800, 5000, 1.5m, 1.8m, 30, 70, 8, 18),

    // Ferretería General
    (4216, "Precinto Negro 765mm x 8,8mm", "Ferretería General", "Distribuidora Ferretera del Sur", 350, 550, 1.8m, 2.2m, 150, 300, 30, 60),
    (4, "Enchufe Adaptador Universal", "Ferretería General", "Distribuidora Ferretera del Sur", 900, 1300, 1.7m, 2.0m, 80, 160, 15, 35),
    (24, "Pila Duracell Alcalina 9V", "Ferretería General", "Distribuidora Ferretera del Sur", 2200, 2800, 1.5m, 1.8m, 60, 130, 15, 30),
    (25, "Pila Duracell Alcalina D x2", "Ferretería General", "Distribuidora Ferretera del Sur", 2600, 3200, 1.5m, 1.8m, 50, 110, 12, 25),
    (28, "Pila Duracell Alcalina C x2", "Ferretería General", "Distribuidora Ferretera del Sur", 2400, 3000, 1.5m, 1.8m, 50, 110, 12, 25),
    (36, "Pila Duracell AAA Blíster x4", "Ferretería General", "Distribuidora Ferretera del Sur", 1800, 2400, 1.6m, 1.9m, 70, 150, 15, 30),
    (39, "Pila Duracell AAA Tira x12", "Ferretería General", "Distribuidora Ferretera del Sur", 4800, 6200, 1.6m, 1.9m, 30, 70, 8, 18),
    (40, "Barbijo KN95", "Ferretería General", "Distribuidora Ferretera del Sur", 400, 650, 1.8m, 2.2m, 200, 400, 40, 80),
    (43, "Pila Duracell Audífono Tipo 312 x6", "Ferretería General", "Distribuidora Ferretera del Sur", 1600, 2100, 1.5m, 1.8m, 40, 90, 10, 20),
    (45, "Pila Duracell Recargable AA x2", "Ferretería General", "Distribuidora Ferretera del Sur", 3800, 4800, 1.5m, 1.8m, 30, 70, 8, 18),

    // Aceites y Lubricantes
    (33, "Unipega Lubricante Spray x 300ml", "Aceites y Lubricantes", "Unipega S.A.", 2800, 3600, 1.4m, 1.7m, 40, 90, 10, 20),
    (377, "Solución Lubricante Kuwait x 230grs", "Aceites y Lubricantes", "Kuwait Pinturería", 1900, 2500, 1.4m, 1.7m, 40, 90, 10, 20),
    (653, "Aceitex Afloja Tuercas x 100cc", "Aceites y Lubricantes", "Aceitex Lubricantes", 1200, 1700, 1.5m, 1.8m, 50, 110, 12, 25),
    (654, "Aceitex RK 2000 x 220cc", "Aceites y Lubricantes", "Aceitex Lubricantes", 2200, 2900, 1.4m, 1.7m, 35, 80, 8, 18),
    (655, "Aceitex RK 3000 x 220cc", "Aceites y Lubricantes", "Aceitex Lubricantes", 2600, 3400, 1.4m, 1.7m, 30, 70, 8, 18),
    (657, "Aceitodo Multiuso Aerosol x 220cc", "Aceites y Lubricantes", "Aceitex Lubricantes", 2400, 3100, 1.4m, 1.7m, 40, 90, 10, 20),
    (658, "Aceitodo Multiuso Aerosol x 400cc", "Aceites y Lubricantes", "Aceitex Lubricantes", 3800, 4800, 1.4m, 1.7m, 25, 60, 6, 15),
    (660, "Aceitodo Grasa de Litio x 90grs", "Aceites y Lubricantes", "Aceitex Lubricantes", 1600, 2100, 1.5m, 1.8m, 35, 80, 8, 18),
    (665, "Aceitodo Grasa Grafitada x 250grs", "Aceites y Lubricantes", "Aceitex Lubricantes", 3200, 4000, 1.4m, 1.7m, 25, 55, 6, 15),
    (372, "Infla y Sella Neumáticos x 300grs", "Aceites y Lubricantes", "Aceitex Lubricantes", 4200, 5400, 1.4m, 1.7m, 20, 50, 5, 12),

    // Pinturas (aerosol Kuwait 240cc)
    (306, "Aerosol Kuwait Blanco x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 40, 90, 10, 20),
    (315, "Aerosol Kuwait Negro x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 40, 90, 10, 20),
    (317, "Aerosol Kuwait Negro Mate x 240cc", "Pinturas", "Kuwait Pinturería", 2000, 2500, 1.5m, 1.8m, 30, 70, 8, 18),
    (311, "Aerosol Kuwait Gris Oscuro x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 30, 70, 8, 18),
    (312, "Aerosol Kuwait Gris Perla x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 25, 60, 6, 15),
    (302, "Aerosol Kuwait Azul Marino x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 25, 60, 6, 15),
    (309, "Aerosol Kuwait Celeste x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 25, 60, 6, 15),
    (300, "Aerosol Kuwait Amarillo x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 20, 50, 5, 12),
    (314, "Aerosol Kuwait Naranja x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 20, 50, 5, 12),
    (305, "Aerosol Kuwait Bermellón x 240cc", "Pinturas", "Kuwait Pinturería", 1900, 2400, 1.5m, 1.8m, 20, 50, 5, 12),

    // Adhesivos y Selladores
    (5, "Unipega Transparente x 50ml", "Adhesivos y Selladores", "Unipega S.A.", 1200, 1700, 1.5m, 1.8m, 50, 110, 12, 25),
    (6, "Unipega Híbrido Negro x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 3800, 4800, 1.4m, 1.7m, 30, 70, 8, 18),
    (7, "Unipega Acrílico Blanco x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 2600, 3400, 1.4m, 1.7m, 35, 80, 8, 18),
    (9, "Unipega Espuma Poliuretano x 300ml", "Adhesivos y Selladores", "Unipega S.A.", 4200, 5400, 1.4m, 1.7m, 25, 60, 6, 15),
    (8, "Unipega Espuma Poliuretano x 750ml", "Adhesivos y Selladores", "Unipega S.A.", 7500, 9200, 1.4m, 1.7m, 15, 40, 4, 10),
    (12, "Silicona Neutra Unipega Transparente x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 3400, 4300, 1.4m, 1.7m, 30, 70, 8, 18),
    (13, "Unipega Alta Temperatura Roja x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 3600, 4600, 1.4m, 1.7m, 20, 50, 5, 12),
    (17, "Unipega Canaletas Aluminio x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 3200, 4100, 1.4m, 1.7m, 25, 60, 6, 15),
    (35, "Unipega High Tack Ultra Blanco x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 4400, 5600, 1.4m, 1.7m, 20, 45, 5, 12),
    (19, "Unipega Con Fungicida Transparente x 280ml", "Adhesivos y Selladores", "Unipega S.A.", 3000, 3900, 1.4m, 1.7m, 25, 60, 6, 15),

    // Insecticidas y Plagas
    (50, "Geltex Cucarachicida Jeringa x 6gs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 3200, 4200, 1.5m, 1.8m, 30, 70, 8, 18),
    (51, "Geltex Cucarachicida Jeringa x 12gs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 5200, 6400, 1.5m, 1.8m, 20, 50, 5, 12),
    (54, "Geltex Raticida x 1000grs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 8000, 9800, 1.4m, 1.7m, 12, 30, 3, 8),
    (55, "Geltex Hormiguicida Jeringa x 6gs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 3200, 4200, 1.5m, 1.8m, 30, 70, 8, 18),
    (65, "K-Othrina Sachets 15ml x 24un", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 6200, 7600, 1.4m, 1.7m, 15, 40, 4, 10),
    (66, "K-Othrina Botella x 60ml", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 4200, 5400, 1.4m, 1.7m, 20, 50, 5, 12),
    (67, "Myrmec Babosa y Caracol x 200grs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 2600, 3400, 1.5m, 1.8m, 25, 60, 6, 15),
    (71, "Fumixan Hogar x 1 unidad", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 4800, 6000, 1.4m, 1.7m, 20, 45, 5, 12),
    (84, "Hormiguicida Dual Allemandi x 250gs", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 2200, 2900, 1.5m, 1.8m, 30, 65, 8, 15),
    (87, "Fluido Desinfectante Allemandi x 500cc", "Insecticidas y Plagas", "Geltex / Allemandi Distribuidora", 3600, 4600, 1.4m, 1.7m, 25, 55, 6, 15),
};

var productos = new List<Producto>();
var stockInicial = new Dictionary<Producto, int>();
foreach (var s in productoSeeds)
{
    var precioCompra = Math.Round(s.PcMin + (decimal)rnd.NextDouble() * (s.PcMax - s.PcMin), 2);
    var markup = s.MkMin + (decimal)rnd.NextDouble() * (s.MkMax - s.MkMin);
    var precioVenta = Math.Round(precioCompra * markup, 2);
    var stock = rnd.Next(s.StMin, s.StMax + 1);
    var stockMinimo = rnd.Next(s.SmMin, s.SmMax + 1);

    var producto = new Producto
    {
        Codigo = $"AL-{s.Codigo}",
        Nombre = s.Nombre,
        Descripcion = $"Producto relevado del catálogo de Alberdi Luz (código de sitio {s.Codigo}).",
        PrecioCompra = precioCompra,
        PrecioVenta = precioVenta,
        StockActual = stock,
        StockMinimo = stockMinimo,
        Categoria = cat[s.Categoria],
        Proveedor = prov[s.Proveedor],
    };
    productos.Add(producto);
    stockInicial[producto] = stock;
}
context.Productos.AddRange(productos);

// ---------- Clientes (zona Rosario y alrededores) ----------
var nombres = new[] { "Roberto", "Laura", "Diego", "Valentina", "Martín", "Lucía", "Franco", "Camila", "Nicolás", "Sofía",
    "Matías", "Agustina", "Federico", "Julieta", "Gonzalo", "Micaela", "Ezequiel", "Florencia", "Ignacio", "Rocío",
    "Bruno", "Antonella", "Tomás", "Milagros", "Joaquín", "Carla", "Emiliano", "Daniela", "Rodrigo", "Yamila" };
var apellidos = new[] { "Sánchez", "Fernández", "Ramírez", "López", "García", "Torres", "Martínez", "Gómez", "Díaz", "Romero",
    "Álvarez", "Molina", "Herrera", "Acosta", "Silva", "Benítez", "Rojas", "Medina", "Núñez", "Ortiz",
    "Ibáñez", "Cabrera", "Aguirre", "Vega", "Correa", "Godoy", "Paz", "Villalba", "Ferreyra", "Sosa" };
var localidades = new[] { "Rosario", "Funes", "Roldán", "Pérez", "Villa Gobernador Gálvez", "Granadero Baigorria",
    "Capitán Bermúdez", "San Lorenzo", "Fray Luis Beltrán", "Ibarlucea", "Soldini", "Zavalla" };
var calles = new[] { "San Martín", "Córdoba", "Mitre", "Pellegrini", "Corrientes", "Urquiza", "Sarmiento", "Moreno",
    "Rivadavia", "Entre Ríos", "Tucumán", "Santa Fe", "Buenos Aires", "España", "Italia" };

var clientes = new List<Cliente>();
for (int i = 0; i < nombres.Length; i++)
{
    var nombre = nombres[i];
    var apellido = apellidos[i];
    var dni = 25000000 + i * 54321;
    var cuitPrefijo = i % 2 == 0 ? "20" : "27";
    clientes.Add(new Cliente
    {
        Nombre = nombre,
        Apellido = apellido,
        Dni = dni.ToString(),
        Cuit = $"{cuitPrefijo}-{dni}-{(i % 9) + 1}",
        Email = $"{Slug(nombre)}.{Slug(apellido)}@gmail.com",
        Telefono = $"341-{4000000 + i * 137}",
        Direccion = $"{calles[i % calles.Length]} {100 + i * 37}",
        Localidad = localidades[i % localidades.Length],
    });
}
context.Clientes.AddRange(clientes);

var estadosCompra = await context.EstadosCompra.ToDictionaryAsync(e => e.Descripcion);
var hoy = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc);
var inicioVentana = hoy.AddMonths(-6);

DateTime FechaAleatoria(DateTime desde, DateTime hasta) =>
    desde.AddDays(rnd.Next(0, (hasta - desde).Days + 1)).AddHours(rnd.Next(8, 19)).AddMinutes(rnd.Next(0, 60));

// ---------- Compras ----------
var compras = new List<Compra>();
for (int i = 1; i <= 60; i++)
{
    var proveedor = proveedores[rnd.Next(proveedores.Count)];
    var fecha = FechaAleatoria(inicioVentana, hoy.AddMonths(-1));
    var estadoDescripcion = Weighted(rnd, ("Confirmada", 75), ("Pendiente", 15), ("Anulada", 10));
    var productosProveedor = productos.Where(p => p.Proveedor == proveedor).ToList();
    if (productosProveedor.Count == 0) productosProveedor = productos;

    var detalles = new List<DetalleCompra>();
    var cantidadLineas = rnd.Next(1, 4);
    var productosElegidos = productosProveedor.OrderBy(_ => rnd.Next()).Take(Math.Min(cantidadLineas, productosProveedor.Count));
    decimal total = 0;
    foreach (var producto in productosElegidos)
    {
        var cantidad = rnd.Next(10, 61);
        var precioUnitario = producto.PrecioCompra;
        var subtotal = Math.Round(precioUnitario * cantidad, 2);
        total += subtotal;
        detalles.Add(new DetalleCompra
        {
            Producto = producto,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = subtotal,
            FechaCreacion = fecha,
        });
    }

    compras.Add(new Compra
    {
        NumeroCompra = $"OC-{i:D5}",
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

// ---------- Ventas + Facturas ----------
var ventas = new List<Venta>();
var facturas = new List<Factura>();
int facturaSeq = 1;
for (int i = 1; i <= 90; i++)
{
    var cliente = clientes[rnd.Next(clientes.Count)];
    var fecha = FechaAleatoria(inicioVentana, hoy);
    var estado = Weighted(rnd, (EstadoVenta.Confirmada, 70), (EstadoVenta.Pendiente, 20), (EstadoVenta.Anulada, 10));

    var detalles = new List<DetalleVenta>();
    var cantidadLineas = rnd.Next(1, 5);
    var productosElegidos = productos.OrderBy(_ => rnd.Next()).Take(cantidadLineas);
    decimal subtotalVenta = 0;
    foreach (var producto in productosElegidos)
    {
        var cantidad = rnd.Next(1, 9);
        var precioUnitario = producto.PrecioVenta;
        var subtotalLinea = Math.Round(precioUnitario * cantidad, 2);
        subtotalVenta += subtotalLinea;
        detalles.Add(new DetalleVenta
        {
            Producto = producto,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = subtotalLinea,
            FechaCreacion = fecha,
        });
    }

    var descuento = rnd.Next(0, 100) < 25 ? Math.Round(subtotalVenta * (decimal)(rnd.Next(5, 16) / 100.0), 2) : 0m;
    var total = subtotalVenta - descuento;

    var venta = new Venta
    {
        NumeroVenta = $"V-{i:D5}",
        Fecha = fecha,
        Cliente = cliente,
        Subtotal = subtotalVenta,
        Descuento = descuento,
        Total = total,
        Estado = estado,
        Observaciones = estado == EstadoVenta.Anulada ? "Venta anulada a pedido del cliente (dato de prueba)." : null,
        Detalles = detalles,
        FechaCreacion = fecha,
    };
    ventas.Add(venta);

    if (estado == EstadoVenta.Confirmada && rnd.Next(0, 100) < 80)
    {
        var fechaEmision = fecha.AddHours(rnd.Next(0, 48));
        var iva = Math.Round(total * 0.21m, 2);
        var estadoFactura = Weighted(rnd, (EstadoFactura.Pagada, 55), (EstadoFactura.Emitida, 30), (EstadoFactura.Vencida, 10), (EstadoFactura.Anulada, 5));
        facturas.Add(new Factura
        {
            NumeroFactura = $"0001-{facturaSeq++:D8}",
            FechaEmision = fechaEmision,
            FechaVencimiento = fechaEmision.AddDays(30),
            Cliente = cliente,
            Venta = venta,
            Subtotal = total,
            Iva = iva,
            Total = total + iva,
            Estado = estadoFactura,
            Observaciones = estadoFactura == EstadoFactura.Anulada ? "Factura anulada (dato de prueba)." : null,
            FechaCreacion = fechaEmision,
        });
    }
}
context.Ventas.AddRange(ventas);
context.Facturas.AddRange(facturas);

// ---------- Movimientos de stock (ENTRADA por compras confirmadas, SALIDA por ventas confirmadas) ----------
var eventosPorProducto = new Dictionary<Producto, List<(DateTime Fecha, string Tipo, int Cantidad, string Origen)>>();
void AgregarEvento(Producto p, DateTime fecha, string tipo, int cantidad, string origen)
{
    if (!eventosPorProducto.TryGetValue(p, out var lista))
        eventosPorProducto[p] = lista = new List<(DateTime, string, int, string)>();
    lista.Add((fecha, tipo, cantidad, origen));
}

foreach (var compra in compras.Where(c => c.EstadoCompra.Descripcion == "Confirmada"))
    foreach (var detalle in compra.Detalles)
        AgregarEvento(detalle.Producto, compra.Fecha, "ENTRADA", detalle.Cantidad, $"Compra {compra.NumeroCompra}");

foreach (var venta in ventas.Where(v => v.Estado == EstadoVenta.Confirmada))
    foreach (var detalle in venta.Detalles)
        AgregarEvento(detalle.Producto, venta.Fecha, "SALIDA", detalle.Cantidad, $"Venta {venta.NumeroVenta}");

var movimientos = new List<MovimientoStock>();
foreach (var producto in productos)
{
    if (!eventosPorProducto.TryGetValue(producto, out var eventos)) continue;
    var actual = stockInicial[producto];
    foreach (var evento in eventos.OrderBy(e => e.Fecha))
    {
        var anterior = actual;
        if (evento.Tipo == "ENTRADA")
        {
            actual += evento.Cantidad;
        }
        else
        {
            actual = Math.Max(0, actual - evento.Cantidad);
        }
        movimientos.Add(new MovimientoStock
        {
            Producto = producto,
            Cantidad = evento.Cantidad,
            TipoMovimiento = evento.Tipo,
            Observacion = evento.Origen,
            StockAnterior = anterior,
            StockPosterior = actual,
            FechaCreacion = evento.Fecha,
        });
    }
    producto.StockActual = actual;
}
context.MovimientosStock.AddRange(movimientos);

Console.WriteLine("Guardando datos de prueba...");
await context.SaveChangesAsync();

Console.WriteLine($"Categorías: {categorias.Count}");
Console.WriteLine($"Proveedores: {proveedores.Count}");
Console.WriteLine($"Productos: {productos.Count}");
Console.WriteLine($"Clientes: {clientes.Count}");
Console.WriteLine($"Compras: {compras.Count} (detalles: {compras.Sum(c => c.Detalles.Count)})");
Console.WriteLine($"Ventas: {ventas.Count} (detalles: {ventas.Sum(v => v.Detalles.Count)})");
Console.WriteLine($"Facturas: {facturas.Count}");
Console.WriteLine($"Movimientos de stock: {movimientos.Count}");
Console.WriteLine("Listo.");
