using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Proveedores")]
public class Proveedores
{
    [Key]
    public int id_Proveedor { get; set; }

    [Column("nombre_Proveedor")]
    public string? nombre_Proveedor { get; set; }

    public long? telefono { get; set; }
    public string? correo { get; set; }
    public string? direccion { get; set; }

    [ForeignKey(nameof(Producto))]
    public int? id_Producto { get; set; }

    [JsonIgnore]
    public Productos? Producto { get; set; }
}
