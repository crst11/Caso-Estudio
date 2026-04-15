// Convierte entidades de base de datos en DTOs listos para el frontend y limpia textos.
public static class TiendaMappers
{
    public static ProductoDto ToDto(Productos producto)
    {
        var inventario = producto.InventarioRelacionado ?? producto.Inventarios.FirstOrDefault();

        return new ProductoDto(
            producto.id_Producto,
            Limpiar(producto.nombre_Producto),
            Limpiar(producto.marca),
            Limpiar(producto.talla),
            Limpiar(producto.color),
            producto.precio,
            producto.id_Proveedor,
            Limpiar(producto.ProveedoresRelacionado?.nombre_Proveedor),
            producto.id_Inventario,
            inventario?.cantidad ?? 0);
    }

    public static ProveedorDto ToDto(Proveedores proveedor)
    {
        return new ProveedorDto(
            proveedor.id_Proveedor,
            Limpiar(proveedor.nombre_Proveedor),
            proveedor.telefono,
            Limpiar(proveedor.correo),
            Limpiar(proveedor.direccion),
            proveedor.id_Producto,
            Limpiar(proveedor.Producto?.nombre_Producto));
    }

    public static InventarioDto ToDto(Inventario inventario)
    {
        return new InventarioDto(
            inventario.id_Inventario,
            inventario.id_Producto,
            Limpiar(inventario.Producto?.nombre_Producto),
            Limpiar(inventario.Producto?.marca),
            Limpiar(inventario.Producto?.color),
            Limpiar(inventario.Producto?.talla),
            Limpiar(inventario.Producto?.ProveedoresRelacionado?.nombre_Proveedor),
            inventario.Producto?.precio,
            inventario.cantidad ?? 0);
    }

    public static VentaDto ToDto(Ventas venta)
    {
        var precioUnitario = venta.Producto?.precio;
        var inventario = venta.Producto?.InventarioRelacionado ?? venta.Producto?.Inventarios.FirstOrDefault();

        return new VentaDto(
            venta.id_Ventas,
            venta.fecha_Venta,
            venta.total ?? CalcularTotal(precioUnitario, venta.cantidad),
            venta.cantidad,
            venta.id_Cliente,
            FormatearNombreCompleto(venta.Cliente?.nombre_Cliente, venta.Cliente?.apellido_Cliente),
            venta.id_Producto,
            Limpiar(venta.Producto?.nombre_Producto),
            precioUnitario,
            inventario?.cantidad ?? 0);
    }

    public static Clientes LimpiarCliente(Clientes cliente)
    {
        return new Clientes
        {
            id_Cliente = cliente.id_Cliente,
            nombre_Cliente = Limpiar(cliente.nombre_Cliente),
            apellido_Cliente = Limpiar(cliente.apellido_Cliente),
            telefono = cliente.telefono,
            correo = Limpiar(cliente.correo),
            direccion = Limpiar(cliente.direccion),
            id_Ventas = cliente.id_Ventas
        };
    }

    public static string? Limpiar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string? FormatearNombreCompleto(string? nombre, string? apellido)
    {
        var nombreLimpio = Limpiar(nombre);
        var apellidoLimpio = Limpiar(apellido);
        var nombreCompleto = string.Join(' ', new[] { nombreLimpio, apellidoLimpio }.Where(item => !string.IsNullOrWhiteSpace(item)));

        return string.IsNullOrWhiteSpace(nombreCompleto) ? null : nombreCompleto;
    }

    private static int? CalcularTotal(double? precioUnitario, int cantidad)
    {
        return precioUnitario == null ? null : (int)Math.Round(precioUnitario.Value * cantidad);
    }
}
