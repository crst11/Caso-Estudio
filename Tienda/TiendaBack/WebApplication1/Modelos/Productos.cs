using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Productos")]
public class Productos
{
    [Key]
    public int id_Producto { get; set; }

    public string? nombre_Producto { get; set; }
    public string? marca { get; set; }
    public string? talla { get; set; }
    public string? color { get; set; }

    [Column(TypeName = "float")]
    public double? precio { get; set; }

    [ForeignKey(nameof(ProveedoresRelacionado))]
    public int? id_Proveedor { get; set; }

    [Column("id__Inventario")]
    [ForeignKey(nameof(InventarioRelacionado))]
    public int? id_Inventario { get; set; }

    [JsonIgnore]
    public Proveedores? ProveedoresRelacionado { get; set; }

    [JsonIgnore]
    public Inventario? InventarioRelacionado { get; set; }

    [JsonIgnore]
    public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    [JsonIgnore]
    public ICollection<Ventas> Ventas { get; set; } = new List<Ventas>();

    [JsonIgnore]
    public ICollection<Proveedores> Proveedores { get; set; } = new List<Proveedores>();
}
