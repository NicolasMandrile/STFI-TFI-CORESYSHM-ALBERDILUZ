import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  Factura, CreateFactura, VentaFacturable, TipoComprobante, PuntoVenta
} from '../models/facturacion/factura.model';

@Injectable({ providedIn: 'root' })
export class FacturaService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/facturas`;

  getAll(): Observable<ApiResponse<Factura[]>> { return this.http.get<ApiResponse<Factura[]>>(this.base); }
  getById(id: number): Observable<ApiResponse<Factura>> { return this.http.get<ApiResponse<Factura>>(`${this.base}/${id}`); }
  emitir(dto: CreateFactura): Observable<ApiResponse<Factura>> { return this.http.post<ApiResponse<Factura>>(this.base, dto); }
  pagar(id: number): Observable<ApiResponse<boolean>> { return this.http.post<ApiResponse<boolean>>(`${this.base}/${id}/pagar`, {}); }
  anular(id: number): Observable<ApiResponse<boolean>> { return this.http.post<ApiResponse<boolean>>(`${this.base}/${id}/anular`, {}); }

  getVentasFacturables(clienteId?: number): Observable<ApiResponse<VentaFacturable[]>> {
    const query = clienteId ? `?clienteId=${clienteId}` : '';
    return this.http.get<ApiResponse<VentaFacturable[]>>(`${this.base}/ventas-facturables${query}`);
  }

  getSaldoFacturar(ventaId: number): Observable<ApiResponse<VentaFacturable>> {
    return this.http.get<ApiResponse<VentaFacturable>>(`${this.base}/ventas/${ventaId}/saldo`);
  }

  getTiposComprobante(): Observable<ApiResponse<TipoComprobante[]>> {
    return this.http.get<ApiResponse<TipoComprobante[]>>(`${this.base}/tipos-comprobante`);
  }

  getPuntosVenta(): Observable<ApiResponse<PuntoVenta[]>> {
    return this.http.get<ApiResponse<PuntoVenta[]>>(`${this.base}/puntos-venta`);
  }
}
