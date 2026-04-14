using Microsoft.EntityFrameworkCore;

public class TiendaDatos(DbContextOptions<TiendaDatos> options) : DbContext(options)
{
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<Productos> Productos => Set<Productos>();
    public DbSet<Clientes> Clientes => Set<Clientes>();
    public DbSet<Ventas> Ventas => Set<Ventas>();
    public DbSet<Proveedores> Proveedores => Set<Proveedores>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventario>()
            .HasOne(inventario => inventario.Producto)
            .WithMany(producto => producto.Inventarios)
            .HasForeignKey(inventario => inventario.id_Producto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ventas>()
            .HasOne(venta => venta.Cliente)
            .WithMany(cliente => cliente.Ventas)
            .HasForeignKey(venta => venta.id_Cliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ventas>()
            .HasOne(venta => venta.Producto)
            .WithMany(producto => producto.Ventas)
            .HasForeignKey(venta => venta.id_Producto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proveedores>()
            .HasOne(proveedor => proveedor.Producto)
            .WithMany(producto => producto.Proveedores)
            .HasForeignKey(proveedor => proveedor.id_Producto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Productos>()
            .HasOne(producto => producto.ProveedoresRelacionado)
            .WithMany()
            .HasForeignKey(producto => producto.id_Proveedor)
            .HasPrincipalKey(proveedor => proveedor.id_Proveedor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Productos>()
            .HasOne(producto => producto.InventarioRelacionado)
            .WithMany()
            .HasForeignKey(producto => producto.id_Inventario)
            .HasPrincipalKey(inventario => inventario.id_Inventario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Productos>()
            .Property(producto => producto.precio)
            .HasColumnType("float");
    }
}
