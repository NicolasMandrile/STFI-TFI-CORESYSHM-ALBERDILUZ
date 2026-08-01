import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { RolesListComponent } from './roles-list/roles-list';

const routes: Routes = [
  { path: '', component: RolesListComponent }
];

@NgModule({
  declarations: [RolesListComponent],
  imports: [CommonModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class RolesModule {}
