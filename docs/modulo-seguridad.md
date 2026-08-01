# Módulo de Seguridad — CoreSysHM

Roles + permisos granulares + auditoría de accesos, construido sobre el login existente (JWT) sin reemplazarlo. Cubre backend (.NET 8 / ASP.NET Core Identity) y frontend (Angular 22).

## 1. Diagnóstico previo (resumen)

Antes de tocar código se auditó el estado real del sistema:

- **Backend**: .NET 8, Clean Architecture (Domain/Application/Infrastructure/API). **No usaba ASP.NET Identity** — `Usuario` era una entidad custom con `PasswordHash` en BCrypt y un enum `RolUsuario` (`Administrador=1, Operador=2, Supervisor=3`) embebido, sin tabla de roles ni de permisos. `AuthController` solo tenía `POST /api/auth/login`. Autorización existente: `[Authorize(Roles = "...")]` hardcodeado en 6 controllers de negocio (Ventas, Facturas, Compras, Productos, Proveedores, Categorías). Sin refresh token, sin auditoría, sin tests.
- **Frontend**: Angular 22 con NgModules clásicos (no standalone), `AuthService` con `BehaviorSubject` + `localStorage`, un único `authGuard` funcional aplicado a nivel de rama raíz, un único interceptor de clase (maneja 401, no 403). Sin directivas de permisos, sin pantallas de administración, sin test runner configurado.
- **Base de datos**: los roles reales en producción eran `Administrador / Supervisor / Operador`, distintos de los 3 roles pedidos (`Administrador / Administrativo / Cliente`).

## 2. Decisiones de arquitectura (consultadas con el usuario)

1. **Migrar a ASP.NET Core Identity completo** (`ApplicationUser : IdentityUser<int>`, `ApplicationRole : IdentityRole<int>`, `ApplicationDbContext : IdentityDbContext<...>`) en vez de un modelo custom paralelo. Para no romper el login existente, se implementó un `IPasswordHasher<ApplicationUser>` custom (`LegacyBCryptPasswordHasher`) que verifica los hashes BCrypt viejos y devuelve `SuccessRehashNeeded`, lo que hace que Identity re-hashee automáticamente al formato nativo en el próximo login exitoso — **sin resetear ninguna contraseña**. Verificado end-to-end: los 3 usuarios sembrados (admin/supervisor1/operador1) loguean con sus passwords originales y quedan re-hasheados tras el primer login.
2. **Mapeo de roles**: `Administrador` se mantiene. `Supervisor` y `Operador` (ambos roles internos con acceso operativo similar) se **fusionan en `Administrativo`**. `Cliente` es un rol nuevo sin usuarios migrados (pensado a futuro para acceso externo restringido). Migración de datos verificada: los 3 usuarios preexistentes conservaron su `Id` (por la FK `Compra.RegistradoPorId`) y quedaron en el rol correcto.
3. **Acoplamiento Domain → Identity**: `Compra.RegistradoPor` ya apuntaba a la entidad `Usuario` de Domain. Para no tener que reescribir `MappingProfile`/`CompraDto`/`CompraService`, `ApplicationUser`/`ApplicationRole` viven en `Domain.Entities.Auth` (reemplazando a `Usuario`), y `CoreSysHM.Domain.csproj` agregó como única dependencia externa `Microsoft.Extensions.Identity.Stores` (paquete liviano, solo clases modelo, sin EF ni ASP.NET hosting). Se documenta acá como decisión consciente: Domain deja de ser 100% framework-agnostic a cambio de no tocar 3-4 archivos adicionales fuera de alcance.
4. **Creación de roles personalizados**: el enunciado pide no agregar roles adicionales a los 3 fijos, pero también pide un CRUD completo de roles ("crear nuevo rol con nombre, descripción y permisos"). Se interpretó como: **el seed nunca crea más de 3 roles**, pero el Administrador sí puede crear roles custom adicionales en runtime vía `RolesController` — es una feature explícitamente pedida en la sección de Servicios/UX. `UserManagementService` valida el rol de un usuario contra `RoleManager.RoleExistsAsync(...)`, no contra una lista fija, para que esos roles custom sean asignables.

## 3. Estructura de roles y permisos

