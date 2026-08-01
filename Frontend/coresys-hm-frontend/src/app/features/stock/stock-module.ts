import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ProductosComponent } from './productos/productos';
import { MovimientosComponent } from './movimientos/movimientos';
import { CategoriasComponent } from './categorias/categorias';

const routes: Routes = [
  { path: '',            redirectTo: 'productos', pathMatch: 'full' },
  { path: 'productos',   component: ProductosComponent },
  { path: 'movimientos', component: MovimientosComponent },
  { path: 'categorias',  component: CategoriasComponent },
];

@NgModule({
  declarations: [ProductosComponent, MovimientosComponent, CategoriasComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class StockModule { }
