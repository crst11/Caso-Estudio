using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Ventas")]
public class Ventas
{
    [Key]
    public int id_Ventas { get; set; }
    public DateTime? fecha_Venta { get; set; }
    public int? total { get; set; }
    public int cantidad { get; set; } = 1;

    [ForeignKey(nameof(Cliente))]
    public int? id_Cliente { get; set; }

    [ForeignKey(nameof(Producto))]
    public int? id_Producto { get; set; }

    [JsonIgnore]
    public Clientes? Cliente { get; set; }

    [JsonIgnore]
    public Productos? Producto { get; set; }
}
