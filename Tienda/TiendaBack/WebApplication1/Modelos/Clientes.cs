using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

[Table("Clientes")]
public class Clientes
{
    [Key]
    public int id_Cliente { get; set; }
    public string? nombre_Cliente { get; set; }
    public string? apellido_Cliente { get; set; }
    public long? telefono { get; set; }
    public string? correo { get; set; }
    public string? direccion { get; set; }
    public int? id_Ventas { get; set; }

    [JsonIgnore]
    public ICollection<Ventas> Ventas { get; set; } = new List<Ventas>();
}
