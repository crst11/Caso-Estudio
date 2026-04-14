public record ProductoDto(
    int id_Producto,
    string? nombre_Producto,
    string? marca,
    string? talla,
    string? color,
    double? precio,
    int? id_Proveedor,
    string? proveedor,
    int? id_Inventario,
    int stock);

public record ProductoUpsertDto(
    string? nombre_Producto,
    string? marca,
    string? talla,
    string? color,
    double? precio,
    int? id_Proveedor,
    int? stock);

public record ProveedorDto(
    int id_Proveedor,
    string? nombre_Proveedor,
    long? telefono,
    string? correo,
    string? direccion,
    int? id_Producto,
    string? producto);

public record ProveedorUpsertDto(
    string? nombre_Proveedor,
    long? telefono,
    string? correo,
    string? direccion,
    int? id_Producto);

public record InventarioDto(
    int id_Inventario,
    int? id_Producto,
    string? nombre_Producto,
    string? marca,
    string? color,
    string? talla,
    string? proveedor,
    double? precio,
    int cantidad);

public record VentaDto(
    int id_Ventas,
    DateTime? fecha_Venta,
    int? total,
    int cantidad,
    int? id_Cliente,
    string? cliente,
    int? id_Producto,
    string? producto,
    double? precioUnitario,
    int stockRestante);

public record VentaUpsertDto(
    int? id_Cliente,
    int? id_Producto,
    int cantidad,
    DateTime? fecha_Venta);
