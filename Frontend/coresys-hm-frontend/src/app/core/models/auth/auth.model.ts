export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiracion: string;
  nombreUsuario: string;
  rol: string;
  permisos: string[];
}

export interface UsuarioActual {
  nombreUsuario: string;
  email: string;
  rol: string;
  permisos: string[];
}
