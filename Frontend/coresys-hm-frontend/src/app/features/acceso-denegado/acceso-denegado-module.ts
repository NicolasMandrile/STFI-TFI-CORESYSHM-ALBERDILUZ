import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AccesoDenegadoComponent } from './acceso-denegado';

const routes: Routes = [
  { path: '', component: AccesoDenegadoComponent }
];

@NgModule({
  declarations: [AccesoDenegadoComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class AccesoDenegadoModule {}
