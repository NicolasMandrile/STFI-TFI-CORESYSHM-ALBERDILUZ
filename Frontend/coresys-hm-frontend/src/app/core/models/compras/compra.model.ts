export interface DetalleCompra {
  productoId: number;
  productoNombre: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

export interface Compra {
  id: number;
  numeroCompra: string;
  fecha: string;
  proveedorId: number;
  proveedorNombre: string;
  total: number;
  estado: string;
  observaciones?: string;
  detalles: DetalleCompra[];
}

export interface CreateDetalleCompra {
  productoId: number;
  cantidad: number;
  precioUnitario: number;
}

export interface CreateCompra {
  proveedorId: number;
  observaciones?: string;
  detalles: CreateDetalleCompra[];
}
