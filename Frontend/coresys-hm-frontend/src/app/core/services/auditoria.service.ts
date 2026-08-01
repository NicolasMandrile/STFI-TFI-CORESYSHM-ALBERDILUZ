import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AuditoriaAcceso, AuditoriaFiltro, PagedResponse } from '../models/auditoria/auditoria.model';

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private readonly baseUrl = `${environment.apiUrl}/auditoria`;

  constructor(private http: HttpClient) {}

  buscar(filtro: AuditoriaFiltro): Observable<ApiResponse<PagedResponse<AuditoriaAcceso>>> {
    let params = new HttpParams();
    if (filtro.usuarioId) params = params.set('usuarioId', filtro.usuarioId);
    if (filtro.accion) params = params.set('accion', filtro.accion);
    if (filtro.fechaDesde) params = params.set('fechaDesde', filtro.fechaDesde);
    if (filtro.fechaHasta) params = params.set('fechaHasta', filtro.fechaHasta);
    params = params.set('pagina', filtro.pagina ?? 1);
    params = params.set('tamanoPagina', filtro.tamanoPagina ?? 20);
    return this.http.get<ApiResponse<PagedResponse<AuditoriaAcceso>>>(this.baseUrl, { params });
  }
}
