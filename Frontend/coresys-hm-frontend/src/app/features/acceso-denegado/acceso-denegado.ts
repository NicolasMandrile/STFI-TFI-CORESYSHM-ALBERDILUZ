import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-acceso-denegado',
  standalone: false,
  templateUrl: './acceso-denegado.html',
  styleUrl: './acceso-denegado.scss'
})
export class AccesoDenegadoComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  volver(): void {
    const destino = this.authService.currentUser?.rol === 'Cliente' ? '/mi-area' : '/dashboard';
    this.router.navigate([destino]);
  }
}
