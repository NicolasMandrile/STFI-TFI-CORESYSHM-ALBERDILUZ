import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { VentaService } from '../../../core/services/venta.service';
import { Venta } from '../../../core/models/ventas/venta.model';

@Component({
  selector: 'app-historial-ventas',
  standalone: false,
  templateUrl: './historial-ventas.html',
  styleUrl: './historial-ventas.scss'
})
export class HistorialVentasComponent implements OnInit {
  private readonly ventaSvc = inject(VentaService);
  private readonly cdr      = inject(ChangeDetectorRef);

  ventas:   Venta[] = [];
  cargando  = false;
  busqueda  = '';
  ventaDetalle: Venta | null = null;

  get ventasFiltradas(): Venta[] {
    if (!this.busqueda) return this.ventas;
    const q = this.busqueda.toLowerCase();
    return this.ventas.filter(v =>
      v.numeroVenta.toLowerCase().includes(q) ||
      v.clienteNombre.toLowerCase().includes(q)
    );
  }

  ngOnInit(): void {
    this.ventaSvc.getAll().subscribe({
      next: r => { this.ventas = r.data ?? []; this.cdr.detectChanges(); },
      error: () => this.cdr.detectChanges()
    });
  }

  verDetalle(v: Venta): void { this.ventaDetalle = v; }

  anular(v: Venta): void {
    if (!confirm(`¿Anular la venta ${v.numeroVenta}? Se revertirá el stock.`)) return;
    this.ventaSvc.anular(v.id).subscribe(() => { v.estado = 'Anulada'; this.ventaDetalle = null; this.cdr.detectChanges(); });
  }

  estadoClass(estado: string): string {
    const map: Record<string, string> = { Confirmada: 'tag-ok', Pendiente: 'tag-warn', Anulada: 'tag-danger' };
    return map[estado] ?? '';
  }
}
