import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ProductoService } from '../../core/services/producto.service';
import { VentaService } from '../../core/services/venta.service';
import { MovimientoStockService } from '../../core/services/movimiento-stock.service';
import { CompraService } from '../../core/services/compra.service';
import { Producto } from '../../core/models/stock/producto.model';
import { MovimientoStock } from '../../core/models/stock/movimiento-stock.model';
import { Venta } from '../../core/models/ventas/venta.model';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  private readonly authService      = inject(AuthService);
  private readonly productoSvc      = inject(ProductoService);
  private readonly ventaSvc         = inject(VentaService);
  private readonly movimientoSvc    = inject(MovimientoStockService);
  private readonly compraSvc        = inject(CompraService);
  private readonly cdr              = inject(ChangeDetectorRef);

  currentUser$ = this.authService.currentUser$;
  today = new Date();
  cargando = true;

  totalProductos   = 0;
  stockBajoCount   = 0;
  ventasHoy        = 0;
  totalVentasHoy   = 0;
  totalComprasMes  = 0;
  productosStockBajo: Producto[] = [];
  ultimosMovimientos: MovimientoStock[] = [];
  ultimasVentas: Venta[] = [];

  ngOnInit(): void {
    forkJoin({
      productos:   this.productoSvc.getAll(),
      stockBajo:   this.productoSvc.getStockBajo(),
      ventas:      this.ventaSvc.getAll(),
      movimientos: this.movimientoSvc.getAll(),
      compras:     this.compraSvc.getAll(),
    }).subscribe({
      next: ({ productos, stockBajo, ventas, movimientos, compras }) => {
        this.totalProductos      = (productos.data ?? []).length;
        this.productosStockBajo  = (stockBajo.data ?? []).slice(0, 5);
        this.stockBajoCount      = (stockBajo.data ?? []).length;

        const hoy = new Date();
        const ventasConfHoy = (ventas.data ?? []).filter(v => {
          const f = new Date(v.fecha);
          return v.estado === 'Confirmada' &&
            f.getDate() === hoy.getDate() && f.getMonth() === hoy.getMonth() && f.getFullYear() === hoy.getFullYear();
        });
        this.ventasHoy      = ventasConfHoy.length;
        this.totalVentasHoy = ventasConfHoy.reduce((s, v) => s + v.total, 0);
        this.ultimasVentas  = (ventas.data ?? []).slice(0, 5);

        this.ultimosMovimientos = (movimientos.data ?? []).slice(0, 8);

        const primerDiaMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
        this.totalComprasMes = (compras.data ?? [])
          .filter(c => c.estado === 'Confirmada' && new Date(c.fecha) >= primerDiaMes)
          .reduce((s, c) => s + c.total, 0);

        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  tipoMovLabel(tipo: string): string {
    const map: Record<string, string> = {
      ENTRADA: 'Entrada', SALIDA: 'Salida', AJUSTE: 'Ajuste',
      PERDIDA: 'Pérdida', COMPRA: 'Compra', VENTA: 'Venta',
      ANULACION_VENTA: 'Anul. venta', ANULACION_COMPRA: 'Anul. compra'
    };
    return map[tipo] ?? tipo;
  }
}
