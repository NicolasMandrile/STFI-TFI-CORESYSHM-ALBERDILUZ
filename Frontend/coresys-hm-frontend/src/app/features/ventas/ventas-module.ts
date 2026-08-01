import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NuevaVentaComponent } from './nueva-venta/nueva-venta';
import { HistorialVentasComponent } from './historial/historial-ventas';
import { ClientesComponent } from './clientes/clientes';

const routes: Routes = [
  { path: '',         component: HistorialVentasComponent },
  { path: 'nueva',    component: NuevaVentaComponent },
  { path: 'clientes', component: ClientesComponent },
];

@NgModule({
  declarations: [NuevaVentaComponent, HistorialVentasComponent, ClientesComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class VentasModule { }