### Roles (tabla `AspNetRoles`, entidad `ApplicationRole`)

| Rol | IsSystem | IsSeeded | Editable | Eliminable |
|---|---|---|---|---|
| Administrador | true | true | No (acceso total fijo) | No |
| Administrativo | false | true | Sí (permisos) | No |
| Cliente | false | true | Sí (permisos) | No |
| *(roles custom creados por el Admin)* | false | false | Sí (todo, incl. nombre) | Sí, si no tiene usuarios asignados |

`IsSystem` y `IsSeeded` son dos flags separados (no solo `IsSystem` como en el enunciado original) porque hacían falta 3 niveles de protección distintos: inmutable total (Administrador), protegido-pero-editable (Administrativo/Cliente), y libre (roles custom). Ver `ApplicationRole.cs`.

### Catálogo de permisos (`Domain.Security.Permissions`)

Organizado por categoría/módulo. `Permissions.All()` se calcula por reflexión sobre las clases anidadas, así que un rol Administrador (`RolePermissions.ForRole`) nunca queda desincronizado si se agrega una categoría nueva.

| Categoría | Claves |
|---|---|
| Usuarios | `usuarios.view/create/edit/delete/reset_password` |
| Roles | `roles.view/create/edit/delete` |
| Seguridad | `security.view`, `security.manage` |
| Productos | `productos.view/create/edit/delete` |
| Categorías | `categorias.view/create/edit/delete` |
| Proveedores | `proveedores.view/create/edit/delete` |
| Stock | `stock.view`, `stock.registrar` |
| Ventas | `ventas.view/create/anular` |
| Clientes | `clientes.view/create/edit` |
| Compras | `compras.view/create/anular` |
| Facturas | `facturas.view/anular` |
| Reportes | `reportes.ver`, `reportes.exportar` |

Las categorías de negocio (Productos, Ventas, Compras, etc.) están en el catálogo para que Administrativo/Cliente tengan un "grupo de acciones" real y configurable, **pero los 9 controllers de negocio existentes no se convirtieron a `[HasPermission]` en este alcance** — siguen con su `[Authorize(Roles=...)]` ya actualizado a los nuevos nombres de rol. Queda como pendiente documentado (sección 7).

### Mapa `RolePermissions` (fuente de verdad SOLO para seed/tests)

`Domain.Security.RolePermissions.ForRole(rol)`:

- **Administrador** → `Permissions.All()` completo (dinámico, nunca hardcodeado — así nunca pierde acceso total).
- **Administrativo** → todo lo operativo: `productos.*`, `categorias.*`, `proveedores.*`, `stock.*`, `ventas.*`, `clientes.*`, `compras.*`, `facturas.*`, `reportes.ver/exportar`. Nada de `usuarios.*`/`roles.*`/`security.*`.
- **Cliente** → catálogo vacío por defecto (rol nuevo sin caso de uso todavía; el Administrador lo configura cuando exista una necesidad real).

**Importante**: en runtime (login, autorización), los permisos efectivos de un rol se leen **siempre** de `ApplicationRole.Permissions` (columna JSON editable por el Administrador vía `RoleManagementService`), nunca de la clase estática `RolePermissions`. Esta última solo alimenta:
1. El seed inicial (migración `SeedRolesAndMigrateUsers` + `DbInitializer.SeedRolesAsync` como fallback idempotente).
2. Los tests unitarios de "cuáles son los permisos default esperados de cada rol".

`RolePermissions.RoleGrants(rol, permiso)`: Administrador siempre `true`; para el resto, `ForRole(rol).Contains(permiso)`.

## 4. Autorización (backend)

- **JWT**: claims `sub`, `email`, `name`, uno o más `ClaimTypes.Role` (compatibilidad con `[Authorize(Roles=...)]` existente), y un claim `"permission"` repetido por cada permiso efectivo del rol (patrón estándar para "arrays" en JWT).
- **`[HasPermission("clave")]`**: azúcar sintáctico sobre `[Authorize(Policy = "Permission:clave")]`. `PermissionPolicyProvider` (`IAuthorizationPolicyProvider`) arma la política al vuelo para cualquier policy con prefijo `"Permission:"`, sin necesidad de registrar cada clave del catálogo en `Program.cs`. `PermissionAuthorizationHandler` aprueba si `User.IsInRole("Administrador")` (chequeo por **rol**, no por presencia individual del permiso — más robusto ante ediciones futuras del catálogo) **o** si el token trae el claim `permission` correspondiente.
- Registro en DI: `AddIdentityCore<ApplicationUser>()` (no `AddIdentity<>()`, que registraría su propio esquema de cookies y pisaría el `AddAuthentication(JwtBearerDefaults...)` ya existente).

