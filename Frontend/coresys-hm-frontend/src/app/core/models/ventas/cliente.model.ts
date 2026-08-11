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
  condicionFiscalId?: number;
  condicionFiscalDescripcion?: string;
  completitud: number;
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
  condicionFiscalId?: number;
}
