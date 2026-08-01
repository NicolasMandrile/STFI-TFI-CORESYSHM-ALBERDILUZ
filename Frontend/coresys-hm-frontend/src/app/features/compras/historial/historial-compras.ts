import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CompraService } from '../../../core/services/compra.service';
import { Compra } from '../../../core/models/compras/compra.model';

@Component({
  selector: 'app-historial-compras',
  standalone: false,
  templateUrl: './historial-compras.html',
  styleUrl: './historial-compras.scss'
})
export class HistorialComprasComponent implements OnInit {
  private readonly svc = inject(CompraService);
  private readonly cdr = inject(ChangeDetectorRef);

  compras:  Compra[] = [];
  cargando  = false;
  busqueda  = '';
  detalle: Compra | null = null;

  get comprasFiltradas(): Compra[] {
    if (!this.busqueda) return this.compras;
    const q = this.busqueda.toLowerCase();
    return this.compras.filter(c =>
      c.numeroCompra.toLowerCase().includes(q) || c.proveedorNombre.toLowerCase().includes(q)
    );
  }

  ngOnInit(): void {
    this.svc.getAll().subscribe({
      next: r => { this.compras = r.data ?? []; this.cdr.detectChanges(); },
      error: () => this.cdr.detectChanges()
    });
  }

  anular(c: Compra): void {
    if (!confirm(`¿Anular la compra ${c.numeroCompra}? Se revertirá el stock.`)) return;
    this.svc.anular(c.id).subscribe(() => { c.estado = 'Anulada'; this.detalle = null; this.cdr.detectChanges(); });
  }
}
