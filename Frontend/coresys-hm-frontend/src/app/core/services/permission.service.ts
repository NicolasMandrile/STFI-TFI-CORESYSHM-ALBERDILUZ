import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

const ROL_ADMINISTRADOR = 'Administrador';

/**
 * Servicio separado de AuthService a propósito: AuthService gestiona la sesión (login/logout/
 * token), este solo responde "¿tiene tal permiso?" a partir del usuario actual. Se suscribe a
 * currentUser$ (BehaviorSubject) así que refleja sincrónicamente el permiso vigente incluso
 * llamado justo después de construirse (ej. desde un guard).
 */
@Injectable({ providedIn: 'root' })
export class PermissionService {
  private permisos: string[] = [];
  private esAdministrador = false;

  constructor(private authService: AuthService) {
    this.authService.currentUser$.subscribe(user => {
      this.permisos = user?.permisos ?? [];
      this.esAdministrador = user?.rol === ROL_ADMINISTRADOR;
    });
  }

  /** Administrador siempre ve/accede a todo, sin importar el listado de permisos del token. */
  has(permiso: string): boolean {
    return this.esAdministrador || this.permisos.includes(permiso);
  }
}
