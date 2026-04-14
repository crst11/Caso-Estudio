using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly TiendaDatos _context;

    public ProductosController(TiendaDatos context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> Get()
    {
        var productos = await QueryProductos().ToListAsync();

        return Ok(productos
            .Select(TiendaMappers.ToDto)
            .OrderBy(producto => producto.nombre_Producto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> GetById(int id)
    {
        var producto = await BuscarProductoDtoAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Post(ProductoUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.nombre_Producto))
        {
            return BadRequest("El nombre del producto es obligatorio.");
        }

        if (request.precio is null || request.precio < 0)
        {
            return BadRequest("El precio debe ser mayor o igual a cero.");
        }

        Proveedores? proveedor = null;
        if (request.id_Proveedor is int proveedorId)
        {
            proveedor = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedor == null)
            {
                return BadRequest("El proveedor seleccionado no existe.");
            }
        }

        var producto = new Productos
        {
            nombre_Producto = request.nombre_Producto.Trim(),
            marca = NormalizarTexto(request.marca),
            talla = NormalizarTexto(request.talla),
            color = NormalizarTexto(request.color),
            precio = request.precio,
            id_Proveedor = request.id_Proveedor
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var inventario = new Inventario
        {
            id_Producto = producto.id_Producto,
            cantidad = Math.Max(request.stock ?? 0, 0)
        };

        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync();

        producto.id_Inventario = inventario.id_Inventario;
        if (proveedor != null)
        {
            await VincularProveedorAsync(proveedor, producto.id_Producto);
        }

        await _context.SaveChangesAsync();

        var dto = await BuscarProductoDtoAsync(producto.id_Producto);
        return CreatedAtAction(nameof(GetById), new { id = producto.id_Producto }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Put(int id, ProductoUpsertDto request)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.nombre_Producto))
        {
            return BadRequest("El nombre del producto es obligatorio.");
        }

        if (request.precio is null || request.precio < 0)
        {
            return BadRequest("El precio debe ser mayor o igual a cero.");
        }

        var proveedorAnteriorId = producto.id_Proveedor;
        Proveedores? proveedorNuevo = null;

        if (request.id_Proveedor is int proveedorId)
        {
            proveedorNuevo = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedorNuevo == null)
            {
                return BadRequest("El proveedor seleccionado no existe.");
            }
        }

        producto.nombre_Producto = request.nombre_Producto.Trim();
        producto.marca = NormalizarTexto(request.marca);
        producto.talla = NormalizarTexto(request.talla);
        producto.color = NormalizarTexto(request.color);
        producto.precio = request.precio;
        producto.id_Proveedor = request.id_Proveedor;

        if (proveedorAnteriorId != request.id_Proveedor && proveedorAnteriorId is int oldProveedorId)
        {
            var proveedorAnterior = await _context.Proveedores.FindAsync(oldProveedorId);
            if (proveedorAnterior?.id_Producto == producto.id_Producto)
            {
                proveedorAnterior.id_Producto = null;
            }
        }

        if (proveedorNuevo != null)
        {
            await VincularProveedorAsync(proveedorNuevo, producto.id_Producto);
        }

        if (request.stock is int stock && producto.id_Inventario is int inventarioId)
        {
            var inventario = await _context.Inventarios.FindAsync(inventarioId);
            if (inventario != null)
            {
                inventario.cantidad = Math.Max(stock, 0);
            }
        }

        await _context.SaveChangesAsync();

        var dto = await BuscarProductoDtoAsync(producto.id_Producto);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        var tieneVentas = await _context.Ventas.AnyAsync(venta => venta.id_Producto == id);
        if (tieneVentas)
        {
            return BadRequest("No puedes eliminar un producto con ventas registradas.");
        }

        if (producto.id_Proveedor is int proveedorId)
        {
            var proveedor = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedor?.id_Producto == producto.id_Producto)
            {
                proveedor.id_Producto = null;
            }
        }

        if (producto.id_Inventario is int inventarioId)
        {
            var inventario = await _context.Inventarios.FindAsync(inventarioId);
            if (inventario != null)
            {
                _context.Inventarios.Remove(inventario);
            }
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Productos> QueryProductos()
    {
        return _context.Productos
            .AsNoTracking()
            .Include(producto => producto.ProveedoresRelacionado)
            .Include(producto => producto.InventarioRelacionado)
            .Include(producto => producto.Inventarios);
    }

    private async Task<ProductoDto?> BuscarProductoDtoAsync(int id)
    {
        var producto = await QueryProductos().FirstOrDefaultAsync(item => item.id_Producto == id);
        return producto == null ? null : TiendaMappers.ToDto(producto);
    }

    private async Task VincularProveedorAsync(Proveedores proveedor, int idProducto)
    {
        if (proveedor.id_Producto == idProducto)
        {
            return;
        }

        if (proveedor.id_Producto is int productoAnteriorId)
        {
            var productoAnterior = await _context.Productos.FindAsync(productoAnteriorId);
            if (productoAnterior?.id_Proveedor == proveedor.id_Proveedor)
            {
                productoAnterior.id_Proveedor = null;
            }
        }

        proveedor.id_Producto = idProducto;
    }

    private static string? NormalizarTexto(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
