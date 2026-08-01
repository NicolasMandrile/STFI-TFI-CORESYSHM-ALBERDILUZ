import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HasPermissionDirective } from './has-permission.directive';
import { PermissionService } from '../../core/services/permission.service';

@Component({
  standalone: false,
  template: `<div *appHasPermission="permiso" class="protegido">Contenido protegido</div>`
})
class HostTestComponent {
  permiso = 'usuarios.view';
}

describe('HasPermissionDirective', () => {
  let fixture: ComponentFixture<HostTestComponent>;
  let permissionServiceMock: { has: jasmine.Spy };

  function crearFixture(tienePermiso: boolean) {
    permissionServiceMock = { has: jasmine.createSpy('has').and.returnValue(tienePermiso) };

    TestBed.configureTestingModule({
      declarations: [HostTestComponent, HasPermissionDirective],
      providers: [{ provide: PermissionService, useValue: permissionServiceMock }]
    });

    fixture = TestBed.createComponent(HostTestComponent);
    fixture.detectChanges();
  }

  it('con permiso => renderiza el elemento', () => {
    crearFixture(true);

    expect(fixture.nativeElement.querySelector('.protegido')).not.toBeNull();
  });

  it('sin permiso => oculta el elemento (no solo lo deshabilita)', () => {
    crearFixture(false);

    expect(fixture.nativeElement.querySelector('.protegido')).toBeNull();
  });
});
