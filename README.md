# CoreSys HM — Sistema de Gestión Operativa

Proyecto de Fin de Año — Ingeniería en Sistemas

## Módulos
- **Stock**: gestión de productos, categorías y movimientos de inventario
- **Ventas**: registro de clientes y procesamiento de ventas
- **Facturación**: emisión y seguimiento de facturas

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
| supervisor1 | supervisor@coresyshm.com | Super123! | Supervisor |
| operador1 | operador@coresyshm.com | Oper123! | Operador |
