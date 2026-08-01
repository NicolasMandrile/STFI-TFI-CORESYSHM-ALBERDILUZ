import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ReportesComprasComponent } from './reportes-compras';

const routes: Routes = [
  { path: '', component: ReportesComprasComponent }
];

@NgModule({
  declarations: [ReportesComprasComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class ReportesComprasModule { }
