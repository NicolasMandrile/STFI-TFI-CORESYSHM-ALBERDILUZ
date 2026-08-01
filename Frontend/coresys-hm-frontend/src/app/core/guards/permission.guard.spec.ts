import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { permissionGuard } from './permission.guard';
import { PermissionService } from '../services/permission.service';

describe('permissionGuard', () => {
  function ejecutarGuard(permisoRequerido: string, tienePermiso: boolean) {
    const permissionServiceMock = { has: jasmine.createSpy('has').and.returnValue(tienePermiso) };
    const urlTree = {} as UrlTree;
    const routerMock = { createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue(urlTree) };

    TestBed.configureTestingModule({
      providers: [
        { provide: PermissionService, useValue: permissionServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });

    const route = { data: { permission: permisoRequerido } } as any;
    const resultado = TestBed.runInInjectionContext(() => permissionGuard(route, {} as any));
    return { resultado, routerMock, urlTree, permissionServiceMock };
  }

  it('permiso no asignado al rol => redirige a /acceso-denegado', () => {
    const { resultado, routerMock, urlTree } = ejecutarGuard('usuarios.view', false);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/acceso-denegado']);
    expect(resultado).toBe(urlTree);
  });

  it('rol con el permiso requerido => permite el acceso', () => {
    const { resultado } = ejecutarGuard('usuarios.view', true);

    expect(resultado).toBe(true);
  });

  it('Administrador (PermissionService.has siempre true) => accede siempre', () => {
    // PermissionService.has ya contempla "Administrador siempre true" -- acá se verifica que el
    // guard confía en esa respuesta sin lógica de rol propia.
    const { resultado } = ejecutarGuard('security.manage', true);

    expect(resultado).toBe(true);
  });

  it('Cliente sin el permiso de una ruta administrativa => 403 (acceso-denegado)', () => {
    const { resultado, routerMock } = ejecutarGuard('security.manage', false);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/acceso-denegado']);
    expect(resultado).not.toBe(true);
  });
});
