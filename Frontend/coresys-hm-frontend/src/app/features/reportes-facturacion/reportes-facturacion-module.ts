import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ReportesFacturacionComponent } from './reportes-facturacion';

const routes: Routes = [
  { path: '', component: ReportesFacturacionComponent }
];

@NgModule({
  declarations: [ReportesFacturacionComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class ReportesFacturacionModule { }
