using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Inventario")]
public class Inventario
{
    [Key]
    public int id_Inventario { get; set; }

    [ForeignKey(nameof(Producto))]
    public int? id_Producto { get; set; }

    public int? cantidad { get; set; }

    [JsonIgnore]
    public Productos? Producto { get; set; }
}
