import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClientesComponent } from '../pages/clientes/clientes';
import { InventarioComponent } from '../pages/inventario/inventario';
import { ProductosComponent } from '../pages/productos/productos';
import { ProveedoresComponent } from '../pages/proveedores/proveedores';
import { VentasComponent } from '../pages/ventas/ventas';

type Vista = 'productos' | 'inventario' | 'ventas' | 'clientes' | 'proveedores';

interface NavItem {
  readonly key: Vista;
  readonly label: string;
  readonly icon: string;
}

@Component({
  selector: 'app-tienda',
  standalone: true,
  imports: [
    CommonModule,
    ProductosComponent,
    ClientesComponent,
    ProveedoresComponent,
    VentasComponent,
    InventarioComponent,
  ],
  templateUrl: './tienda.html',
  styleUrl: './tienda.css',
})
export class TiendaComponent {
  vista: Vista = 'productos';
  sidebarOpen = false;

  readonly catalogoNav: readonly NavItem[] = [
    { key: 'productos', label: 'Productos', icon: 'P' },
    { key: 'inventario', label: 'Inventario', icon: 'I' },
  ];

  readonly gestionNav: readonly NavItem[] = [
    { key: 'ventas', label: 'Ventas', icon: 'V' },
    { key: 'clientes', label: 'Clientes', icon: 'C' },
    { key: 'proveedores', label: 'Proveedores', icon: 'R' },
  ];

  private readonly allNav = [...this.catalogoNav, ...this.gestionNav];

  get vistaLabel(): string {
    return this.allNav.find((item) => item.key === this.vista)?.label ?? 'StyleStore';
  }

  setVista(vista: Vista): void {
    this.vista = vista;

    if (window.innerWidth <= 900) {
      this.sidebarOpen = false;
    }
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }
}
