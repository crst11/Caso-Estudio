# Tienda StyleStore

Sistema de gestion para una tienda con modulo de productos, inventario, ventas, clientes y proveedores.

## Tecnologias

- Backend: ASP.NET Core 8 + Entity Framework Core + SQL Server
- Frontend: Angular + TypeScript + HTML + CSS
- Base de datos: SQL Server

## Estructura Del Proyecto

### `Base de datos en script/`
Contiene el script SQL base para crear o recrear la base de datos del proyecto.

### `TiendaBack/WebApplication1/`
Contiene la API del sistema.

- `Program.cs`: punto de entrada del backend. Configura servicios, base de datos, CORS y Swagger.
- `appsettings.json`: define la cadena de conexion a SQL Server.
- `Datos/TiendaDatos.cs`: contexto de Entity Framework y relaciones entre tablas.
- `Modelos/`: clases que representan las tablas de la base de datos.
- `Contratos/`: DTOs y mapeadores para transformar entidades en respuestas limpias para el frontend.
- `Controllers/`: logica de negocio y endpoints CRUD de cada modulo.

### `TiendaFront/frontend/`
Contiene la aplicacion Angular.

- `src/app/app.ts`: componente raiz.
- `src/app/tienda/`: layout principal del panel y navegacion lateral.
- `src/app/services/api.ts`: servicio central para consumir la API.
- `src/app/models/tienda.models.ts`: interfaces tipadas de productos, ventas, clientes, inventario y proveedores.
- `src/app/pages/`: pantallas del sistema separadas por modulo.
- `src/styles.css`: estilos globales de la aplicacion.

## Modulos Del Sistema

- Productos: crea, lista y elimina productos. Cada producto se relaciona con proveedor e inventario.
- Inventario: registra entradas de stock y mantiene sincronizada la cantidad disponible por producto.
- Ventas: registra ventas, valida stock disponible y descuenta inventario automaticamente.
- Clientes: administra la informacion de los clientes.
- Proveedores: administra proveedores y su relacion con los productos.

## Flujo General

1. El usuario interactua con Angular.
2. Angular llama a la API mediante `ApiService`.
3. Los controladores del backend aplican validaciones y reglas de negocio.
4. Entity Framework consulta o actualiza SQL Server.
5. La API devuelve DTOs limpios al frontend.
6. El frontend actualiza la interfaz con los nuevos datos.

## Como Ejecutarlo

### Backend

```powershell
cd "TiendaBack\WebApplication1"
dotnet run --launch-profile http
```

API disponible en:

- `http://localhost:5186`
- `http://localhost:5186/swagger`

### Frontend

```powershell
cd "TiendaFront\frontend"
npm start -- --host 0.0.0.0 --port 4200
```

Aplicacion disponible en:

- `http://localhost:4200`

## Notas

- Las carpetas `bin`, `obj`, `node_modules`, `dist` y `.angular` son generadas y no deben subirse al repositorio.
- El proyecto usa una API REST para mantener separada la logica de negocio del frontend.
