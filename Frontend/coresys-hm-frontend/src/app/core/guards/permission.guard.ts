import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';

/** Uso: { path: 'usuarios', canActivate: [permissionGuard], data: { permission: 'usuarios.view' } } */
export const permissionGuard: CanActivateFn = (route) => {
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  const permisoRequerido = route.data['permission'] as string;
  return permissionService.has(permisoRequerido) ? true : router.createUrlTree(['/acceso-denegado']);
};
