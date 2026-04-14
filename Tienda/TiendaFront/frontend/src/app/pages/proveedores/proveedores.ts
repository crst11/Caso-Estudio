import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import { AlertType, Producto, Proveedor, ProveedorPayload } from '../../models/tienda.models';
import { ApiService } from '../../services/api';
import { getErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-proveedores',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './proveedores.html',
  styleUrl: './proveedores.css',
})
export class ProveedoresComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  proveedores: Proveedor[] = [];
  productos: Producto[] = [];
  mostrarForm = false;
  cargando = false;
  mensaje = '';
  mensajeTipo: AlertType = 'success';
  nuevo: ProveedorPayload = this.crearFormulario();

  ngOnInit(): void {
    this.cargar();

    this.api.cambios$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cargar());
  }

  cargar(): void {
    this.cargando = true;

    forkJoin({
      proveedores: this.api.getProveedores(),
      productos: this.api.getProductos(),
    })
      .pipe(finalize(() => (this.cargando = false)))
      .subscribe({
        next: ({ proveedores, productos }) => {
          this.proveedores = proveedores;
          this.productos = productos;
        },
        error: (error) => {
          this.mostrar(getErrorMessage(error, 'No fue posible cargar los proveedores.'), 'error');
        },
      });
  }

  guardar(): void {
    if (!this.nuevo.nombre_Proveedor.trim()) {
      this.mostrar('El nombre es obligatorio.', 'error');
      return;
    }

    const proveedor = { ...this.nuevo };
    this.cerrarFormulario();

    this.api.agregarProveedor(proveedor).subscribe({
      next: () => {
        this.mostrar('Proveedor guardado correctamente.', 'success');
        this.cargar();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.nuevo = proveedor;
        this.mostrarForm = true;
        this.mostrar(getErrorMessage(error, 'No fue posible guardar el proveedor.'), 'error');
      },
    });
  }

  eliminar(id: number): void {
    if (!confirm('Eliminar este proveedor?')) {
      return;
    }

    this.api.eliminarProveedor(id).subscribe({
      next: () => {
        this.mostrar('Proveedor eliminado.', 'success');
        this.cargar();
        this.api.notificarCambios();
      },
      error: (error) => {
        this.mostrar(getErrorMessage(error, 'No fue posible eliminar el proveedor.'), 'error');
      },
    });
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

  private crearFormulario(): ProveedorPayload {
    return {
      nombre_Proveedor: '',
      telefono: null,
      correo: '',
      direccion: '',
      id_Producto: null,
    };
  }

  private mostrar(msg: string, tipo: AlertType): void {
    this.mensaje = msg;
    this.mensajeTipo = tipo;
    window.setTimeout(() => (this.mensaje = ''), 3000);
  }
}
