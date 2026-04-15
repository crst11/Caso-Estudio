using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Administra clientes y evita eliminar registros que ya tienen ventas asociadas.
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly TiendaDatos _context;

    public ClientesController(TiendaDatos context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Clientes>>> Get()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.id_Cliente)
            .ToListAsync();

        return Ok(clientes.Select(TiendaMappers.LimpiarCliente));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Clientes>> GetById(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.id_Cliente == id);

        return cliente == null ? NotFound() : Ok(TiendaMappers.LimpiarCliente(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<Clientes>> Post(Clientes cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.nombre_Cliente))
        {
            return BadRequest("El nombre del cliente es obligatorio.");
        }

        cliente.nombre_Cliente = TiendaMappers.Limpiar(cliente.nombre_Cliente);
        cliente.apellido_Cliente = TiendaMappers.Limpiar(cliente.apellido_Cliente);
        cliente.correo = TiendaMappers.Limpiar(cliente.correo);
        cliente.direccion = TiendaMappers.Limpiar(cliente.direccion);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.id_Cliente }, TiendaMappers.LimpiarCliente(cliente));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Clientes>> Put(int id, Clientes cliente)
    {
        var existente = await _context.Clientes.FindAsync(id);
        if (existente == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(cliente.nombre_Cliente))
        {
            return BadRequest("El nombre del cliente es obligatorio.");
        }

        existente.nombre_Cliente = TiendaMappers.Limpiar(cliente.nombre_Cliente);
        existente.apellido_Cliente = TiendaMappers.Limpiar(cliente.apellido_Cliente);
        existente.telefono = cliente.telefono;
        existente.correo = TiendaMappers.Limpiar(cliente.correo);
        existente.direccion = TiendaMappers.Limpiar(cliente.direccion);
        existente.id_Ventas = cliente.id_Ventas;

        await _context.SaveChangesAsync();

        return Ok(TiendaMappers.LimpiarCliente(existente));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound();
        }

        var tieneVentas = await _context.Ventas.AnyAsync(venta => venta.id_Cliente == id);
        if (tieneVentas)
        {
            return BadRequest("No puedes eliminar un cliente con ventas registradas.");
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
