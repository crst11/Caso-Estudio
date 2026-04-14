import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { AlertType, Cliente, ClientePayload } from '../../models/tienda.models';
import { ApiService } from '../../services/api';
import { getErrorMessage } from '../../shared/http-error';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clientes.html',
  styleUrl: './clientes.css',
})
export class ClientesComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  clientes: Cliente[] = [];
  mostrarForm = false;
  cargando = false;
  mensaje = '';
  mensajeTipo: AlertType = 'success';
  nuevo: ClientePayload = this.crearFormulario();

  ngOnInit(): void {
    this.cargar();

    this.api.cambios$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.cargar());
  }

  cargar(): void {
    this.cargando = true;

    this.api
      .getClientes()
      .pipe(finalize(() => (this.cargando = false)))
      .subscribe({
        next: (clientes) => {
          this.clientes = clientes;
        },
        error: (error) => {
          this.mostrar(getErrorMessage(error, 'No fue posible cargar los clientes.'), 'error');
        },
      });
  }

  guardar(): void {
    if (!this.nuevo.nombre_Cliente.trim()) {
      this.mostrar('El nombre es obligatorio.', 'error');
      return;
    }

    const cliente = { ...this.nuevo };
    this.cerrarFormulario();

    this.api.agregarCliente(cliente).subscribe({
      next: () => {
        this.mostrar('Cliente guardado correctamente.', 'success');
        this.cargar();
      },
      error: (error) => {
        this.nuevo = cliente;
        this.mostrarForm = true;
        this.mostrar(getErrorMessage(error, 'No fue posible guardar el cliente.'), 'error');
      },
    });
  }

  eliminar(id: number): void {
    if (!confirm('Eliminar este cliente?')) {
      return;
    }

    this.api.eliminarCliente(id).subscribe({
      next: () => {
        this.mostrar('Cliente eliminado.', 'success');
        this.cargar();
      },
      error: (error) => {
        this.mostrar(getErrorMessage(error, 'No fue posible eliminar el cliente.'), 'error');
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

  private crearFormulario(): ClientePayload {
    return {
      nombre_Cliente: '',
      apellido_Cliente: '',
      telefono: null,
      correo: '',
      direccion: '',
      id_Ventas: null,
    };
  }

  private mostrar(msg: string, tipo: AlertType): void {
    this.mensaje = msg;
    this.mensajeTipo = tipo;
    window.setTimeout(() => (this.mensaje = ''), 3000);
  }
}
