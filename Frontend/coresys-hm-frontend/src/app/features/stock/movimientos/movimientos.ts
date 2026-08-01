import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { MovimientoStockService } from '../../../core/services/movimiento-stock.service';
import { MovimientoStock } from '../../../core/models/stock/movimiento-stock.model';

@Component({
  selector: 'app-movimientos',
  standalone: false,
  templateUrl: './movimientos.html',
  styleUrl: './movimientos.scss'
})
export class MovimientosComponent implements OnInit {
  private readonly svc = inject(MovimientoStockService);
  private readonly cdr = inject(ChangeDetectorRef);

  movimientos: MovimientoStock[] = [];
  cargando = false;
  busqueda = '';

  get movimientosFiltrados(): MovimientoStock[] {
    if (!this.busqueda) return this.movimientos;
    const q = this.busqueda.toLowerCase();
    return this.movimientos.filter(m =>
      m.productoNombre.toLowerCase().includes(q) ||
      m.productoCodigo.toLowerCase().includes(q) ||
      m.tipoMovimiento.toLowerCase().includes(q)
    );
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
