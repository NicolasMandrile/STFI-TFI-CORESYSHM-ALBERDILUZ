import { HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('AuthInterceptor', () => {
  function crearInterceptor() {
    const authServiceMock = { getToken: jasmine.createSpy('getToken').and.returnValue('token123'), logout: jasmine.createSpy('logout') };
    const routerMock = { navigate: jasmine.createSpy('navigate') };

    const interceptor = new AuthInterceptor(
      authServiceMock as unknown as AuthService,
      routerMock as unknown as Router
    );
    return { interceptor, authServiceMock, routerMock };
  }

  it('adjunta el Bearer token a la request saliente', () => {
    const { interceptor } = crearInterceptor();
    const req = new HttpRequest('GET', '/api/algo');
    let reqRecibida!: HttpRequest<unknown>;
    const next = { handle: (r: HttpRequest<unknown>) => { reqRecibida = r; return of({} as any); } };

    interceptor.intercept(req, next).subscribe();

    expect(reqRecibida.headers.get('Authorization')).toBe('Bearer token123');
  });

  it('401 => hace logout', () => {
    const { interceptor, authServiceMock } = crearInterceptor();
    const req = new HttpRequest('GET', '/api/algo');
    const error = new HttpErrorResponse({ status: 401 });
    const next = { handle: () => throwError(() => error) };

    interceptor.intercept(req, next).subscribe({ error: () => {} });

    expect(authServiceMock.logout).toHaveBeenCalled();
  });

  it('403 => redirige a /acceso-denegado sin hacer logout', () => {
    const { interceptor, authServiceMock, routerMock } = crearInterceptor();
    const req = new HttpRequest('GET', '/api/algo');
    const error = new HttpErrorResponse({ status: 403 });
    const next = { handle: () => throwError(() => error) };

    interceptor.intercept(req, next).subscribe({ error: () => {} });

    expect(routerMock.navigate).toHaveBeenCalledWith(['/acceso-denegado']);
    expect(authServiceMock.logout).not.toHaveBeenCalled();
  });
});
