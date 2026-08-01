import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { PermissionService } from '../../core/services/permission.service';

/**
 * *appHasPermission="'usuarios.view'" -- oculta el elemento (no solo lo deshabilita) si el
 * usuario actual no tiene el permiso. Administrador siempre lo ve (PermissionService.has
 * ya contempla esa regla). Evalúa una sola vez en ngOnInit: los permisos se fijan en el login
 * y no cambian en caliente durante la sesión (limitación conocida y aceptable para este alcance).
 */
@Directive({
  selector: '[appHasPermission]',
  standalone: false
})
export class HasPermissionDirective implements OnInit {
  @Input('appHasPermission') permiso: string | undefined = '';

  constructor(
    private templateRef: TemplateRef<unknown>,
    private viewContainer: ViewContainerRef,
    private permissionService: PermissionService
  ) {}

  ngOnInit(): void {
    this.viewContainer.clear();
    // Sin permiso especificado => sin restricción (visible para cualquier autenticado).
    const permitido = !this.permiso || this.permissionService.has(this.permiso);
    if (permitido) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }
}
