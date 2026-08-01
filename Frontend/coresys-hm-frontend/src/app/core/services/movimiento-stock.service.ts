import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateMovimientoStock, MovimientoStock } from '../models/stock/movimiento-stock.model';

@Injectable({ providedIn: 'root' })
export class MovimientoStockService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/movimientos`;

  getAll(): Observable<ApiResponse<MovimientoStock[]>> {
    return this.http.get<ApiResponse<MovimientoStock[]>>(this.base);
  }

  getByProducto(productoId: number): Observable<ApiResponse<MovimientoStock[]>> {
    return this.http.get<ApiResponse<MovimientoStock[]>>(`${this.base}/producto/${productoId}`);
  }

  registrar(dto: CreateMovimientoStock): Observable<ApiResponse<MovimientoStock>> {
    return this.http.post<ApiResponse<MovimientoStock>>(this.base, dto);
  }
}

