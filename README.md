# CoreSys HM — Sistema de Gestión Operativa

Proyecto de Fin de Año — Ingeniería en Sistemas

## Módulos
- **Stock**: gestión de productos, categorías y movimientos de inventario
- **Ventas**: registro de clientes y procesamiento de ventas
- **Facturación**: emisión (parcial o total) de comprobantes sobre ventas confirmadas, con
  numeración correlativa por punto de venta, idempotencia y anulación
- **Maestros**: alta, edición y baja lógica de Clientes y Proveedores, con unicidad fiscal
  (DNI/CUIT) e historial de cambios
- **Reportes operativos**: facturación por período, desempeño por cliente/producto y cartera
  pendiente de facturar, con exportación a CSV

## Stack Tecnológico
| Capa | Tecnología |
|------|-----------|
| Frontend | Angular 22 |
| Backend | .NET 8 / ASP.NET Core Web API |
| Base de Datos | SQL Server 2022 LocalDB (dev) / SQL Server (prod) |
| ORM | Entity Framework Core 8 (Code-First, Migrations) |
| Autenticación | JWT Bearer |
| Documentación API | Swagger / OpenAPI |

## Arquitectura Backend — Clean Architecture
```
CoreSysHM.Domain          → Entidades, interfaces, excepciones de dominio
CoreSysHM.Application     → DTOs, interfaces de servicios, AutoMapper
CoreSysHM.Infrastructure  → EF Core, repositorios, Unit of Work
CoreSysHM.API             → Controllers REST, middleware, configuración
```

## Estructura del repositorio
```
CoreSysHM/
├── Backend/
│   ├── CoreSysHM.sln
│   └── src/
│       ├── CoreSysHM.Domain/
│       ├── CoreSysHM.Application/
│       ├── CoreSysHM.Infrastructure/
│       └── CoreSysHM.API/
├── Frontend/
│   └── coresys-hm-frontend/   (Angular 22)
└── Database/
    └── Scripts/
        ├── 01_CreateDatabase.sql
        ├── 02_CreateTables.sql
        ├── 03_StoredProcedures.sql
        └── 04_SeedData.sql
```

## Cómo levantar el proyecto

### Prerequisitos
- .NET 8 SDK
- Node.js 18+ / npm
- SQL Server 2022 LocalDB (instalado automáticamente con `SqlLocalDB.msi`)

### Base de datos
La base de datos se crea y migra automáticamente al iniciar el backend.
Connection string (Development): `Server=(localdb)\MSSQLLocalDB;Database=CoreSysHM_Dev;Trusted_Connection=True`

Para iniciar la instancia LocalDB manualmente:
```bash
sqllocaldb start MSSQLLocalDB
```

### Backend
```bash
cd Backend
dotnet run --project src/CoreSysHM.API
```
API disponible en: `http://localhost:5000` | Swagger: `http://localhost:5000/swagger`

Primera vez (si hace falta recrear migraciones):
```bash
dotnet ef migrations add InitialCreate --project src/CoreSysHM.Infrastructure --startup-project src/CoreSysHM.API --output-dir Data/Migrations
dotnet ef database update --project src/CoreSysHM.Infrastructure --startup-project src/CoreSysHM.API
```

### Frontend
```bash
cd Frontend/coresys-hm-frontend
npm install
ng serve
```
App disponible en: `http://localhost:4200`

## Credenciales de prueba
| Usuario | Email | Contraseña | Rol |
|---------|-------|-----------|-----|
| admin | admin@coresyshm.com | Admin123! | Administrador |

Los roles del sistema son `Administrador`, `Administrativo` y `Cliente` (ver
`CoreSysHM.Domain.Security.RoleNames`). Los usuarios `Administrativo`/`Cliente` se crean desde
la pantalla de Usuarios (o el registro de portal, para Cliente) — no vienen preseedeados.

## Facturación, Maestros y Reportes (Iteración 2)

### Catálogos seedeados automáticamente
- **CondicionFiscal**: Responsable Inscripto, Monotributo, Consumidor Final, Exento.
- **TipoComprobante**: Factura A/B (afecta stock, signo `+`), Nota de Crédito A/B (afecta
  stock, signo `-`).
- **PuntoVenta**: `0001`.
- **NumeracionComprobante**: contador atómico por par (PuntoVenta, TipoComprobante), arranca en 0.

### Reglas de negocio clave
- **Stock**: se descuenta una única vez, al confirmarse la `Venta` (comportamiento ya
  existente). La `Factura` (signo `+`) es un documento fiscal sobre una venta cuyo stock ya se
  movió — **no** vuelve a tocarlo. Solo las Notas de Crédito (signo `-`) generan su propio
  movimiento de stock, porque reponen algo que todavía no estaba registrado (una devolución).
- **Facturación parcial**: cada línea de venta (`DetalleVenta`) puede facturarse en varias
  facturas mientras haya saldo pendiente (`GET /api/facturas/ventas/{id}/saldo`); la
  sobre-facturación se rechaza.
- **Idempotencia**: toda emisión requiere una `IdempotencyKey` (el frontend genera un
  `crypto.randomUUID()`); reenviar la misma clave devuelve la factura ya emitida en vez de
  duplicarla.
- **Numeración**: correlativa y sin huecos por punto de venta + tipo de comprobante, formato
  `"0001-00000001"`, asignada con un `UPDATE` atómico sobre `NumeracionesComprobante`.
- **Unicidad fiscal**: DNI (Cliente) y CUIT (Cliente/Proveedor) son únicos entre los registros
  activos; un duplicado responde `409 Conflict`. La baja es lógica (soft delete) y libera el
  documento para un alta nueva sin perder el historial de ventas/facturas ya emitidas.
- **Historial de cambios**: alta/edición/baja de Cliente/Proveedor y emisión/anulación de
  Factura quedan registradas en `HistorialCambio` (`GET /api/clientes/{id}/historial`,
  `GET /api/proveedores/{id}/historial`).

### Endpoints principales
| Recurso | Endpoints |
|---|---|
| Facturas | `GET/POST api/facturas`, `POST api/facturas/{id}/anular`, `GET api/facturas/ventas-facturables`, `GET api/facturas/ventas/{id}/saldo`, `GET api/facturas/tipos-comprobante`, `GET api/facturas/puntos-venta` |
| Clientes | `GET/POST/PUT/DELETE api/clientes`, `GET api/clientes/{id}/historial` |
| Proveedores | `GET/POST/PUT/DELETE api/proveedores`, `GET api/proveedores/{id}/historial` |
| Condiciones fiscales | `GET api/condicionesfiscales` |
| Reportes | `GET api/reportes/facturacion/por-periodo`, `/desempeno-clientes`, `/desempeno-productos`, `/cartera-por-facturar` |

Todos documentados en Swagger (`/swagger`) con esquema Bearer.

### Tests
```bash
cd Backend
dotnet test tests/CoreSysHM.UnitTests
```
Cubren: cálculo de neto/IVA/total, numeración correlativa, idempotencia (sin duplicados),
facturación parcial y rechazo de sobre-facturación, reversión de stock en Notas de Crédito,
unicidad fiscal de Cliente, completitud de datos, y consistencia 0-desviación entre los
reportes y las facturas reales (`ReporteFacturacionServiceTests`).
