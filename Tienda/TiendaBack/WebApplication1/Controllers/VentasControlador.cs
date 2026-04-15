using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Registra ventas, valida stock disponible y devuelve unidades al eliminar una venta.
[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly TiendaDatos _context;

    public VentasController(TiendaDatos context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VentaDto>>> Get()
    {
        var ventas = await QueryVentas().ToListAsync();

        return Ok(ventas
            .Select(TiendaMappers.ToDto)
            .OrderByDescending(venta => venta.fecha_Venta)
            .ThenByDescending(venta => venta.id_Ventas));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> GetById(int id)
    {
        var venta = await BuscarVentaDtoAsync(id);
        if (venta == null)
        {
            return NotFound();
        }

        return Ok(venta);
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Post(VentaUpsertDto request)
    {
        if (request.id_Cliente == null || request.id_Cliente <= 0)
        {
            return BadRequest("Debes seleccionar un cliente valido.");
        }

        if (request.id_Producto == null || request.id_Producto <= 0)
        {
            return BadRequest("Debes seleccionar un producto valido.");
        }

        if (request.cantidad <= 0)
        {
            return BadRequest("La cantidad debe ser mayor a cero.");
        }

        var cliente = await _context.Clientes.FindAsync(request.id_Cliente);
        if (cliente == null)
        {
            return BadRequest("El cliente seleccionado no existe.");
        }

        var producto = await _context.Productos.FindAsync(request.id_Producto);
        if (producto == null)
        {
            return BadRequest("El producto seleccionado no existe.");
        }

        var inventario = await _context.Inventarios.FirstOrDefaultAsync(item => item.id_Producto == request.id_Producto);
        if (inventario == null)
        {
            return BadRequest("No hay inventario para este producto.");
        }

        var stockActual = inventario.cantidad ?? 0;
        if (stockActual < request.cantidad)
        {
            return BadRequest($"Stock insuficiente. Solo quedan {stockActual} unidad(es).");
        }

        var venta = new Ventas
        {
            id_Cliente = request.id_Cliente,
            id_Producto = request.id_Producto,
            cantidad = request.cantidad,
            fecha_Venta = request.fecha_Venta?.Date ?? DateTime.Today,
            total = (int)Math.Round((producto.precio ?? 0) * request.cantidad)
        };

        inventario.cantidad = stockActual - request.cantidad;

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        var dto = await BuscarVentaDtoAsync(venta.id_Ventas);
        return CreatedAtAction(nameof(GetById), new { id = venta.id_Ventas }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta == null)
        {
            return NotFound();
        }

        if (venta.id_Producto != null && venta.cantidad > 0)
        {
            var inventario = await _context.Inventarios.FirstOrDefaultAsync(item => item.id_Producto == venta.id_Producto);
            if (inventario != null)
            {
                inventario.cantidad = (inventario.cantidad ?? 0) + venta.cantidad;
            }
        }

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Ventas> QueryVentas()
    {
        return _context.Ventas
            .AsNoTracking()
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Producto)
            .ThenInclude(producto => producto!.InventarioRelacionado)
            .Include(venta => venta.Producto)
            .ThenInclude(producto => producto!.Inventarios);
    }

    private async Task<VentaDto?> BuscarVentaDtoAsync(int id)
    {
        var venta = await QueryVentas().FirstOrDefaultAsync(item => item.id_Ventas == id);
        return venta == null ? null : TiendaMappers.ToDto(venta);
    }
}
