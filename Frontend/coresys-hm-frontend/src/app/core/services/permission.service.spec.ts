import { BehaviorSubject } from 'rxjs';
import { PermissionService } from './permission.service';
import { AuthService } from './auth.service';
import { UsuarioActual } from '../models/auth/auth.model';

describe('PermissionService', () => {
  function crearServicio(user: UsuarioActual | null) {
    const authServiceMock = { currentUser$: new BehaviorSubject<UsuarioActual | null>(user) };
    return new PermissionService(authServiceMock as unknown as AuthService);
  }

  it('Administrador tiene cualquier permiso, incluso uno que no está en su lista', () => {
    const service = crearServicio({
      nombreUsuario: 'admin', email: 'admin@test.com', rol: 'Administrador', permisos: []
    });

    expect(service.has('usuarios.delete')).toBeTrue();
    expect(service.has('cualquier.permiso.inventado')).toBeTrue();
  });

  it('rol no-Administrador solo tiene los permisos que vinieron en el token', () => {
    const service = crearServicio({
      nombreUsuario: 'oper', email: 'oper@test.com', rol: 'Administrativo', permisos: ['ventas.view']
    });

    expect(service.has('ventas.view')).toBeTrue();
    expect(service.has('usuarios.view')).toBeFalse();
  });

  it('sin usuario logueado => ningún permiso', () => {
    const service = crearServicio(null);

    expect(service.has('ventas.view')).toBeFalse();
  });
});
