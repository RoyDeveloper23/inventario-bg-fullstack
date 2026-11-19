# Sistema de Inventario BG

## Requisitos locales

- .NET 8 SDK
- Node.js compatible con Angular 19
- PostgreSQL o proyecto Supabase

## API

Configurar secretos locales con variables de entorno. No guardar credenciales reales en los archivos `appsettings*.json`.

```powershell
$env:ConnectionStrings__Postgres = "Host=<host>;Port=5432;Database=<base>;Username=<usuario>;Password=<password>;SSL Mode=Require"
$env:Jwt__ClaveSecreta = "<clave-secreta-de-al-menos-32-caracteres>"
$env:Jwt__Emisor = "InventarioBg"
$env:Jwt__Audiencia = "InventarioBg"
$env:Jwt__MinutosExpiracion = "60"
$env:Cors__Origins__0 = "http://localhost:4200"
```

Ejecutar la API:

```powershell
cd backend/InventarioBg
dotnet run --project InventarioBg.API
```

La API registra solicitudes y errores en consola y en `logs/inventario-*.log`.
