using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Administra proveedores y su relacion directa con los productos.
[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly TiendaDatos _context;

    public ProveedoresController(TiendaDatos context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProveedorDto>>> Get()
    {
        var proveedores = await QueryProveedores().ToListAsync();

        return Ok(proveedores
            .Select(TiendaMappers.ToDto)
            .OrderBy(proveedor => proveedor.nombre_Proveedor));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> GetById(int id)
    {
        var proveedor = await BuscarProveedorDtoAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }

        return Ok(proveedor);
    }

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Post(ProveedorUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.nombre_Proveedor))
        {
            return BadRequest("El nombre del proveedor es obligatorio.");
        }

        Productos? producto = null;
        if (request.id_Producto is int productoId)
        {
            producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                return BadRequest("El producto seleccionado no existe.");
            }
        }

        var proveedor = new Proveedores
        {
            nombre_Proveedor = request.nombre_Proveedor.Trim(),
            telefono = request.telefono,
            correo = NormalizarTexto(request.correo),
            direccion = NormalizarTexto(request.direccion),
            id_Producto = request.id_Producto
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        if (producto != null)
        {
            await VincularProductoAsync(producto, proveedor.id_Proveedor);
            await _context.SaveChangesAsync();
        }

        var dto = await BuscarProveedorDtoAsync(proveedor.id_Proveedor);
        return CreatedAtAction(nameof(GetById), new { id = proveedor.id_Proveedor }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> Put(int id, ProveedorUpsertDto request)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.nombre_Proveedor))
        {
            return BadRequest("El nombre del proveedor es obligatorio.");
        }

        var productoAnteriorId = proveedor.id_Producto;
        Productos? productoNuevo = null;

        if (request.id_Producto is int productoId)
        {
            productoNuevo = await _context.Productos.FindAsync(productoId);
            if (productoNuevo == null)
            {
                return BadRequest("El producto seleccionado no existe.");
            }
        }

        proveedor.nombre_Proveedor = request.nombre_Proveedor.Trim();
        proveedor.telefono = request.telefono;
        proveedor.correo = NormalizarTexto(request.correo);
        proveedor.direccion = NormalizarTexto(request.direccion);
        proveedor.id_Producto = request.id_Producto;

        if (productoAnteriorId != request.id_Producto && productoAnteriorId is int oldProductoId)
        {
            var productoAnterior = await _context.Productos.FindAsync(oldProductoId);
            if (productoAnterior?.id_Proveedor == proveedor.id_Proveedor)
            {
                productoAnterior.id_Proveedor = null;
            }
        }

        if (productoNuevo != null)
        {
            await VincularProductoAsync(productoNuevo, proveedor.id_Proveedor);
        }

        await _context.SaveChangesAsync();

        var dto = await BuscarProveedorDtoAsync(proveedor.id_Proveedor);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }

        var tieneProductos = await _context.Productos.AnyAsync(producto => producto.id_Proveedor == id);
        if (tieneProductos)
        {
            return BadRequest("No puedes eliminar un proveedor que sigue ligado a productos.");
        }

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Proveedores> QueryProveedores()
    {
        return _context.Proveedores
            .AsNoTracking()
            .Include(proveedor => proveedor.Producto);
    }

    private async Task<ProveedorDto?> BuscarProveedorDtoAsync(int id)
    {
        var proveedor = await QueryProveedores().FirstOrDefaultAsync(item => item.id_Proveedor == id);
        return proveedor == null ? null : TiendaMappers.ToDto(proveedor);
    }

    private async Task VincularProductoAsync(Productos producto, int idProveedor)
    {
        if (producto.id_Proveedor == idProveedor)
        {
            return;
        }

        if (producto.id_Proveedor is int proveedorAnteriorId)
        {
            var proveedorAnterior = await _context.Proveedores.FindAsync(proveedorAnteriorId);
            if (proveedorAnterior?.id_Producto == producto.id_Producto)
            {
                proveedorAnterior.id_Producto = null;
            }
        }

        producto.id_Proveedor = idProveedor;
    }

    private static string? NormalizarTexto(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
