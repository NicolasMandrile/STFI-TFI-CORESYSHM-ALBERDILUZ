import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout';
import { permissionGuard } from '../../core/guards/permission.guard';
import { SharedModule } from '../../shared/shared-module';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('../dashboard/dashboard-module').then(m => m.DashboardModule)
      },
      {
        path: 'stock',
        loadChildren: () => import('../stock/stock-module').then(m => m.StockModule)
      },
      {
        path: 'ventas',
        loadChildren: () => import('../ventas/ventas-module').then(m => m.VentasModule)
      },
      {
        path: 'compras',
        loadChildren: () => import('../compras/compras-module').then(m => m.ComprasModule)
      },
      {
        path: 'facturacion',
        loadChildren: () => import('../facturacion/facturacion-module').then(m => m.FacturacionModule)
      },
      {
        path: 'reportes',
        loadChildren: () => import('../reportes/reportes-module').then(m => m.ReportesModule)
      },
      {
        path: 'reportes-compras',
        loadChildren: () => import('../reportes-compras/reportes-compras-module').then(m => m.ReportesComprasModule)
      },
      {
        path: 'reportes-facturacion',
        loadChildren: () => import('../reportes-facturacion/reportes-facturacion-module').then(m => m.ReportesFacturacionModule)
      },
      {
        path: 'usuarios',
        canActivate: [permissionGuard],
        data: { permission: 'usuarios.view' },
        loadChildren: () => import('../usuarios/usuarios-module').then(m => m.UsuariosModule)
      },
      {
        path: 'roles',
        canActivate: [permissionGuard],
        data: { permission: 'security.view' },
        loadChildren: () => import('../roles/roles-module').then(m => m.RolesModule)
      },
      {
        path: 'auditoria',
        canActivate: [permissionGuard],
        data: { permission: 'security.view' },
        loadChildren: () => import('../auditoria/auditoria-module').then(m => m.AuditoriaModule)
      },
      {
        path: 'perfil',
        loadChildren: () => import('../perfil/perfil-module').then(m => m.PerfilModule)
      },
      {
        path: 'mi-area',
        loadChildren: () => import('../perfil/perfil-module').then(m => m.PerfilModule)
      },
      {
        path: 'acceso-denegado',
        loadChildren: () => import('../acceso-denegado/acceso-denegado-module').then(m => m.AccesoDenegadoModule)
      }
    ]
  }
];

@NgModule({
  declarations: [LayoutComponent],
  imports: [CommonModule, RouterModule.forChild(routes), SharedModule]
})
export class LayoutModule { }
