import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { CondicionFiscal } from '../models/common/condicion-fiscal.model';

@Injectable({ providedIn: 'root' })
export class CondicionFiscalService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/condicionesfiscales`;

  getAll(): Observable<ApiResponse<CondicionFiscal[]>> {
    return this.http.get<ApiResponse<CondicionFiscal[]>>(this.base);
  }
}
