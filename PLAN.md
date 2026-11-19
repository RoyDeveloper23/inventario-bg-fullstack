# Plan Maestro de Desarrollo

## 1. Objetivo y estado actual

Este plan lleva el repositorio desde su scaffold actual hasta un sistema web de inventario funcional, con Angular 19, .NET 8 Web API y PostgreSQL en Supabase.

El repositorio contiene:

- `frontend/`: aplicación Angular 19 standalone con TypeScript estricto. Solo están implementados el componente de bienvenida y una prueba de scaffold; las rutas están vacías. Tailwind CSS todavía no aparece en las dependencias.
- `backend/InventarioBg`: solución .NET 8 con los proyectos `InventarioBg.Domain`, `InventarioBg.Application`, `InventarioBg.Infrastructure` e `InventarioBg.API`. Los tres primeros no tienen archivos de código fuente todavía. La API solo configura controladores y Swagger.
- `.github/copilot-instructions.md`: especifica la arquitectura por capas, nombres de negocio en español, DTOs camelCase, Tailwind, JWT, EF Core, Serilog y middleware global. Es la guía para las decisiones de implementación de este plan.
- No se encontraron migraciones, entidades, endpoints funcionales ni pruebas de backend.

## 2. Alcance y decisiones

- El backend es la autoridad para validación, reglas de negocio, permisos, paginación y stock.
- Solo el login es anónimo. No se publica registro de usuarios; las cuentas se aprovisionan fuera de la API. No incluir credenciales por defecto ni secretos en el repositorio.
- El stock representa el saldo actual por producto, proveedor y lote. No se incorpora un libro de movimientos ni auditoría de cada ajuste en esta primera versión.
- El producto maestro no se duplica por proveedor o lote. Los precios y cantidades se guardan en las filas de `ProductoInventario`.
- El catálogo de proveedores es de consulta en la aplicación. La creación y mantenimiento inicial de proveedores se realiza por un proceso administrativo controlado, no por una pantalla adicional.
- El borrado de un producto es lógico (`Activo = false`) para conservar sus relaciones e historial; el borrado de un lote se permite únicamente con stock cero.
- Los IDs serán UUID (`Guid` en .NET y `uuid` en PostgreSQL). Los precios usan `decimal(18,2)` y el stock usa enteros no negativos.
- La UI consume respuestas JSON serializadas en camelCase. Los nombres de las propiedades documentados en los contratos de este archivo son normativos.
- El token de acceso se conserva en `sessionStorage` en la versión inicial; no se guardan contraseñas ni refresh tokens. La mitigación de XSS sigue siendo necesaria; una política de seguridad más estricta puede migrar a cookies `HttpOnly` si se incorpora soporte de cookies en la API.

## 3. Arquitectura propuesta

### Backend

Mantener los cuatro proyectos existentes y limitar las dependencias a:

```text
InventarioBg.API
  ├── InventarioBg.Application ──> InventarioBg.Domain
  └── InventarioBg.Infrastructure ──> InventarioBg.Application
                                      └──> InventarioBg.Domain
```

