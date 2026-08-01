export interface Cliente {
  id: number;
  nombre: string;
  apellido: string;
  dni?: string;
  cuit?: string;
  email?: string;
  telefono?: string;
  direccion?: string;
  localidad?: string;
}

export interface CreateCliente {
  nombre: string;
  apellido: string;
  dni?: string;
  cuit?: string;
  email?: string;
  telefono?: string;
  direccion?: string;
  localidad?: string;
}
