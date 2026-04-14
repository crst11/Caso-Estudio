using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class InventariosController : ControllerBase
{
    private readonly TiendaDatos _context;

    public InventariosController(TiendaDatos context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> Get()
    {
        var inventarios = await QueryInventario().ToListAsync();

        return Ok(inventarios
            .Select(TiendaMappers.ToDto)
            .OrderBy(item => item.nombre_Producto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InventarioDto>> GetById(int id)
    {
        var inventario = await BuscarInventarioDtoAsync(id);
        if (inventario == null)
        {
            return NotFound();
        }

        return Ok(inventario);
    }

    [HttpPost]
    public async Task<ActionResult<InventarioDto>> Post(Inventario inventario)
    {
        if (inventario.id_Producto == null)
        {
            return BadRequest("Debe enviar un producto valido.");
        }

        if ((inventario.cantidad ?? 0) < 0)
        {
            return BadRequest("La cantidad no puede ser negativa.");
        }

        var producto = await _context.Productos.FindAsync(inventario.id_Producto);
        if (producto == null)
        {
            return BadRequest("El producto seleccionado no existe.");
        }

        var existente = await _context.Inventarios
            .FirstOrDefaultAsync(item => item.id_Producto == inventario.id_Producto);

        if (existente != null)
        {
            existente.cantidad = (existente.cantidad ?? 0) + (inventario.cantidad ?? 0);
            await SincronizarProductoInventarioAsync(existente.id_Producto, existente.id_Inventario);
            await _context.SaveChangesAsync();

            var dtoExistente = await BuscarInventarioDtoAsync(existente.id_Inventario);
            return Ok(dtoExistente);
        }

        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync();

        await SincronizarProductoInventarioAsync(inventario.id_Producto, inventario.id_Inventario);
        await _context.SaveChangesAsync();

        var dto = await BuscarInventarioDtoAsync(inventario.id_Inventario);
        return CreatedAtAction(nameof(GetById), new { id = inventario.id_Inventario }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InventarioDto>> Put(int id, Inventario inventario)
    {
        var existente = await _context.Inventarios.FindAsync(id);
        if (existente == null)
        {
            return NotFound();
        }

        if (inventario.id_Producto == null)
        {
            return BadRequest("Debe enviar un producto valido.");
        }

        if ((inventario.cantidad ?? 0) < 0)
        {
            return BadRequest("La cantidad no puede ser negativa.");
        }

        var producto = await _context.Productos.FindAsync(inventario.id_Producto);
        if (producto == null)
        {
            return BadRequest("El producto seleccionado no existe.");
        }

        var productoAnteriorId = existente.id_Producto;
        var duplicado = await _context.Inventarios
            .FirstOrDefaultAsync(item => item.id_Inventario != id && item.id_Producto == inventario.id_Producto);

        if (duplicado != null)
        {
            duplicado.cantidad = inventario.cantidad;
            _context.Inventarios.Remove(existente);

            await DesvincularProductoInventarioAsync(productoAnteriorId, existente.id_Inventario);
            await SincronizarProductoInventarioAsync(duplicado.id_Producto, duplicado.id_Inventario);
            await _context.SaveChangesAsync();

            var dtoDuplicado = await BuscarInventarioDtoAsync(duplicado.id_Inventario);
            return Ok(dtoDuplicado);
        }

        if (productoAnteriorId != inventario.id_Producto)
        {
            await DesvincularProductoInventarioAsync(productoAnteriorId, existente.id_Inventario);
        }

        existente.id_Producto = inventario.id_Producto;
        existente.cantidad = inventario.cantidad;

        await SincronizarProductoInventarioAsync(existente.id_Producto, existente.id_Inventario);
        await _context.SaveChangesAsync();

        var dto = await BuscarInventarioDtoAsync(existente.id_Inventario);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inventario = await _context.Inventarios.FindAsync(id);
        if (inventario == null)
        {
            return NotFound();
        }

        await DesvincularProductoInventarioAsync(inventario.id_Producto, inventario.id_Inventario);
        _context.Inventarios.Remove(inventario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("normalizar")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> NormalizarDuplicados()
    {
        var inventarios = await _context.Inventarios
            .OrderBy(inventario => inventario.id_Inventario)
            .ToListAsync();

        var gruposDuplicados = inventarios
            .Where(inventario => inventario.id_Producto != null)
            .GroupBy(inventario => inventario.id_Producto)
            .Where(grupo => grupo.Count() > 1)
            .ToList();

        foreach (var grupo in gruposDuplicados)
        {
            var principal = grupo.OrderBy(item => item.id_Inventario).First();
            var secundarios = grupo.Where(item => item.id_Inventario != principal.id_Inventario).ToList();

            principal.cantidad = grupo.Sum(item => item.cantidad ?? 0);
            _context.Inventarios.RemoveRange(secundarios);
            await SincronizarProductoInventarioAsync(principal.id_Producto, principal.id_Inventario);
        }

        await _context.SaveChangesAsync();

        var inventarioNormalizado = await QueryInventario().ToListAsync();

        return Ok(inventarioNormalizado
            .Select(TiendaMappers.ToDto)
            .OrderBy(item => item.nombre_Producto));
    }

    private IQueryable<Inventario> QueryInventario()
    {
        return _context.Inventarios
            .AsNoTracking()
            .Include(inventario => inventario.Producto)
            .ThenInclude(producto => producto!.ProveedoresRelacionado);
    }

    private async Task<InventarioDto?> BuscarInventarioDtoAsync(int id)
    {
        var inventario = await QueryInventario().FirstOrDefaultAsync(item => item.id_Inventario == id);
        return inventario == null ? null : TiendaMappers.ToDto(inventario);
    }

    private async Task SincronizarProductoInventarioAsync(int? idProducto, int idInventario)
    {
        if (idProducto == null)
        {
            return;
        }

        var producto = await _context.Productos.FirstOrDefaultAsync(item => item.id_Producto == idProducto);
        if (producto == null)
        {
            return;
        }

        producto.id_Inventario = idInventario;
    }

    private async Task DesvincularProductoInventarioAsync(int? idProducto, int idInventario)
    {
        if (idProducto == null)
        {
            return;
        }

        var producto = await _context.Productos.FirstOrDefaultAsync(item => item.id_Producto == idProducto);
        if (producto?.id_Inventario == idInventario)
        {
            producto.id_Inventario = null;
        }
    }
}
