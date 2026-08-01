import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserAdminService } from '../../core/services/user-admin.service';
import { Usuario } from '../../core/models/usuarios/usuario.model';

@Component({
  selector: 'app-perfil',
  standalone: false,
  templateUrl: './perfil.html',
  styleUrl: './perfil.scss'
})
export class PerfilComponent implements OnInit {
  private readonly userAdminService = inject(UserAdminService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  usuario: Usuario | null = null;
  cargando = true;

  form: FormGroup = this.fb.group({
    passwordActual: ['', Validators.required],
    passwordNueva: ['', [Validators.required, Validators.minLength(8)]]
  });

  guardando = false;
  mensaje = '';
  error = '';

  ngOnInit(): void {
    this.userAdminService.me().subscribe({
      next: res => {
        this.usuario = res.data ?? null;
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  cambiarPassword(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.guardando = true;
    this.mensaje = '';
    this.error = '';

    const { passwordActual, passwordNueva } = this.form.value;
    this.userAdminService.cambiarPasswordPropio(passwordActual, passwordNueva).subscribe({
      next: res => {
        this.guardando = false;
        if (res.exitoso) {
          this.mensaje = 'Contraseña actualizada correctamente.';
          this.form.reset();
        } else {
          this.error = res.mensaje ?? 'No se pudo cambiar la contraseña.';
        }
        this.cdr.detectChanges();
      },
      error: err => {
        this.guardando = false;
        this.error = err.error?.mensaje ?? 'No se pudo cambiar la contraseña.';
        this.cdr.detectChanges();
      }
    });
  }
}
