import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CreateUsuarioRequest, UpdateUsuarioRequest, Usuario } from '../models/usuarios/usuario.model';

@Injectable({ providedIn: 'root' })
export class UserAdminService {
  private readonly baseUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(rol?: string, activo?: boolean): Observable<ApiResponse<Usuario[]>> {
    let params = new HttpParams();
    if (rol) params = params.set('rol', rol);
    if (activo !== undefined) params = params.set('activo', activo);
    return this.http.get<ApiResponse<Usuario[]>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<Usuario>> {
    return this.http.get<ApiResponse<Usuario>>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateUsuarioRequest): Observable<ApiResponse<Usuario>> {
    return this.http.post<ApiResponse<Usuario>>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateUsuarioRequest): Observable<ApiResponse<Usuario>> {
    return this.http.put<ApiResponse<Usuario>>(`${this.baseUrl}/${id}`, dto);
  }

  toggleActivo(id: number, activo: boolean): Observable<ApiResponse<boolean>> {
    const params = new HttpParams().set('activo', activo);
    return this.http.patch<ApiResponse<boolean>>(`${this.baseUrl}/${id}/activo`, {}, { params });
  }

  resetPassword(id: number, nuevaPassword: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/${id}/reset-password`, { nuevaPassword });
  }

  me(): Observable<ApiResponse<Usuario>> {
    return this.http.get<ApiResponse<Usuario>>(`${this.baseUrl}/me`);
  }

  cambiarPasswordPropio(passwordActual: string, passwordNueva: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/me/cambiar-password`, { passwordActual, passwordNueva });
  }
}
