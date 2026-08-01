import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FacturacionHomeComponent } from './facturacion-home';

const routes: Routes = [
  { path: '', component: FacturacionHomeComponent }
];

@NgModule({
  declarations: [FacturacionHomeComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class FacturacionModule { }
