import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import {
  AlertType,
  Cliente,
  Producto,
  Venta,
  VentaPayload,
} from '../../models/tienda.models';
import { ApiService } from '../../services/api';
import { getErrorMessage } from '../../shared/http-error';

// Pantalla de ventas: registra compras, calcula total y refleja el descuento en inventario.
@Component({
  selector: 'app-ventas',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ventas.html',
  styleUrl: './ventas.css',
})
export class VentasComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  ventas: Venta[] = [];
  productos: Producto[] = [];
  clientes: Cliente[] = [];
  mostrarForm = false;
  cargando = false;
  mensaje = '';
  mensajeTipo: AlertType = 'success';
  nuevaVenta: VentaPayload = this.crearFormulario();

  ngOnInit(): void {
    this.cargarTodo();

    this.api.cambios$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cargarTodo());
  }

  get productoSeleccionado(): Producto | undefined {
    return this.productos.find((item) => item.id_Producto === this.nuevaVenta.id_Producto);
  }

  get stockDisponible(): number {
    return this.productoSeleccionado?.stock ?? 0;
  }

  get stockDespuesDeVenta(): number {
    const cantidad = Number(this.nuevaVenta.cantidad) || 0;
    return Math.max(this.stockDisponible - cantidad, 0);
  }

  get puedeGuardar(): boolean {
    return Boolean(
      this.nuevaVenta.id_Cliente &&
        this.nuevaVenta.id_Producto &&
        this.nuevaVenta.cantidad > 0 &&
        this.nuevaVenta.cantidad <= this.stockDisponible,
    );
  }

  cargarTodo(): void {
    this.cargando = true;

    forkJoin({
      productos: this.api.getProductos(),
      clientes: this.api.getClientes(),
      ventas: this.api.getVentas(),
    })
      .pipe(finalize(() => (this.cargando = false)))
      .subscribe({
        next: ({ productos, clientes, ventas }) => {
          this.productos = productos;
          this.clientes = clientes;
          this.ventas = ventas;
          this.calcularTotal();
        },
        error: (error) => {
          this.mostrar(getErrorMessage(error, 'No fue posible cargar las ventas.'), 'error');
        },
      });
  }

  calcularTotal(): number {
    const cantidad = Number(this.nuevaVenta.cantidad) || 0;
    const total = (this.productoSeleccionado?.precio ?? 0) * cantidad;
    return total;
  }

  guardarVenta(): void {
    if (!this.nuevaVenta.id_Cliente || !this.nuevaVenta.id_Producto || this.nuevaVenta.cantidad <= 0) {
      this.mostrar('Selecciona cliente, producto y una cantidad valida.', 'error');
      return;
    }

    if (this.nuevaVenta.cantidad > this.stockDisponible) {
      this.mostrar(`No hay suficiente stock. Solo quedan ${this.stockDisponible} unidad(es).`, 'error');
      return;
    }

    const venta = { ...this.nuevaVenta };
    this.cerrarFormulario();

    this.api.agregarVenta(venta).subscribe({
      next: () => {
        this.mostrar('Venta registrada y stock descontado.', 'success');
        this.cargarTodo();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.nuevaVenta = venta;
        this.mostrarForm = true;
        this.mostrar(getErrorMessage(error, 'No fue posible registrar la venta.'), 'error');
      },
    });
  }

  eliminarVenta(id: number): void {
    if (!confirm('Eliminar esta venta y devolver el stock?')) {
      return;
    }

    this.api.eliminarVenta(id).subscribe({
      next: () => {
        this.mostrar('Venta eliminada y stock restaurado.', 'success');
        this.cargarTodo();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.mostrar(getErrorMessage(error, 'No fue posible eliminar la venta.'), 'error');
      },
    });
  }

  texto(value: string | null | undefined, fallback = '-'): string {
    const textoLimpio = this.limpiarTexto(value ?? '');
    return textoLimpio || fallback;
  }

  toggleFormulario(): void {
    this.mensaje = '';

    if (this.mostrarForm) {
      this.cerrarFormulario();
      return;
    }

    this.resetearFormulario();
    this.mostrarForm = true;
  }

  private crearFormulario(): VentaPayload {
    return {
      id_Cliente: null,
      id_Producto: null,
      cantidad: 1,
      fecha_Venta: new Date().toISOString().slice(0, 10),
    };
  }

  private resetearFormulario(): void {
    this.nuevaVenta = this.crearFormulario();
  }

  cerrarFormulario(): void {
    this.resetearFormulario();
    this.mostrarForm = false;
  }

  private mostrar(msg: string, tipo: AlertType): void {
    this.mensaje = msg;
    this.mensajeTipo = tipo;
    window.setTimeout(() => (this.mensaje = ''), 4000);
  }

  private limpiarTexto(value: string): string {
    return value.replace(/\s+/g, ' ').trim();
  }
}
