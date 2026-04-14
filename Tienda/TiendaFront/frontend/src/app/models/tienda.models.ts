export type AlertType = 'success' | 'error';

export interface Producto {
  id_Producto: number;
  nombre_Producto: string;
  marca: string | null;
  talla: string | null;
  color: string | null;
  precio: number | null;
  id_Proveedor: number | null;
  proveedor: string | null;
  id_Inventario: number | null;
  stock: number;
}

export interface ProductoPayload {
  nombre_Producto: string;
  marca: string | null;
  talla: string | null;
  color: string | null;
  precio: number | null;
  id_Proveedor: number | null;
  stock: number | null;
}

export interface Cliente {
  id_Cliente: number;
  nombre_Cliente: string;
  apellido_Cliente: string | null;
  telefono: number | null;
  correo: string | null;
  direccion: string | null;
  id_Ventas: number | null;
}

export interface ClientePayload {
  nombre_Cliente: string;
  apellido_Cliente: string | null;
  telefono: number | null;
  correo: string | null;
  direccion: string | null;
  id_Ventas: number | null;
}

export interface Proveedor {
  id_Proveedor: number;
  nombre_Proveedor: string;
  telefono: number | null;
  correo: string | null;
  direccion: string | null;
  id_Producto: number | null;
  producto: string | null;
}

export interface ProveedorPayload {
  nombre_Proveedor: string;
  telefono: number | null;
  correo: string | null;
  direccion: string | null;
  id_Producto: number | null;
}

export interface Inventario {
  id_Inventario: number;
  id_Producto: number | null;
  nombre_Producto: string | null;
  marca: string | null;
  color: string | null;
  talla: string | null;
  proveedor: string | null;
  precio: number | null;
  cantidad: number;
}

export interface InventarioPayload {
  id_Producto: number | null;
  cantidad: number | null;
}

export interface Venta {
  id_Ventas: number;
  fecha_Venta: string | null;
  total: number | null;
  cantidad: number;
  id_Cliente: number | null;
  cliente: string | null;
  id_Producto: number | null;
  producto: string | null;
  precioUnitario: number | null;
  stockRestante: number;
}

export interface VentaPayload {
  id_Cliente: number | null;
  id_Producto: number | null;
  cantidad: number;
  fecha_Venta: string | null;
}
