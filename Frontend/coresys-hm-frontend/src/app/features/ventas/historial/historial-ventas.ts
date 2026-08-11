import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { VentaService } from '../../../core/services/venta.service';
import { AuthService } from '../../../core/services/auth.service';
import { Venta } from '../../../core/models/ventas/venta.model';

@Component({
  selector: 'app-historial-ventas',
  standalone: false,
  templateUrl: './historial-ventas.html',
  styleUrl: './historial-ventas.scss'
})
export class HistorialVentasComponent implements OnInit {
  private readonly ventaSvc = inject(VentaService);
  private readonly authSvc  = inject(AuthService);
  private readonly cdr      = inject(ChangeDetectorRef);

  /** Solo Administrador/Administrativo pueden confirmar o anular ventas. */
  readonly puedeGestionar = ['Administrador', 'Administrativo'].includes(this.authSvc.currentUser?.rol ?? '');

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
    this.cargar();
  }

  private cargar(): void {
    this.cargando = true;
    this.ventaSvc.getAll().subscribe({
      next: r => { this.ventas = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  verDetalle(v: Venta): void { this.ventaDetalle = v; }

  confirmar(v: Venta): void {
    if (!confirm(`¿Confirmar la venta ${v.numeroVenta}?`)) return;
    this.ventaSvc.confirmar(v.id).subscribe({
      // Se recarga desde el servidor en vez de mutar "v" en memoria: mutar el objeto local
      // + detectChanges() no estaba reflejando el nuevo estado en la fila de forma confiable.
      next: () => { this.ventaDetalle = null; this.cargar(); },
      error: e => { alert(e.error?.mensaje ?? 'No se pudo confirmar la venta.'); this.cdr.detectChanges(); }
    });
  }

  anular(v: Venta): void {
    if (!confirm(`¿Anular la venta ${v.numeroVenta}? Se revertirá el stock.`)) return;
    this.ventaSvc.anular(v.id).subscribe({
      next: () => { this.ventaDetalle = null; this.cargar(); },
      error: e => { alert(e.error?.mensaje ?? 'No se pudo anular la venta.'); this.cdr.detectChanges(); }
    });
  }

  estadoClass(estado: string): string {
    const map: Record<string, string> = { Confirmada: 'tag-ok', Pendiente: 'tag-warn', Anulada: 'tag-danger' };
    return map[estado] ?? '';
  }
}
