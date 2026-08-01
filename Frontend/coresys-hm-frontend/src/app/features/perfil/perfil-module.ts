import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { PerfilComponent } from './perfil';

const routes: Routes = [
  { path: '', component: PerfilComponent }
];

@NgModule({
  declarations: [PerfilComponent],
  imports: [CommonModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class PerfilModule {}
