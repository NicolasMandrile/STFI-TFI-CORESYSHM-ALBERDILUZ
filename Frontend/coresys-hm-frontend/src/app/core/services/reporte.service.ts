import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  ComprasPorPeriodo,
  EvolucionPrecioCompra,
  FiltrosReporte,
  FiltrosReporteCompras,
  MargenProducto,
  ProductoMasComprado,
  ProductoMasVendido,
  RankingCliente,
  RankingProveedor,
  SugerenciaReposicion,
  TicketPromedio,
  VentasPorPeriodo
} from '../models/reportes/reporte.model';

@Injectable({ providedIn: 'root' })
export class ReporteService {
  private readonly http = inject(HttpClient);
  private readonly baseVentas  = `${environment.apiUrl}/reportes/ventas`;
  private readonly baseCompras = `${environment.apiUrl}/reportes/compras`;

  // ── Helpers ────────────────────────────────────────────────────────────────
  private params(f: FiltrosReporte): HttpParams {
    let p = new HttpParams()
      .set('desde', f.desde)
      .set('hasta', f.hasta);
    if (f.topN        != null) p = p.set('topN',         f.topN);
    if (f.ordenarPor  != null) p = p.set('ordenarPor',   f.ordenarPor);
    if (f.granularidad!= null) p = p.set('granularidad', f.granularidad);
    return p;
  }

  private comprasParams(f: FiltrosReporteCompras): HttpParams {
    let p = new HttpParams();
    if (f.desde       != null) p = p.set('desde',        f.desde);
    if (f.hasta       != null) p = p.set('hasta',        f.hasta);
    if (f.topN        != null) p = p.set('topN',         f.topN);
    if (f.ordenarPor  != null) p = p.set('ordenarPor',   f.ordenarPor);
    if (f.granularidad!= null) p = p.set('granularidad', f.granularidad);
    if (f.productoId  != null) p = p.set('productoId',   f.productoId);
    if (f.proveedorId != null) p = p.set('proveedorId',  f.proveedorId);
    return p;
  }

  // ── Reportes de Ventas ─────────────────────────────────────────────────────
  productosMasVendidos(f: FiltrosReporte): Observable<ApiResponse<ProductoMasVendido[]>> {
    return this.http.get<ApiResponse<ProductoMasVendido[]>>(
      `${this.baseVentas}/productos-mas-vendidos`, { params: this.params(f) });
  }

  ventasPorPeriodo(f: FiltrosReporte): Observable<ApiResponse<VentasPorPeriodo[]>> {
    return this.http.get<ApiResponse<VentasPorPeriodo[]>>(
      `${this.baseVentas}/por-periodo`, { params: this.params(f) });
  }

  rankingClientes(f: FiltrosReporte): Observable<ApiResponse<RankingCliente[]>> {
    return this.http.get<ApiResponse<RankingCliente[]>>(
      `${this.baseVentas}/ranking-clientes`, { params: this.params(f) });
  }

  ticketPromedio(f: FiltrosReporte): Observable<ApiResponse<TicketPromedio>> {
    return this.http.get<ApiResponse<TicketPromedio>>(
      `${this.baseVentas}/ticket-promedio`, { params: this.params(f) });
  }

  margenProductos(f: FiltrosReporte): Observable<ApiResponse<MargenProducto[]>> {
    return this.http.get<ApiResponse<MargenProducto[]>>(
      `${this.baseVentas}/margen-productos`, { params: this.params(f) });
  }

  // ── Reportes de Compras ────────────────────────────────────────────────────
  comprasPorPeriodo(f: FiltrosReporteCompras): Observable<ApiResponse<ComprasPorPeriodo[]>> {
    return this.http.get<ApiResponse<ComprasPorPeriodo[]>>(
      `${this.baseCompras}/por-periodo`, { params: this.comprasParams(f) });
  }

  rankingProveedores(f: FiltrosReporteCompras): Observable<ApiResponse<RankingProveedor[]>> {
    return this.http.get<ApiResponse<RankingProveedor[]>>(
      `${this.baseCompras}/ranking-proveedores`, { params: this.comprasParams(f) });
  }

  productosMasComprados(f: FiltrosReporteCompras): Observable<ApiResponse<ProductoMasComprado[]>> {
    return this.http.get<ApiResponse<ProductoMasComprado[]>>(
      `${this.baseCompras}/productos-mas-comprados`, { params: this.comprasParams(f) });
  }

  evolucionPrecioCompra(f: FiltrosReporteCompras): Observable<ApiResponse<EvolucionPrecioCompra[]>> {
    return this.http.get<ApiResponse<EvolucionPrecioCompra[]>>(
      `${this.baseCompras}/evolucion-precio`, { params: this.comprasParams(f) });
  }

  sugerenciasReposicion(f: FiltrosReporteCompras): Observable<ApiResponse<SugerenciaReposicion[]>> {
    return this.http.get<ApiResponse<SugerenciaReposicion[]>>(
      `${this.baseCompras}/sugerencias-reposicion`, { params: this.comprasParams(f) });
  }
}
