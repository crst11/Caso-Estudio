import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import { AlertType, Inventario, InventarioPayload, Producto } from '../../models/tienda.models';
import { ApiService } from '../../services/api';
import { getErrorMessage } from '../../shared/http-error';

// Pantalla de inventario: permite agregar cantidades y ver el estado del stock por producto.
@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventario.html',
  styleUrl: './inventario.css',
})
export class InventarioComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  inventario: Inventario[] = [];
  productos: Producto[] = [];
  mostrarForm = false;
  cargando = false;
  mensaje = '';
  mensajeTipo: AlertType = 'success';
  nuevo: InventarioPayload = this.crearFormulario();

  ngOnInit(): void {
    this.cargar();

    this.api.cambios$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cargar());
  }

  cargar(): void {
    this.cargando = true;

    forkJoin({
      inventario: this.api.getInventario(),
      productos: this.api.getProductos(),
    })
      .pipe(finalize(() => (this.cargando = false)))
      .subscribe({
        next: ({ inventario, productos }) => {
          this.inventario = inventario;
          this.productos = productos;
        },
        error: (error) => {
          this.mostrar(getErrorMessage(error, 'No fue posible cargar el inventario.'), 'error');
        },
      });
  }

  guardar(): void {
    if (!this.nuevo.id_Producto || (this.nuevo.cantidad ?? 0) <= 0) {
      this.mostrar('Selecciona un producto y una cantidad valida.', 'error');
      return;
    }

    const inventario = { ...this.nuevo };
    this.cerrarFormulario();

    this.api.agregarInventario(inventario).subscribe({
      next: () => {
        this.mostrar('Inventario actualizado correctamente.', 'success');
        this.cargar();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.nuevo = inventario;
        this.mostrarForm = true;
        this.mostrar(getErrorMessage(error, 'No fue posible actualizar el inventario.'), 'error');
      },
    });
  }

  stockClass(cantidad: number): string {
    if (cantidad <= 0) {
      return 'badge-red';
    }

    if (cantidad <= 5) {
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

  private crearFormulario(): InventarioPayload {
    return {
      id_Producto: null,
      cantidad: null,
    };
  }

  private mostrar(msg: string, tipo: AlertType): void {
    this.mensaje = msg;
    this.mensajeTipo = tipo;
    window.setTimeout(() => (this.mensaje = ''), 3000);
  }
}
