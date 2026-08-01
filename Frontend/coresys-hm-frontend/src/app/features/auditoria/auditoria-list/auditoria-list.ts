import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { AuditoriaService } from '../../../core/services/auditoria.service';
import { AuditoriaAcceso } from '../../../core/models/auditoria/auditoria.model';

@Component({
  selector: 'app-auditoria-list',
  standalone: false,
  templateUrl: './auditoria-list.html',
  styleUrl: './auditoria-list.scss'
})
export class AuditoriaListComponent implements OnInit {
  private readonly auditoriaService = inject(AuditoriaService);
  private readonly cdr = inject(ChangeDetectorRef);

  registros: AuditoriaAcceso[] = [];
  cargando = true;

  filtroUsuarioId: number | null = null;
  filtroAccion = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  pagina = 1;
  totalPaginas = 1;

  readonly acciones = ['Login', 'LoginFallido', 'Logout', 'ResetPassword'];

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    this.auditoriaService.buscar({
      usuarioId: this.filtroUsuarioId ?? undefined,
      accion: this.filtroAccion || undefined,
      fechaDesde: this.filtroFechaDesde || undefined,
      fechaHasta: this.filtroFechaHasta || undefined,
      pagina: this.pagina,
      tamanoPagina: 20
    }).subscribe({
      next: res => {
        this.registros = res.data?.items ?? [];
        this.totalPaginas = res.data?.totalPaginas ?? 1;
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  aplicarFiltros(): void {
    this.pagina = 1;
    this.cargar();
  }

  irAPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) return;
    this.pagina = pagina;
    this.cargar();
  }
}