- **Domain**: entidades y reglas invariantes puras (`Usuario`, `Producto`, `Proveedor`, `ProductoInventario`). No depende de EF Core, ASP.NET ni PostgreSQL.
- **Application**: DTOs de entrada/salida, interfaces de servicios y persistencia, validaciones y casos de uso. Depende únicamente de Domain.
- **Infrastructure**: `AppDbContext`, configuraciones Fluent API, repositorios EF Core y adaptadores de seguridad (BCrypt y generación de JWT). Implementa abstracciones de Application.
- **API**: controladores delgados, autenticación/autorización, configuración de DI, Swagger, CORS y middleware. Traduce HTTP a casos de uso; no contiene consultas EF ni reglas de negocio.
- **Repositorios**: utilizarlos para encapsular persistencia y consultas necesarias por los casos de uso, no como una capa genérica con métodos CRUD sin valor. Las lecturas de catálogo deben filtrar, paginar y proyectar a DTO en SQL.
- **Servicios**: `AuthService`, `ProductoService`, `ProveedorService` y `ProductoInventarioService`; inyectar interfaces y registrar implementaciones en extensiones de DI de cada capa.
- **Validación**: DataAnnotations en DTOs para formato/rangos y servicios de Application para reglas que requieren datos (existencia, SKU duplicado, lote duplicado, stock no negativo). Responder `400` o `409` según corresponda.
- **EF Core**: Npgsql, `AsNoTracking()` en lectura, proyección a DTO, paginación en servidor, transacciones para operaciones relacionadas y manejo de concurrencia al modificar stock.
- **Errores**: middleware global que devuelva `ProblemDetails` en camelCase y códigos semánticos (`400`, `401`, `404`, `409`, `500`); no incluir stack traces ni detalles internos en producción.
- **Autenticación**: JWT Bearer, claves y expiración desde configuración segura; BCrypt para verificar hashes de contraseña. Todas las rutas de negocio requieren `[Authorize]`.
- **Configuración**: connection string, clave JWT y orígenes CORS por variables de entorno/User Secrets. No guardar secretos en `appsettings*.json`.
- **Observabilidad**: Serilog con logging de solicitudes, consola y archivo rotativo para desarrollo/servidores con disco persistente; no registrar contraseñas, tokens ni connection strings.
- **Swagger**: describir operaciones y respuestas; configurar esquema Bearer para permitir probar rutas protegidas.

### Frontend

Organizar por responsabilidades y funcionalidades dentro de `frontend/src/app/`:

- `core/`: configuración API, modelos compartidos, `AuthService`, guard, interceptores HTTP y servicio de notificaciones. Instancias singleton.
- `shared/`: elementos reutilizables accesibles y responsivos, como toast, indicador de carga y componentes de formulario.
- `layouts/`: shell autenticado con navegación y contenedor de rutas.
- `features/auth/`: pantalla de login y modelo de autenticación.
- `features/productos/`: listado/búsqueda/paginación y formulario standalone reactivo para crear/editar producto.
- `features/inventario/`: desglose y edición de existencias por proveedor/lote desde el producto.
- Usar rutas lazy-loaded, guard funcional para rutas privadas e interceptor funcional que adjunta el Bearer token y maneja `401`.
- Usar Reactive Forms con validaciones coincidentes con la API. El interceptor de errores convierte `ProblemDetails` en mensajes localizados; el servicio de notificaciones los presenta sin duplicar lógica en cada pantalla.
- Mantener la sesión en un único `AuthService`; al expirar o recibir `401`, limpiar sesión y redirigir al login.
- Mantener las interfaces TypeScript alineadas exactamente con los DTOs JSON de la sección 6.
- Aplicar Tailwind CSS conforme a las instrucciones del repositorio y verificar comportamiento en móvil y escritorio.

### Base de datos y relaciones

```text
Usuario (1)                         (sin relaciones de negocio en v1)

Producto (1) ──────── (N) ProductoInventario (N) ──────── (1) Proveedor
```

| Tabla | Columnas principales | Restricciones e índices |
|---|---|---|
| `usuarios` | `id uuid PK`, `nombre`, `email`, `email_normalizado`, `password_hash`, `activo` | `email_normalizado` único; campos obligatorios; hash BCrypt, nunca contraseña en texto plano. |
| `productos` | `id uuid PK`, `codigo_sku`, `nombre`, `descripcion NULL`, `categoria NULL`, `activo` | SKU único; índices en `codigo_sku` y `nombre`; SKU normalizado antes de persistir. |
| `proveedores` | `id uuid PK`, `nombre`, `activo` | Nombre obligatorio; índice de búsqueda por nombre. |
| `productos_inventario` | `id uuid PK`, `producto_id FK`, `proveedor_id FK`, `numero_lote`, `precio_unitario numeric(18,2)`, `stock_disponible integer`, `fecha_ingreso timestamptz` | FKs con índice; `precio_unitario >= 0`; `stock_disponible >= 0`; identidad de lote única por `(producto_id, proveedor_id, numero_lote)`; campos requeridos. |

