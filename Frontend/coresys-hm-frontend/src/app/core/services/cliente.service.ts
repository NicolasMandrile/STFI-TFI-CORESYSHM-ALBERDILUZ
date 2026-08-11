import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Cliente, CreateCliente } from '../models/ventas/cliente.model';
import { HistorialCambio } from '../models/common/historial-cambio.model';

/// CRUD completo del maestro de Clientes (api/clientes). Distinto de VentaService.getClientes()/
/// createCliente(), que pega a api/ventas/clientes y solo cubre el alta rápida desde "Nueva venta".
@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clientes`;

  getAll(): Observable<ApiResponse<Cliente[]>> { return this.http.get<ApiResponse<Cliente[]>>(this.base); }
  getById(id: number): Observable<ApiResponse<Cliente>> { return this.http.get<ApiResponse<Cliente>>(`${this.base}/${id}`); }
  create(dto: CreateCliente): Observable<ApiResponse<Cliente>> { return this.http.post<ApiResponse<Cliente>>(this.base, dto); }
  update(id: number, dto: CreateCliente): Observable<ApiResponse<Cliente>> { return this.http.put<ApiResponse<Cliente>>(`${this.base}/${id}`, dto); }
  delete(id: number): Observable<ApiResponse<boolean>> { return this.http.delete<ApiResponse<boolean>>(`${this.base}/${id}`); }
  getHistorial(id: number): Observable<ApiResponse<HistorialCambio[]>> { return this.http.get<ApiResponse<HistorialCambio[]>>(`${this.base}/${id}/historial`); }
}
