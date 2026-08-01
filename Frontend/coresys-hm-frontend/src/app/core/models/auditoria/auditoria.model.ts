export interface AuditoriaAcceso {
  id: number;
  usuarioId: number | null;
  usuarioNombre: string | null;
  usuarioEmail: string | null;
  rolSnapshot: string | null;
  accion: string;
  ip: string | null;
  userAgent: string | null;
  timestamp: string;
  detalle: string | null;
}

export interface AuditoriaFiltro {
  usuarioId?: number;
  accion?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  pagina?: number;
  tamanoPagina?: number;
}

export interface PagedResponse<T> {
  items: T[];
  totalRegistros: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
}
