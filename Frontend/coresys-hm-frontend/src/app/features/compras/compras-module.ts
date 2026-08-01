import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NuevaCompraComponent } from './nueva-compra/nueva-compra';
import { HistorialComprasComponent } from './historial/historial-compras';
import { ProveedoresComponent } from './proveedores/proveedores';

const routes: Routes = [
  { path: '',            component: HistorialComprasComponent },
  { path: 'nueva',       component: NuevaCompraComponent },
  { path: 'proveedores', component: ProveedoresComponent },
];

@NgModule({
  declarations: [NuevaCompraComponent, HistorialComprasComponent, ProveedoresComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class ComprasModule { }
