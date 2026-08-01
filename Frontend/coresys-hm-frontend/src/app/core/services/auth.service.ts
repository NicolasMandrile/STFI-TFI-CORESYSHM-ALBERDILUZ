import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, UsuarioActual } from '../models/auth/auth.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'coresys_token';
  private readonly USER_KEY  = 'coresys_user';

  private currentUserSubject = new BehaviorSubject<UsuarioActual | null>(this.getUserFromStorage());
  currentUser$ = this.currentUserSubject.asObservable();

  get currentUser(): UsuarioActual | null {
    return this.currentUserSubject.value;
  }

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(res => {
        if (res.exitoso && res.data) {
          localStorage.setItem(this.TOKEN_KEY, res.data.token);
          const user: UsuarioActual = {
            nombreUsuario: res.data.nombreUsuario,
            email: request.email,
            rol: res.data.rol,
            permisos: res.data.permisos ?? []
          };
          localStorage.setItem(this.USER_KEY, JSON.stringify(user));
          this.currentUserSubject.next(user);
        }
      })
    );
  }

  logout(): void {
    if (this.getToken()) {
      // Fire-and-forget: solo para que quede registrado en auditoría, no bloquea el logout local.
      this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({ error: () => {} });
    }
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  private getUserFromStorage(): UsuarioActual | null {
    const stored = localStorage.getItem(this.USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