### Endpoints protegidos

| Controller | Permiso requerido |
|---|---|
| `POST /api/auth/login` | público |
| `POST /api/auth/logout` | `[Authorize]` (cualquier autenticado) |
| `GET/POST/PUT /api/users`, `PATCH .../activo`, `POST .../reset-password` | `usuarios.view/create/edit/reset_password` |
| `GET /api/users/me`, `POST /api/users/me/cambiar-password` | `[Authorize]` (cualquier autenticado, sobre sí mismo) |
| `GET /api/roles`, `GET .../catalogo-permisos` | `security.view` |
| `POST/PUT/DELETE /api/roles` | `security.manage` |
| `GET /api/auditoria` | `security.view` |
| Ventas/Facturas/Compras (anular), Productos/Proveedores/Categorías (delete) | `[Authorize(Roles="Administrador,Administrativo")]` o `[Authorize(Roles="Administrador")]` (strings actualizados, **sin** convertir a permisos granulares — pendiente) |

## 5. Reglas de negocio implementadas

- No se puede desactivar ni reasignar el rol del único Administrador activo (`UserManagementService.ContarAdministradoresActivosAsync`).
- Usuario inactivo no puede loguear (`AuthService.LoginAsync` chequea `IsActive`).
- Rol de sistema (`IsSystem=true`) no editable ni eliminable; rol protegido (`IsSeeded=true`) editable pero no eliminable; rol con usuarios asignados no eliminable (`RoleManagementService`).
- Nombre de rol duplicado rechazado (`RoleManager.CreateAsync` + chequeo explícito de `FindByNameAsync`).
- Auditoría de `Login`, `LoginFallido`, `Logout`, `ResetPassword` — sin método de borrado en ningún nivel (`IAuditoriaService` no expone `Delete`).

## 6. Frontend: rutas y guards

| Ruta | Guard | Permiso (`data.permission`) |
|---|---|---|
| `/auth/login` | — | público |
| `/dashboard`, `/stock`, `/ventas`, `/compras`, `/facturacion`, `/reportes*` | `authGuard` (heredado de la rama raíz `''`) | — |
| `/usuarios` | `authGuard` + `permissionGuard` | `usuarios.view` |
| `/roles` | `authGuard` + `permissionGuard` | `security.view` |
| `/auditoria` | `authGuard` + `permissionGuard` | `security.view` |
| `/perfil`, `/mi-area` | `authGuard` | — (cualquier autenticado) |
| `/acceso-denegado` | `authGuard` | — (sin permission, debe ser alcanzable siempre) |

- `PermissionService` (nuevo, separado de `AuthService`) se suscribe a `currentUser$` y expone `has(permiso)`; Administrador siempre `true`.
- `permissionGuard`: si `!permissionService.has(permiso)` → redirige a `/acceso-denegado`.
- `*appHasPermission` (en `SharedModule`, nueva carpeta `shared/` — no existía): oculta el elemento (no solo lo deshabilita) si falta el permiso; sin permiso especificado en el binding, no restringe.
- `AuthInterceptor` extendido: 401 → `logout()` (comportamiento preexistente, sin tocar); 403 → `router.navigate(['/acceso-denegado'])` (nuevo).
- Redirección post-login por rol: en `LoginComponent` (no en un guard), porque el dato ya está disponible sincrónicamente en la respuesta de login — `Cliente → /mi-area`, cualquier otro rol → `/dashboard`.

## 7. Pendientes / fuera de alcance de esta entrega

