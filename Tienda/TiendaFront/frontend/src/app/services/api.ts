import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  Cliente,
  ClientePayload,
  Inventario,
  InventarioPayload,
  Producto,
  ProductoPayload,
  Proveedor,
  ProveedorPayload,
  Venta,
  VentaPayload,
} from '../models/tienda.models';

// Servicio central de Angular: concentra las llamadas HTTP y notifica cambios entre modulos.
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5186/api';
  private readonly cambiosSubject = new Subject<void>();

  readonly cambios$ = this.cambiosSubject.asObservable();

  private limpiarTexto<T>(valor: T): T {
    if (typeof valor === 'string') {
      return valor.trim() as T;
    }

    if (Array.isArray(valor)) {
      return valor.map((item) => this.limpiarTexto(item)) as T;
    }

    if (valor && typeof valor === 'object') {
      return Object.fromEntries(
        Object.entries(valor as Record<string, unknown>).map(([clave, contenido]) => [
          clave,
          this.limpiarTexto(contenido),
        ]),
      ) as T;
    }

    return valor;
  }

  private getLista<T>(resource: string): Observable<T[]> {
    return this.http
      .get<T[]>(`${this.baseUrl}/${resource}`)
      .pipe(map((data) => this.limpiarTexto(data)));
  }

  private post<TPayload, TResponse>(resource: string, payload: TPayload): Observable<TResponse> {
    return this.http
      .post<TResponse>(`${this.baseUrl}/${resource}`, payload)
      .pipe(map((data) => this.limpiarTexto(data)));
  }

  private put<TPayload, TResponse>(
    resource: string,
    id: number,
    payload: TPayload,
  ): Observable<TResponse> {
    return this.http
      .put<TResponse>(`${this.baseUrl}/${resource}/${id}`, payload)
      .pipe(map((data) => this.limpiarTexto(data)));
  }

  private delete(resource: string, id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${resource}/${id}`);
  }

  getProductos(): Observable<Producto[]> {
    return this.getLista<Producto>('Productos');
  }

  agregarProducto(payload: ProductoPayload): Observable<Producto> {
    return this.post<ProductoPayload, Producto>('Productos', payload);
  }

  actualizarProducto(id: number, payload: ProductoPayload): Observable<Producto> {
    return this.put<ProductoPayload, Producto>('Productos', id, payload);
  }

  eliminarProducto(id: number): Observable<void> {
    return this.delete('Productos', id);
  }

  getClientes(): Observable<Cliente[]> {
    return this.getLista<Cliente>('Clientes');
  }

  agregarCliente(payload: ClientePayload): Observable<Cliente> {
    return this.post<ClientePayload, Cliente>('Clientes', payload);
  }

  actualizarCliente(id: number, payload: ClientePayload): Observable<Cliente> {
    return this.put<ClientePayload, Cliente>('Clientes', id, payload);
  }

  eliminarCliente(id: number): Observable<void> {
    return this.delete('Clientes', id);
  }

  getProveedores(): Observable<Proveedor[]> {
    return this.getLista<Proveedor>('Proveedores');
  }

  agregarProveedor(payload: ProveedorPayload): Observable<Proveedor> {
    return this.post<ProveedorPayload, Proveedor>('Proveedores', payload);
  }

  actualizarProveedor(id: number, payload: ProveedorPayload): Observable<Proveedor> {
    return this.put<ProveedorPayload, Proveedor>('Proveedores', id, payload);
  }

  eliminarProveedor(id: number): Observable<void> {
    return this.delete('Proveedores', id);
  }

  getInventario(): Observable<Inventario[]> {
    return this.getLista<Inventario>('Inventarios');
  }

  agregarInventario(payload: InventarioPayload): Observable<Inventario> {
    return this.post<InventarioPayload, Inventario>('Inventarios', payload);
  }

  actualizarInventario(id: number, payload: InventarioPayload): Observable<Inventario> {
    return this.put<InventarioPayload, Inventario>('Inventarios', id, payload);
  }

  getVentas(): Observable<Venta[]> {
    return this.getLista<Venta>('Ventas');
  }

  agregarVenta(payload: VentaPayload): Observable<Venta> {
    return this.post<VentaPayload, Venta>('Ventas', payload);
  }

  eliminarVenta(id: number): Observable<void> {
    return this.delete('Ventas', id);
  }

  notificarCambios(): void {
    this.cambiosSubject.next();
  }
}
