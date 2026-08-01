import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AuditoriaListComponent } from './auditoria-list/auditoria-list';

const routes: Routes = [
  { path: '', component: AuditoriaListComponent }
];

@NgModule({
  declarations: [AuditoriaListComponent],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class AuditoriaModule {}
