export interface DetalleVenta {
  productoId: number;
  productoNombre: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

export interface Venta {
  id: number;
  numeroVenta: string;
  fecha: string;
  clienteId: number;
  clienteNombre: string;
  subtotal: number;
  descuento: number;
  total: number;
  estado: string;
  detalles: DetalleVenta[];
}

export interface CreateDetalleVenta {
  productoId: number;
  cantidad: number;
}

export interface CreateVenta {
  clienteId: number;
  descuento: number;
  observaciones?: string;
  detalles: CreateDetalleVenta[];
}

export interface ItemCarrito {
  productoId: number;
  productoNombre: string;
  productoCodigo: string;
  cantidad: number;
  precioUnitario: number;
  stockDisponible: number;
}