Reglas de integridad:

- Un producto y un proveedor pueden tener muchos lotes; cada fila de inventario pertenece a exactamente un producto y un proveedor.
- No borrar físicamente productos/proveedores referenciados; desactivarlos. La FK del lote debe restringir borrados accidentales.
- Rechazar precios negativos, stock negativo, SKU/correo duplicados y combinaciones repetidas de producto-proveedor-lote.
- Crear y actualizar la fila del lote en una transacción. Al ajustar cantidades, evitar condiciones de carrera y confirmar que el saldo final no sea negativo.
- Guardar fechas UTC. Los valores monetarios no deben convertirse a `float`/`double`.
- Los índices iniciales deben cubrir unicidad, FKs y búsquedas principales; no añadir índices de texto avanzados sin medir consultas reales.
- Los registros de lote son el saldo vigente, no un historial inmutable de entradas/salidas. Si se exige trazabilidad de movimientos, diseñar una tabla separada después de validar ese requisito.

## 4. Contratos de API

Base: `/api`. Todas las rutas excepto `POST /api/auth/login` requieren `Authorization: Bearer <accessToken>`. El API devuelve JSON camelCase. Las respuestas de error usan `ProblemDetails` con `status`, `title`, `detail` y opcionalmente `errors`.

### Autenticación

`POST /api/auth/login` (anónimo)

Request:

```json
{
  "email": "persona@ejemplo.com",
  "password": "contraseña"
}
```

Response `200`:

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-11-19T03:00:00Z",
  "usuario": {
    "id": "00000000-0000-0000-0000-000000000001",
    "nombre": "Nombre",
    "email": "persona@ejemplo.com"
  }
}
```

Credenciales inválidas o usuario inactivo: `401` con error genérico, sin revelar cuál dato falló.

### Proveedores

`GET /api/proveedores` — `200`:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "nombre": "Proveedor Uno"
    }
  ]
}
```

Solo proveedores activos se presentan para nuevos lotes.

### Productos

`GET /api/productos?busqueda=texto&proveedorId=<uuid>&page=1&pageSize=20` — `200`. Filtros opcionales; valores predeterminados `page=1`, `pageSize=20`, máximo `100`. Devuelve solo productos activos. `proveedorId` filtra productos con inventario de ese proveedor.

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000003",
      "codigoSku": "SKU-001",
      "nombre": "Producto",
      "descripcion": "Descripción",
      "categoria": "General",
      "activo": true,
      "stockTotal": 12
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

`GET /api/productos/{id}` — `200` con un objeto producto (`id`, `codigoSku`, `nombre`, `descripcion`, `categoria`, `activo`); `404` si no existe o no está disponible.

`POST /api/productos` — `201` y `Location: /api/productos/{id}`. Request:

```json
{
  "codigoSku": "SKU-001",
  "nombre": "Producto",
  "descripcion": "Descripción",
  "categoria": "General"
}
```

`PUT /api/productos/{id}` — `200` con el producto actualizado. Request con los mismos cuatro campos que el POST. SKU duplicado: `409`.

`DELETE /api/productos/{id}` — `204`; desactiva el producto sin borrar sus lotes. `404` si no existe.

### Inventario por proveedor y lote

