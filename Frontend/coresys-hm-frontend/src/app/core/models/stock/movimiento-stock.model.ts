export interface MovimientoStock {
  id: number;
  productoId: number;
  productoNombre: string;
  productoCodigo: string;
  cantidad: number;
  tipoMovimiento: string;
  observacion?: string;
  stockAnterior: number;
  stockPosterior: number;
  fechaCreacion: string;
}

export interface CreateMovimientoStock {
  productoId: number;
  cantidad: number;
  tipoMovimiento: string;
  observacion?: string;
}

export const TIPOS_MOVIMIENTO = [
  { value: 'ENTRADA',  label: 'Entrada' },
  { value: 'SALIDA',   label: 'Salida' },
  { value: 'AJUSTE',   label: 'Ajuste' },
  { value: 'PERDIDA',  label: 'Pérdida' },
  { value: 'RECUENTO', label: 'Recuento físico' },
];
