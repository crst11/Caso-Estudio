import { Component } from '@angular/core';
import { TiendaComponent } from './tienda/tienda';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [TiendaComponent],
  template: '<app-tienda></app-tienda>',
})
export class AppComponent {}