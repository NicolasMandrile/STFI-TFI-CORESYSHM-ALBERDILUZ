import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { MovimientoStockService } from '../../../core/services/movimiento-stock.service';
import { MovimientoStock, TIPOS_MOVIMIENTO } from '../../../core/models/stock/movimiento-stock.model';

@Component({
  selector: 'app-movimientos',
  standalone: false,
  templateUrl: './movimientos.html',
  styleUrl: './movimientos.scss'
})
export class MovimientosComponent implements OnInit {
  private readonly svc = inject(MovimientoStockService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly tiposMovimiento = TIPOS_MOVIMIENTO;

  movimientos: MovimientoStock[] = [];
  cargando = false;
  busqueda = '';
  tipoFiltro = '';
  fechaDesde = '';
  fechaHasta = '';

  get movimientosFiltrados(): MovimientoStock[] {
    return this.movimientos.filter(m => {
      if (this.busqueda) {
        const q = this.busqueda.toLowerCase();
        const coincide = m.productoNombre.toLowerCase().includes(q) ||
          m.productoCodigo.toLowerCase().includes(q) ||
          m.tipoMovimiento.toLowerCase().includes(q);
        if (!coincide) return false;
      }
      if (this.tipoFiltro && m.tipoMovimiento !== this.tipoFiltro) return false;

      const fecha = m.fechaCreacion.substring(0, 10); // 'yyyy-MM-dd', mismo formato que <input type="date">
      if (this.fechaDesde && fecha < this.fechaDesde) return false;
      if (this.fechaHasta && fecha > this.fechaHasta) return false;

      return true;
    });
  }

  limpiarFiltros(): void {
    this.busqueda = '';
    this.tipoFiltro = '';
    this.fechaDesde = '';
    this.fechaHasta = '';
  }

  ngOnInit(): void {
    this.svc.getAll().subscribe({
      next: r => { this.movimientos = r.data ?? []; this.cdr.detectChanges(); },
      error: () => this.cdr.detectChanges()
    });
  }

  tipoClass(tipo: string): string {
    const entradas = ['ENTRADA', 'AJUSTE', 'COMPRA', 'ANULACION_VENTA'];
    return entradas.includes(tipo) ? 'tag-entrada' : 'tag-salida';
  }

  tipoLabel(tipo: string): string {
    const map: Record<string, string> = {
      ENTRADA: 'Entrada', SALIDA: 'Salida', AJUSTE: 'Ajuste',
      PERDIDA: 'Pérdida', RECUENTO: 'Recuento', COMPRA: 'Compra',
      VENTA: 'Venta', ANULACION_VENTA: 'Anul. venta', ANULACION_COMPRA: 'Anul. compra'
    };
    return map[tipo] ?? tipo;
  }
}
