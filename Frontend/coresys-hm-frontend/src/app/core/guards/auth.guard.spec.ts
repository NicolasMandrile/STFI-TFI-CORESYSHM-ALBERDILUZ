import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  function ejecutarGuard(isAuthenticated: boolean) {
    const authServiceMock = { isAuthenticated: () => isAuthenticated };
    const urlTree = {} as UrlTree;
    const routerMock = { createUrlTree: jasmine.createSpy('createUrlTree').and.returnValue(urlTree) };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });

    const resultado = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    return { resultado, routerMock, urlTree };
  }

  it('ruta privada sin token => redirige a login', () => {
    const { resultado, routerMock, urlTree } = ejecutarGuard(false);

    expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/auth/login']);
    expect(resultado).toBe(urlTree);
  });

  it('con token válido => permite el acceso', () => {
    const { resultado } = ejecutarGuard(true);

    expect(resultado).toBe(true);
  });
});
