import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import { AlertType, Producto, ProductoPayload, Proveedor } from '../../models/tienda.models';
import { ApiService } from '../../services/api';
import { getErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './productos.html',
  styleUrl: './productos.css',
})
export class ProductosComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  productos: Producto[] = [];
  proveedores: Proveedor[] = [];
  mostrarForm = false;
  cargando = false;
  mensaje = '';
  mensajeTipo: AlertType = 'success';
  nuevo: ProductoPayload = this.crearFormulario();

  ngOnInit(): void {
    this.cargar();

    this.api.cambios$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cargar());
  }

  cargar(): void {
    this.cargando = true;

    forkJoin({
      productos: this.api.getProductos(),
      proveedores: this.api.getProveedores(),
    })
      .pipe(finalize(() => (this.cargando = false)))
      .subscribe({
        next: ({ productos, proveedores }) => {
          this.productos = productos;
          this.proveedores = proveedores;
        },
        error: (error) => {
          this.mostrar(getErrorMessage(error, 'No fue posible cargar los productos.'), 'error');
        },
      });
  }

  guardar(): void {
    if (!this.nuevo.nombre_Producto.trim() || this.nuevo.precio == null) {
      this.mostrar('Completa el nombre y el precio.', 'error');
      return;
    }

    if ((this.nuevo.stock ?? 0) < 0) {
      this.mostrar('El stock inicial no puede ser negativo.', 'error');
      return;
    }

    const producto = { ...this.nuevo };
    this.cerrarFormulario();

    this.api.agregarProducto(producto).subscribe({
      next: () => {
        this.mostrar('Producto guardado y ligado al inventario.', 'success');
        this.cargar();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.nuevo = producto;
        this.mostrarForm = true;
        this.mostrar(getErrorMessage(error, 'No fue posible guardar el producto.'), 'error');
      },
    });
  }

  eliminar(id: number): void {
    if (!confirm('Eliminar este producto?')) {
      return;
    }

    this.api.eliminarProducto(id).subscribe({
      next: () => {
        this.mostrar('Producto eliminado.', 'success');
        this.cargar();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.mostrar(getErrorMessage(error, 'No fue posible eliminar el producto.'), 'error');
      },
    });
  }

  stockClass(stock: number): string {
    if (stock <= 0) {
      return 'badge-red';
    }

    if (stock <= 5) {
      return 'badge-gold';
    }

    return 'badge-green';
  }

  toggleFormulario(): void {
    this.mensaje = '';

    if (this.mostrarForm) {
      this.cerrarFormulario();
      return;
    }

    this.nuevo = this.crearFormulario();
    this.mostrarForm = true;
  }

  cerrarFormulario(): void {
    this.nuevo = this.crearFormulario();
    this.mostrarForm = false;
  }

  private crearFormulario(): ProductoPayload {
    return {
      nombre_Producto: '',
      marca: null,
      talla: null,
      color: null,
      precio: null,
      id_Proveedor: null,
      stock: 0,
    };
  }

  private mostrar(msg: string, tipo: AlertType): void {
    this.mensaje = msg;
    this.mensajeTipo = tipo;
    window.setTimeout(() => (this.mensaje = ''), 3000);
  }
}
