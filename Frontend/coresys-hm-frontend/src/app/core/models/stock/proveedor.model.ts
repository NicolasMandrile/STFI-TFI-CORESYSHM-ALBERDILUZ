export interface Proveedor {
  id: number;
  razonSocial: string;
  cuit: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  contacto?: string;
}

export interface CreateProveedor {
  razonSocial: string;
  cuit: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  contacto?: string;
}
