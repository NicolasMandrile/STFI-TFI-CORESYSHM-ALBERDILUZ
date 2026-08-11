export interface DetalleFactura {
  productoId: number;
  productoNombre: string;
  detalleVentaId: number;
  cantidad: number;
  precioUnitario: number;
  impuesto: number;
  descuento: number;
  subtotal: number;
}

export interface Factura {
  id: number;
  numeroFactura: string;
  fechaEmision: string;
  fechaVencimiento?: string;
  clienteId: number;
  clienteNombre: string;
  ventaId: number;
  numeroVenta: string;
  tipoComprobanteId: number;
  tipoComprobanteDescripcion: string;
  puntoVentaId: number;
  puntoVentaDescripcion: string;
  subtotal: number;
  iva: number;
  total: number;
  estado: string;
  observaciones?: string;
  detalles: DetalleFactura[];
}

export interface CreateDetalleFactura {
  detalleVentaId: number;
  cantidad: number;
  impuesto: number;
  descuento: number;
}

export interface CreateFactura {
  ventaId: number;
  tipoComprobanteId: number;
  puntoVentaId: number;
  fechaVencimiento?: string;
  observaciones?: string;
  idempotencyKey: string;
  detalles: CreateDetalleFactura[];
}

export interface SaldoFacturarLinea {
  detalleVentaId: number;
  productoId: number;
  productoNombre: string;
  precioUnitario: number;
  cantidadVenta: number;
  cantidadFacturada: number;
  cantidadPendiente: number;
}

export interface VentaFacturable {
  ventaId: number;
  numeroVenta: string;
  clienteId: number;
  clienteNombre: string;
  fecha: string;
  lineas: SaldoFacturarLinea[];
}

export interface TipoComprobante {
  id: number;
  descripcion: string;
  afectaStock: boolean;
  signoContable: string;
}

export interface PuntoVenta {
  id: number;
  descripcion: string;
}
