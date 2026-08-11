import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FacturaService } from '../../../core/services/factura.service';
import { AuthService } from '../../../core/services/auth.service';
import { Factura } from '../../../core/models/facturacion/factura.model';

@Component({
  selector: 'app-historial-facturas',
  standalone: false,
  templateUrl: './historial-facturas.html',
  styleUrl: './historial-facturas.scss'
})
export class HistorialFacturasComponent implements OnInit {
  private readonly facturaSvc = inject(FacturaService);
  private readonly authSvc    = inject(AuthService);
  private readonly cdr        = inject(ChangeDetectorRef);

  readonly puedeGestionar = ['Administrador', 'Administrativo'].includes(this.authSvc.currentUser?.rol ?? '');

  facturas: Factura[] = [];
  cargando = false;
  busqueda = '';
  facturaDetalle: Factura | null = null;

  get facturasFiltradas(): Factura[] {
    if (!this.busqueda) return this.facturas;
    const q = this.busqueda.toLowerCase();
    return this.facturas.filter(f =>
      f.numeroFactura.toLowerCase().includes(q) ||
      f.clienteNombre.toLowerCase().includes(q) ||
      f.numeroVenta.toLowerCase().includes(q)
    );
  }

  ngOnInit(): void {
    this.cargar();
  }

  private cargar(): void {
    this.cargando = true;
    this.facturaSvc.getAll().subscribe({
      next: r => { this.facturas = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  verDetalle(f: Factura): void { this.facturaDetalle = f; }

  pagar(f: Factura): void {
    if (!confirm(`¿Marcar la factura ${f.numeroFactura} como pagada?`)) return;
    this.facturaSvc.pagar(f.id).subscribe({
      next: () => { this.facturaDetalle = null; this.cargar(); },
      error: e => { alert(e.error?.mensaje ?? 'No se pudo marcar como pagada.'); this.cdr.detectChanges(); }
    });
  }

  anular(f: Factura): void {
    if (!confirm(`¿Anular la factura ${f.numeroFactura}? Se revertirá el stock si corresponde.`)) return;
    this.facturaSvc.anular(f.id).subscribe({
      next: () => { this.facturaDetalle = null; this.cargar(); },
      error: e => { alert(e.error?.mensaje ?? 'No se pudo anular la factura.'); this.cdr.detectChanges(); }
    });
  }

  estadoClass(estado: string): string {
    const map: Record<string, string> = { Emitida: 'tag-warn', Pagada: 'tag-ok', Vencida: 'tag-danger', Anulada: 'tag-danger' };
    return map[estado] ?? '';
  }
}