`GET /api/productos/{productoId}/inventarios` — `200`:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000004",
      "productoId": "00000000-0000-0000-0000-000000000003",
      "proveedorId": "00000000-0000-0000-0000-000000000002",
      "proveedorNombre": "Proveedor Uno",
      "numeroLote": "LOTE-2026-01",
      "precioUnitario": 14.5,
      "stockDisponible": 12,
      "fechaIngreso": "2026-11-18T18:00:00Z"
    }
  ]
}
```

`POST /api/productos/{productoId}/inventarios` — `201` y `Location` del lote. Request:

```json
{
  "proveedorId": "00000000-0000-0000-0000-000000000002",
  "numeroLote": "LOTE-2026-01",
  "precioUnitario": 14.5,
  "stockDisponible": 12
}
```

La API asigna `id` y `fechaIngreso` UTC. Producto/proveedor inexistente: `404`; lote repetido, precio o stock inválido: `409` o `400` según sea conflicto o validación.

`PUT /api/productos/{productoId}/inventarios/{inventarioId}` — `200` con el lote actualizado. Request:

```json
{
  "precioUnitario": 15.25,
  "stockDisponible": 10
}
```

El producto, proveedor y número de lote de una fila existente no se cambian; para otra combinación se crea otro lote. Stock o precio negativos: `400`; IDs que no coinciden: `404`.

`DELETE /api/productos/{productoId}/inventarios/{inventarioId}` — `204` solo si el stock es cero; `409` si aún tiene existencias y `404` si no existe.

### Convenciones de respuesta

- `400`: DTO/formato inválido o parámetros fuera de rango.
- `401`: token ausente, inválido/expirado o login fallido.
- `404`: recurso inexistente.
- `409`: SKU/lote duplicado, intento de borrar lote con stock o conflicto concurrente.
- `500`: error no esperado, mensaje genérico y detalle registrado en logs.
- Nunca retornar hashes de contraseña, claves JWT, tokens en logs ni detalles del servidor.

## 5. Roadmap por fases

Cada tarea es un cambio acotado. Los paths son relativos a la raíz del repositorio; las migraciones EF indicadas se generan con EF Core y se incorporan al control de versiones.

### Fase 1 — Estructura de aplicación y configuración segura

- [x] Configurar registro de servicios de Application e Infrastructure sin introducir dependencias de infraestructura en Domain/Application. Archivos: `backend/InventarioBg.Application/DependencyInjection.cs` (crear), `backend/InventarioBg.Infrastructure/DependencyInjection.cs` (crear), `backend/InventarioBg.API/Program.cs` (modificar).
- [x] Configurar el proveedor desde Infrastructure usando la referencia Npgsql ya existente y agregar el sink Serilog de consola si no está disponible transitivamente. Archivos: `backend/InventarioBg.Infrastructure/DependencyInjection.cs` (modificar), `backend/InventarioBg.API/InventarioBg.API.csproj` (modificar solo si falta `Serilog.Sinks.Console`).
- [x] Definir configuración tipada para connection string, JWT, CORS y expiración, sin valores secretos en archivos versionados. Archivos: `backend/InventarioBg.Infrastructure/Configuration/JwtSettings.cs` (crear), `backend/InventarioBg.API/appsettings.json` (modificar), `backend/InventarioBg.API/appsettings.Development.json` (modificar).
- [x] Configurar Serilog con salida a consola y rolling file, request logging y exclusión de datos sensibles. Archivos: `backend/InventarioBg.API/Program.cs` (modificar), `backend/InventarioBg.API/appsettings.json` (modificar).
- [x] Documentar variables locales y de despliegue (sin credenciales reales), incluyendo connection string Supabase y origen CORS. Archivos: `README.md` (crear).

### Fase 2 — Dominio y persistencia PostgreSQL

- [x] Crear entidades con UUID, propiedades requeridas y reglas invariantes básicas. Archivos: `backend/InventarioBg.Domain/Entities/Usuario.cs` (crear), `backend/InventarioBg.Domain/Entities/Producto.cs` (crear), `backend/InventarioBg.Domain/Entities/Proveedor.cs` (crear), `backend/InventarioBg.Domain/Entities/ProductoInventario.cs` (crear).
- [x] Definir `AppDbContext` y configuraciones Fluent API para tablas, columnas, precisión decimal, longitudes, relaciones, restricciones, índices y borrado restringido. Archivos: `backend/InventarioBg.Infrastructure/Data/AppDbContext.cs` (crear), `backend/InventarioBg.Infrastructure/Data/Configurations/UsuarioConfiguration.cs` (crear), `backend/InventarioBg.Infrastructure/Data/Configurations/ProductoConfiguration.cs` (crear), `backend/InventarioBg.Infrastructure/Data/Configurations/ProveedorConfiguration.cs` (crear), `backend/InventarioBg.Infrastructure/Data/Configurations/ProductoInventarioConfiguration.cs` (crear), `backend/InventarioBg.Infrastructure/DependencyInjection.cs` (modificar).
- [x] Agregar interfaces de persistencia únicamente para las consultas y escrituras requeridas por los casos de uso. Archivos: `backend/InventarioBg.Application/Abstractions/Persistence/IUsuarioRepository.cs` (crear), `backend/InventarioBg.Application/Abstractions/Persistence/IProductoRepository.cs` (crear), `backend/InventarioBg.Application/Abstractions/Persistence/IProveedorRepository.cs` (crear), `backend/InventarioBg.Application/Abstractions/Persistence/IProductoInventarioRepository.cs` (crear).
- [x] Implementar repositorios EF Core con lectura no rastreada, proyecciones, búsqueda/paginación en servidor y cambios de inventario transaccionales. Archivos: `backend/InventarioBg.Infrastructure/Repositories/UsuarioRepository.cs` (crear), `backend/InventarioBg.Infrastructure/Repositories/ProductoRepository.cs` (crear), `backend/InventarioBg.Infrastructure/Repositories/ProveedorRepository.cs` (crear), `backend/InventarioBg.Infrastructure/Repositories/ProductoInventarioRepository.cs` (crear), `backend/InventarioBg.Infrastructure/DependencyInjection.cs` (modificar).
- [x] Crear y aplicar la migración inicial usando una base de datos de desarrollo, y verificar las cuatro tablas, constraints e índices en PostgreSQL. Archivos: `backend/InventarioBg.Infrastructure/Data/Migrations/<timestamp>_InitialSchema.cs` (generado), `backend/InventarioBg.Infrastructure/Data/Migrations/<timestamp>_InitialSchema.Designer.cs` (generado), `backend/InventarioBg.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs` (generado).

### Fase 3 — Contratos, servicios y seguridad de API

- [ ] Crear DTOs de autenticación, producto, proveedor, inventario, paginación y errores con nombres serializados compatibles con la sección 4. Archivos: `backend/InventarioBg.Application/Contracts/Auth/LoginRequestDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Auth/LoginResponseDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Productos/ProductoDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Productos/ProductoRequestDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Productos/ProductoListItemDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Proveedores/ProveedorDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Inventario/ProductoInventarioDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Inventario/CrearProductoInventarioDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Inventario/ActualizarProductoInventarioDto.cs` (crear), `backend/InventarioBg.Application/Contracts/Common/PagedResultDto.cs` (crear).
- [ ] Implementar servicios/interfaces de aplicación para login, producto, proveedores y lotes; validar duplicados, estado de usuario/producto y rangos de stock/precio. Archivos: `backend/InventarioBg.Application/Abstractions/Services/IAuthService.cs` (crear), `backend/InventarioBg.Application/Abstractions/Services/IProductoService.cs` (crear), `backend/InventarioBg.Application/Abstractions/Services/IProveedorService.cs` (crear), `backend/InventarioBg.Application/Abstractions/Services/IProductoInventarioService.cs` (crear), `backend/InventarioBg.Application/Services/AuthService.cs` (crear), `backend/InventarioBg.Application/Services/ProductoService.cs` (crear), `backend/InventarioBg.Application/Services/ProveedorService.cs` (crear), `backend/InventarioBg.Application/Services/ProductoInventarioService.cs` (crear), `backend/InventarioBg.Application/DependencyInjection.cs` (modificar).
- [ ] Implementar hashing/verificación BCrypt y generación de JWT con `sub`, nombre y correo, expiración y clave obtenida de configuración. Archivos: `backend/InventarioBg.Application/Abstractions/Security/IPasswordHasher.cs` (crear), `backend/InventarioBg.Application/Abstractions/Security/IJwtTokenService.cs` (crear), `backend/InventarioBg.Infrastructure/Security/BCryptPasswordHasher.cs` (crear), `backend/InventarioBg.Infrastructure/Security/JwtTokenService.cs` (crear), `backend/InventarioBg.Infrastructure/DependencyInjection.cs` (modificar).
- [ ] Configurar JWT Bearer, política de CORS explícita para Angular y autorización antes de mapear controladores. Archivos: `backend/InventarioBg.API/Program.cs` (modificar), `backend/InventarioBg.API/appsettings.json` (modificar).
- [ ] Implementar middleware global que convierta errores conocidos y excepciones imprevistas a ProblemDetails sin filtrar información interna. Archivos: `backend/InventarioBg.API/Middleware/ExceptionHandlingMiddleware.cs` (crear), `backend/InventarioBg.API/Program.cs` (modificar).
- [ ] Implementar controladores con rutas, autorización, códigos HTTP y respuestas alineados a la sección 4. Archivos: `backend/InventarioBg.API/Controllers/AuthController.cs` (crear), `backend/InventarioBg.API/Controllers/ProductosController.cs` (crear), `backend/InventarioBg.API/Controllers/ProveedoresController.cs` (crear).
- [ ] Configurar Swagger con esquema Bearer y anotaciones de respuestas/contratos. Archivos: `backend/InventarioBg.API/Program.cs` (modificar).
- [ ] Retirar la petición de ejemplo de clima y sustituirla por ejemplos HTTP útiles para login y endpoints protegidos, sin credenciales reales. Archivos: `backend/InventarioBg.API/InventarioBg.API.http` (modificar).

### Fase 4 — Base Angular, sesión y errores centralizados

- [ ] Incorporar Tailwind CSS siguiendo la integración compatible con Angular CLI 19 y dejar preparada la hoja de estilos global. Archivos: `frontend/package.json` (modificar), `frontend/package-lock.json` (modificar), `frontend/src/styles.css` (modificar), `frontend/postcss.config.json` (crear).
- [ ] Configurar URL base del API para desarrollo/producción y registrar interceptores HTTP funcionales. Archivos: `frontend/src/environments/environment.ts` (crear), `frontend/src/environments/environment.development.ts` (crear), `frontend/angular.json` (modificar), `frontend/src/app/app.config.ts` (modificar).
- [ ] Definir modelos TypeScript para DTOs de login, usuario, errores, productos, lotes, proveedores y paginación; verificar correspondencia camelCase. Archivos: `frontend/src/app/core/models/auth.models.ts` (crear), `frontend/src/app/core/models/api-error.models.ts` (crear), `frontend/src/app/features/productos/models/producto.models.ts` (crear), `frontend/src/app/features/inventario/models/inventario.models.ts` (crear), `frontend/src/app/core/models/proveedor.models.ts` (crear), `frontend/src/app/core/models/paged-result.models.ts` (crear).
- [ ] Implementar servicio de autenticación con login/logout, estado de sesión y almacenamiento temporal del token en sessionStorage. Archivos: `frontend/src/app/core/services/auth.service.ts` (crear).
- [ ] Implementar guard funcional que redirija a login y conserve la ruta destino sin permitir acceso a páginas protegidas. Archivos: `frontend/src/app/core/guards/auth.guard.ts` (crear).
- [ ] Implementar interceptor JWT para añadir Bearer solo a peticiones del API y limpiar sesión al recibir `401`. Archivos: `frontend/src/app/core/interceptors/jwt.interceptor.ts` (crear), `frontend/src/app/app.config.ts` (modificar).
- [ ] Implementar tratamiento centralizado de errores API y notificaciones accesibles, evitando mensajes duplicados por componente. Archivos: `frontend/src/app/core/interceptors/error.interceptor.ts` (crear), `frontend/src/app/core/services/notification.service.ts` (crear), `frontend/src/app/shared/components/toast/toast.component.ts` (crear), `frontend/src/app/shared/components/toast/toast.component.html` (crear), `frontend/src/app/shared/components/toast/toast.component.css` (crear), `frontend/src/app/app.config.ts` (modificar).
- [ ] Reemplazar el scaffold de bienvenida por el shell principal, rutas lazy-loaded públicas/privadas y layout con navegación responsive. Archivos: `frontend/src/app/app.component.ts` (modificar), `frontend/src/app/app.component.html` (modificar), `frontend/src/app/app.component.css` (modificar), `frontend/src/app/app.routes.ts` (modificar), `frontend/src/app/layouts/main-layout/main-layout.component.ts` (crear), `frontend/src/app/layouts/main-layout/main-layout.component.html` (crear), `frontend/src/app/layouts/main-layout/main-layout.component.css` (crear).
- [ ] Crear formulario reactivo de login con validación, mensajes localizados, estado de envío y navegación tras éxito. Archivos: `frontend/src/app/features/auth/login-page/login-page.component.ts` (crear), `frontend/src/app/features/auth/login-page/login-page.component.html` (crear), `frontend/src/app/features/auth/login-page/login-page.component.css` (crear).

### Fase 5 — Catálogo y CRUD de productos

- [ ] Implementar cliente HTTP tipado para listar, obtener, crear, actualizar y desactivar productos con filtros/paginación. Archivos: `frontend/src/app/features/productos/services/producto.service.ts` (crear).
- [ ] Construir listado responsive con búsqueda, filtro de proveedor, paginación, carga/vacío/error y acceso al detalle del stock. Archivos: `frontend/src/app/features/productos/product-list/product-list.component.ts` (crear), `frontend/src/app/features/productos/product-list/product-list.component.html` (crear), `frontend/src/app/features/productos/product-list/product-list.component.css` (crear).
- [ ] Construir formulario reactivo reutilizable para crear/editar producto con validaciones alineadas a los DTOs. Archivos: `frontend/src/app/features/productos/product-form/product-form.component.ts` (crear), `frontend/src/app/features/productos/product-form/product-form.component.html` (crear), `frontend/src/app/features/productos/product-form/product-form.component.css` (crear).
- [ ] Añadir rutas de catálogo protegidas y navegación entre listado/formulario/detalle sin romper navegación directa por URL. Archivos: `frontend/src/app/app.routes.ts` (modificar).
- [ ] Sustituir pruebas de scaffold que asumen pantalla Angular inicial y añadir pruebas de servicio/formulario para flujos CRUD y errores de validación. Archivos: `frontend/src/app/app.component.spec.ts` (modificar), `frontend/src/app/features/productos/product-form/product-form.component.spec.ts` (crear), `frontend/src/app/features/productos/services/producto.service.spec.ts` (crear).

### Fase 6 — Proveedores y existencias por lote

- [ ] Implementar servicios HTTP tipados de consulta de proveedores y operaciones de lotes, con tipos alineados a sección 4. Archivos: `frontend/src/app/core/services/proveedor.service.ts` (crear), `frontend/src/app/features/inventario/services/inventario.service.ts` (crear).
- [ ] Construir vista de detalle de producto que muestre lotes, proveedor, precio, cantidad y fecha de ingreso, con estados de carga/vacío/error. Archivos: `frontend/src/app/features/inventario/product-inventory-page/product-inventory-page.component.ts` (crear), `frontend/src/app/features/inventario/product-inventory-page/product-inventory-page.component.html` (crear), `frontend/src/app/features/inventario/product-inventory-page/product-inventory-page.component.css` (crear).
- [ ] Implementar formulario reactivo de alta/edición de lote con selector de proveedor, validaciones de precio/stock y confirmación de eliminación condicionada a stock cero. Archivos: `frontend/src/app/features/inventario/inventory-form/inventory-form.component.ts` (crear), `frontend/src/app/features/inventario/inventory-form/inventory-form.component.html` (crear), `frontend/src/app/features/inventario/inventory-form/inventory-form.component.css` (crear).
- [ ] Añadir ruta protegida de inventario y acciones de navegación desde el catálogo. Archivos: `frontend/src/app/app.routes.ts` (modificar), `frontend/src/app/features/productos/product-list/product-list.component.html` (modificar).
- [ ] Añadir pruebas focalizadas para modelos de respuesta, consulta de proveedores y validación/errores del formulario de lotes. Archivos: `frontend/src/app/core/services/proveedor.service.spec.ts` (crear), `frontend/src/app/features/inventario/inventory-form/inventory-form.component.spec.ts` (crear), `frontend/src/app/features/inventario/services/inventario.service.spec.ts` (crear).

### Fase 7 — Pruebas, integración y entrega

- [ ] Crear proyecto de pruebas .NET, agregarlo a la solución y probar reglas de servicio (login, SKU, paginación, lote único, stock/precio no negativos). Archivos: `backend/InventarioBg.Tests/InventarioBg.Tests.csproj` (crear), `backend/InventarioBg.Tests/Services/AuthServiceTests.cs` (crear), `backend/InventarioBg.Tests/Services/ProductoServiceTests.cs` (crear), `backend/InventarioBg.Tests/Services/ProductoInventarioServiceTests.cs` (crear), `backend/InventarioBg.sln` (modificar).
- [ ] Probar contratos HTTP de login, autorización, validación y ProblemDetails con base de datos de pruebas aislada; no apuntar pruebas a Supabase productivo. Archivos: `backend/InventarioBg.API.Tests/InventarioBg.API.Tests.csproj` (crear), `backend/InventarioBg.API.Tests/AuthEndpointsTests.cs` (crear), `backend/InventarioBg.API.Tests/ProductosEndpointsTests.cs` (crear), `backend/InventarioBg.API.Tests/InventarioEndpointsTests.cs` (crear), `backend/InventarioBg.sln` (modificar).
- [ ] Añadir pruebas de guard, interceptor de errores y login, incluyendo sesión expirada y respuesta `401`. Archivos: `frontend/src/app/core/guards/auth.guard.spec.ts` (crear), `frontend/src/app/core/interceptors/error.interceptor.spec.ts` (crear), `frontend/src/app/features/auth/login-page/login-page.component.spec.ts` (crear).
- [ ] Verificar build y tests del frontend, build/test .NET, migración en PostgreSQL de desarrollo y recorrido manual completo contra la API. Archivos: ninguno (verificación; cualquier corrección se registra como tarea específica antes de modificar código).
- [ ] Documentar cómo ejecutar frontend/API, configurar secretos locales, preparar Supabase, aplicar migraciones, aprovisionar usuarios y explorar Swagger. Archivos: `README.md` (modificar), `frontend/README.md` (modificar).

## 6. Criterios de terminado

- Un usuario previamente aprovisionado puede autenticarse; credenciales inválidas no revelan información y las rutas de negocio rechazan requests sin JWT.
- Se pueden crear, consultar, actualizar y desactivar productos, con búsqueda y paginación del lado del servidor.
- Un producto conserva múltiples lotes por proveedor; precios y cantidades se mantienen por lote, y ninguna operación permite stock negativo o combinación duplicada.
- Angular consume los contratos documentados sin conversiones de nombres y presenta errores y estados de formulario de manera centralizada.
- Los datos relacionales, FKs, constraints e índices están migrados en PostgreSQL; los secretos quedan fuera del repositorio.
- Swagger documenta y permite probar los endpoints autenticados; logs permiten correlacionar solicitudes sin exponer datos sensibles.
- Las pruebas cubren reglas críticas y errores HTTP; las compilaciones de Angular y .NET finalizan correctamente.