- **Refresh token**: no se implementó (no era bloqueante para el diagnóstico original; el JWT sigue expirando a las `Jwt:ExpirationHours` configuradas, 24h por defecto).
- **Conversión de los 9 controllers de negocio a `[HasPermission]` granular**: hoy siguen con `[Authorize(Roles=...)]` (ya actualizado a los 3 roles nuevos). El catálogo de permisos de negocio ya existe y está listo para ese trabajo futuro.
- **JWT `Jwt:Key` hardcodeada en `appsettings.json`** (preexistente, no introducido por este trabajo): mover a variable de entorno o User Secrets.
- **Build de producción de Angular** sin `environment.prod.ts`/`fileReplacements` (preexistente): un build `--configuration production` seguiría apuntando a `http://localhost:5000/api`.
- **Vulnerabilidad conocida en AutoMapper 12.0.1** (`NU1903`, preexistente): reportada por NuGet en `dotnet test`/`dotnet build`, no introducida por este trabajo.
- La directiva `*appHasPermission` evalúa una sola vez en `ngOnInit` — si el Administrador revoca un permiso en caliente, el usuario afectado lo ve reflejado recién en su próximo login (no hay refresco reactivo mid-sesión). Aceptable para este alcance.

## 8. Credenciales del Administrador inicial

Configurables por variable de entorno (con defaults de desarrollo si no se setean):

```
InitialAdmin__Email=admin@coresyshm.com
InitialAdmin__Password=Admin123!
InitialAdmin__UserName=admin
```

`DbInitializer.SeedAdminAsync` solo crea el usuario si no existe ya alguien con el rol Administrador (idempotente en cada arranque).

## 9. Resultado de tests

### Backend — `dotnet test tests\CoreSysHM.UnitTests` → **22/22 OK**

- `RolePermissionsTests` (5): defaults de Administrador/Administrativo/Cliente + `RoleGrants`.
- `UsuarioManagementTests` (5): crear OK, email duplicado, password sin requisitos, desactivar único Administrador activo, login de usuario inactivo.
- `RoleManagementTests` (6): crear rol personalizado, nombre duplicado, eliminar rol de sistema, eliminar rol protegido, eliminar rol con usuarios asignados, editar permisos de rol no-sistema.
- `PermissionAuthorizationHandlerTests` (3): Administrador accede siempre, permiso justo autoriza, permiso no asignado deniega (equivalente backend de "guards y permisos").
- `AuditoriaTests` (3): login exitoso, login fallido y reset de password quedan registrados.

Infraestructura de test: Sqlite in-memory vía `EnsureCreated()` (no las migraciones reales, que tienen SQL específico de SQL Server), con el mismo `AddIdentityCore` + `LegacyBCryptPasswordHasher` que producción — xUnit crea una instancia nueva de cada clase de test por `[Fact]`, así que cada test tiene su propia BD aislada sin necesidad de un fixture compartido.

### Frontend — `npm run test:ci` → **14/14 OK**

- `authGuard` (2): sin token → redirige a login; con token → permite acceso.
- `permissionGuard` (4): sin permiso → 403/acceso-denegado; con permiso → OK; Administrador siempre OK; Cliente sin permiso administrativo → 403.
- `PermissionService` (3): Administrador tiene cualquier permiso; rol normal solo los del token; sin usuario → ningún permiso.
- `AuthInterceptor` (3): adjunta Bearer token; 401 → logout; 403 → redirige a acceso-denegado sin logout.
- `HasPermissionDirective` (2): con permiso renderiza, sin permiso oculta el elemento.

## 10. Verificación manual realizada

- Login de los 3 usuarios preexistentes con sus passwords originales (BCrypt) — confirmado re-hash automático a formato Identity nativo tras el primer login exitoso.
- `GET /api/productos` con rol Administrativo → 200 OK (controllers de negocio no rotos).
- `GET /api/users` con rol Administrativo (sin `usuarios.view`) → 403 Forbidden.
- Login con password incorrecta → 401 + registro `LoginFallido` en `AuditoriaAcceso`.
- `dotnet build` de toda la solución y `ng build` del frontend, ambos sin errores.

No se hizo verificación manual en navegador de las pantallas Angular nuevas (usuarios/roles/auditoría/perfil) — se validó que compilan, lintean y that el bundle se genera correctamente, pero no se ejecutó un recorrido interactivo real en browser.
