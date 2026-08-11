import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HistorialFacturasComponent } from './historial/historial-facturas';
import { NuevaFacturaComponent } from './nueva-factura/nueva-factura';

const routes: Routes = [
  { path: '',      component: HistorialFacturasComponent },
  { path: 'nueva', component: NuevaFacturaComponent },
];

@NgModule({
  declarations: [HistorialFacturasComponent, NuevaFacturaComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class FacturacionModule { }
