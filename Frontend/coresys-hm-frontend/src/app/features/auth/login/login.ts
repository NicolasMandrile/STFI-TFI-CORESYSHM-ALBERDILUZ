import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.error   = '';

    this.authService.login(this.form.value).subscribe({
      next: res => {
        if (res.exitoso) {
          const destino = res.data?.rol === 'Cliente' ? '/mi-area' : '/dashboard';
          this.router.navigate([destino]);
        } else {
          this.error   = res.mensaje ?? 'Credenciales incorrectas';
          this.loading = false;
        }
      },
      error: () => {
        this.error   = 'No se pudo conectar con el servidor.';
        this.loading = false;
      }
    });
  }

  get emailCtrl()    { return this.form.get('email')!; }
  get passwordCtrl() { return this.form.get('password')!; }
}
