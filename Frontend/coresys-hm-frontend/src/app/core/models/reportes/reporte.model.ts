export interface ProductoMasVendido {
  productoId:      number;
  codigo:          string;
  nombre:          string;
  cantidadVendida: number;
  totalFacturado:  number;
}

export interface VentasPorPeriodo {
  periodo:        string;
  cantidadVentas: number;
  totalFacturado: number;
}

export interface RankingCliente {
  clienteId:       number;
  nombre:          string;
  apellido:        string;
  cantidadCompras: number;
  montoTotal:      number;
  ticketPromedio:  number;
}

export interface TicketPromedio {
  cantidadVentas: number;
  totalFacturado: number;
  ticketPromedio: number;
}

export interface MargenProducto {
  productoId:      number;
  codigo:          string;
  nombre:          string;
  cantidadVendida: number;
  ingresoTotal:    number;
  costoTotal:      number;
  margenTotal:     number;
  margenPorcentual:number;
}

export interface FiltrosReporte {
  desde:       string;
  hasta:       string;
  topN?:       number;
  ordenarPor?: string;
  granularidad?:string;
}

// ── Modelos para Reportes de Compras ───────────────────────────────────────

export interface ComprasPorPeriodo {
  periodo:        string;
  cantidadCompras:number;
  totalGastado:   number;
}

export interface RankingProveedor {
  proveedorId:    number;
  razonSocial:    string;
  cuit:           string;
  cantidadCompras:number;
  montoTotal:     number;
  ticketPromedio: number;
}

export interface ProductoMasComprado {
  productoId:      number;
  codigo:          string;
  nombre:          string;
  cantidadComprada:number;
  montoTotal:      number;
}

export interface EvolucionPrecioCompra {
  fecha:          string;
  precioUnitario: number;
  numeroCompra:   string;
}

export interface SugerenciaReposicion {
  productoId:        number;
  codigo:            string;
  nombre:            string;
  stockActual:       number;
  stockMinimo:       number;
  diferencia:        number;
  proveedorNombre:   string;
  ultimoPrecioCompra:number;
}

export interface FiltrosReporteCompras {
  desde?:       string;
  hasta?:       string;
  topN?:        number;
  ordenarPor?:  string;
  granularidad?:string;
  productoId?:  number;
  proveedorId?: number;
}

// ── Modelos para Reportes de Facturación ───────────────────────────────────

export interface FacturacionPorPeriodo {
  periodo:              string;
  cantidadComprobantes: number;
  neto:                 number;
  iva:                  number;
  total:                number;
}

export interface DesempenoCliente {
  clienteId:            number;
  clienteNombre:        string;
  cantidadComprobantes: number;
  montoTotal:           number;
  ticketPromedio:        number;
}

export interface DesempenoProducto {
  productoId:        number;
  codigo:            string;
  nombre:            string;
  cantidadFacturada: number;
  montoTotal:        number;
}

export interface FiltrosReporteFacturacion {
  desde?:             string;
  hasta?:             string;
  topN?:              number;
  granularidad?:      string;
  puntoVentaId?:      number;
  tipoComprobanteId?: number;
  clienteId?:         number;
  productoId?:        number;
}
