export interface Proveedor {
  id: number;
  razonSocial: string;
  cuit: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  contacto?: string;
  condicionFiscalId?: number;
  condicionFiscalDescripcion?: string;
  completitud: number;
}

export interface CreateProveedor {
  razonSocial: string;
  cuit: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  contacto?: string;
  condicionFiscalId?: number;
}
