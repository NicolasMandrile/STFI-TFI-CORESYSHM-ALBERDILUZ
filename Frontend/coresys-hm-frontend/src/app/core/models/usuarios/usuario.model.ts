export interface Usuario {
  id: number;
  nombreUsuario: string;
  email: string;
  nombre: string;
  apellido: string;
  rol: string;
  isActive: boolean;
  ultimoAcceso: string | null;
  createdAt: string;
}

export interface CreateUsuarioRequest {
  nombreUsuario: string;
  email: string;
  password: string;
  nombre: string;
  apellido: string;
  rol: string;
}

export interface UpdateUsuarioRequest {
  nombre: string;
  apellido: string;
  rol: string;
}
