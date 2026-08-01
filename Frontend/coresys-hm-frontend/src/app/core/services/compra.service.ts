import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Compra, CreateCompra } from '../models/compras/compra.model';

@Injectable({ providedIn: 'root' })
export class CompraService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/compras`;

  getAll(): Observable<ApiResponse<Compra[]>> {
    return this.http.get<ApiResponse<Compra[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<Compra>> {
    return this.http.get<ApiResponse<Compra>>(`${this.base}/${id}`);
  }

  create(dto: CreateCompra): Observable<ApiResponse<Compra>> {
    return this.http.post<ApiResponse<Compra>>(this.base, dto);
  }

  anular(id: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.base}/${id}/anular`, {});
  }
}

