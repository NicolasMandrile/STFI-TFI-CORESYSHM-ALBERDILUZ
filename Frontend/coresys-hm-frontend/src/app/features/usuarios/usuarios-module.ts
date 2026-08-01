import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { UsuariosListComponent } from './usuarios-list/usuarios-list';
import { SharedModule } from '../../shared/shared-module';

const routes: Routes = [
  { path: '', component: UsuariosListComponent }
];

@NgModule({
  declarations: [UsuariosListComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes), SharedModule]
})
export class UsuariosModule {}
