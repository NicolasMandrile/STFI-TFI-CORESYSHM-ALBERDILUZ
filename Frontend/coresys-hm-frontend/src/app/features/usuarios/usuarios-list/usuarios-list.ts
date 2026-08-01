import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserAdminService } from '../../../core/services/user-admin.service';
import { RoleAdminService } from '../../../core/services/role-admin.service';
import { Usuario } from '../../../core/models/usuarios/usuario.model';
import { Role } from '../../../core/models/roles/role.model';

@Component({
  selector: 'app-usuarios-list',
  standalone: false,
  templateUrl: './usuarios-list.html',
  styleUrl: './usuarios-list.scss'
})
export class UsuariosListComponent implements OnInit {
  private readonly userAdminService = inject(UserAdminService);
  private readonly roleAdminService = inject(RoleAdminService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  usuarios: Usuario[] = [];
  roles: Role[] = [];
  cargando = true;
  error = '';

  filtroRol = '';
  filtroActivo = '';

  mostrarForm = false;
  editandoId: number | null = null;
  guardando = false;
  formError = '';

  form: FormGroup = this.fb.group({
    nombreUsuario: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    nombre: ['', Validators.required],
    apellido: ['', Validators.required],
    rol: ['', Validators.required]
  });

  ngOnInit(): void {
    this.roleAdminService.getAll().subscribe(res => {
      this.roles = res.data ?? [];
      this.cdr.detectChanges();
    });
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    const rol = this.filtroRol || undefined;
    const activo = this.filtroActivo === '' ? undefined : this.filtroActivo === 'true';
    this.userAdminService.getAll(rol, activo).subscribe({
      next: res => {
        this.usuarios = res.data ?? [];
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  abrirCrear(): void {
    this.editandoId = null;
    this.formError = '';
    this.guardando = false;
    this.form.reset({ nombreUsuario: '', email: '', password: '', nombre: '', apellido: '', rol: '' });
    this.form.get('password')!.setValidators([Validators.required, Validators.minLength(8)]);
    this.form.get('password')!.updateValueAndValidity();
    this.mostrarForm = true;
  }

  abrirEditar(usuario: Usuario): void {
    this.editandoId = usuario.id;
    this.formError = '';
    this.guardando = false;
    this.form.reset({
      nombreUsuario: usuario.nombreUsuario,
      email: usuario.email,
      password: '',
      nombre: usuario.nombre,
      apellido: usuario.apellido,
      rol: usuario.rol
    });
    this.form.get('password')!.clearValidators();
    this.form.get('password')!.updateValueAndValidity();
    this.mostrarForm = true;
  }

  cancelar(): void {
    this.mostrarForm = false;
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      const camposInvalidos = Object.entries(this.form.controls)
        .filter(([, control]) => control.invalid)
        .map(([nombre]) => nombre);
      this.formError = `Completá correctamente: ${camposInvalidos.join(', ')}.`;
      return;
    }
    this.guardando = true;
    this.formError = '';
    const valor = this.form.value;

    const onDone = (exitoso: boolean, mensaje?: string) => {
      this.guardando = false;
      if (exitoso) {
        this.mostrarForm = false;
        this.cargar();
      } else {
        this.formError = mensaje ?? 'No se pudo guardar el usuario.';
      }
      this.cdr.detectChanges();
    };

    if (this.editandoId) {
      this.userAdminService.update(this.editandoId, {
        nombre: valor.nombre, apellido: valor.apellido, rol: valor.rol
      }).subscribe({
        next: res => onDone(res.exitoso, res.mensaje),
        error: err => onDone(false, err.error?.mensaje)
      });
    } else {
      this.userAdminService.create(valor).subscribe({
        next: res => onDone(res.exitoso, res.mensaje),
        error: err => onDone(false, err.error?.mensaje)
      });
    }
  }

  toggleActivo(usuario: Usuario): void {
    this.userAdminService.toggleActivo(usuario.id, !usuario.isActive).subscribe({
      next: res => {
        if (res.exitoso) this.cargar();
        else {
          this.error = res.mensaje ?? 'No se pudo actualizar el estado.';
          this.cdr.detectChanges();
        }
      }
    });
  }

  resetearPassword(usuario: Usuario): void {
    const nueva = window.prompt(`Nueva contraseña para ${usuario.nombreUsuario} (mínimo 8 caracteres):`);
    if (!nueva) return;
    this.userAdminService.resetPassword(usuario.id, nueva).subscribe({
      next: res => {
        this.error = res.exitoso ? '' : (res.mensaje ?? 'No se pudo resetear la contraseña.');
        if (res.exitoso) window.alert('Contraseña restablecida correctamente.');
        this.cdr.detectChanges();
      }
    });
  }
}
