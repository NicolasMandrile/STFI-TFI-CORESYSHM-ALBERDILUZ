import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateProveedor, Proveedor } from '../models/stock/proveedor.model';
import { HistorialCambio } from '../models/common/historial-cambio.model';

@Injectable({ providedIn: 'root' })
export class ProveedorService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/proveedores`;

  getAll(): Observable<ApiResponse<Proveedor[]>> {
    return this.http.get<ApiResponse<Proveedor[]>>(this.base);
  }

  create(dto: CreateProveedor): Observable<ApiResponse<Proveedor>> {
    return this.http.post<ApiResponse<Proveedor>>(this.base, dto);
  }

  update(id: number, dto: CreateProveedor): Observable<ApiResponse<Proveedor>> {
    return this.http.put<ApiResponse<Proveedor>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.base}/${id}`);
  }

  getHistorial(id: number): Observable<ApiResponse<HistorialCambio[]>> {
    return this.http.get<ApiResponse<HistorialCambio[]>>(`${this.base}/${id}/historial`);
  }
}

