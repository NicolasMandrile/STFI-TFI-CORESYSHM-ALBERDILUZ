import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FacturaService } from '../../../core/services/factura.service';
import {
  VentaFacturable, SaldoFacturarLinea, TipoComprobante, PuntoVenta, CreateFactura
} from '../../../core/models/facturacion/factura.model';

interface LineaSeleccionable extends SaldoFacturarLinea {
  cantidadAFacturar: number;
  impuesto: number;
}

@Component({
  selector: 'app-nueva-factura',
  standalone: false,
  templateUrl: './nueva-factura.html',
  styleUrl: './nueva-factura.scss'
})
export class NuevaFacturaComponent implements OnInit {
  private readonly facturaSvc = inject(FacturaService);
  private readonly fb         = inject(FormBuilder);
  private readonly router     = inject(Router);
  private readonly cdr        = inject(ChangeDetectorRef);

  ventasFacturables: VentaFacturable[] = [];
  tiposComprobante:  TipoComprobante[] = [];
  puntosVenta:       PuntoVenta[]      = [];

  ventaSeleccionada: VentaFacturable | null = null;
  lineas: LineaSeleccionable[] = [];

  cargando  = false;
  emitiendo = false;
  error     = '';
  exito     = '';

  facturaForm!: FormGroup;

  /// Se genera una sola vez al elegir la venta y se reenvía sin cambios ante reintentos:
  /// evita que un doble clic o una caída de red dupliquen la factura.
  private idempotencyKey = '';

  get subtotal(): number {
    return this.lineas.reduce((s, l) => s + (l.precioUnitario * l.cantidadAFacturar), 0);
  }
  get iva(): number {
    return this.lineas.reduce((s, l) => s + Math.round(l.precioUnitario * l.cantidadAFacturar * (l.impuesto / 100) * 100) / 100, 0);
  }
  get total(): number { return this.subtotal + this.iva; }

  ngOnInit(): void {
    this.facturaForm = this.fb.group({
      tipoComprobanteId: ['', Validators.required],
      puntoVentaId:      ['', Validators.required],
      fechaVencimiento:  [''],
      observaciones:     ['']
    });
    this.cargar();
  }

  private cargar(): void {
    this.cargando = true;
    this.facturaSvc.getVentasFacturables().subscribe(r => {
      this.ventasFacturables = r.data ?? [];
      this.cargando = false;
      this.cdr.detectChanges();
    });
    this.facturaSvc.getTiposComprobante().subscribe(r => { this.tiposComprobante = r.data ?? []; this.cdr.detectChanges(); });
    this.facturaSvc.getPuntosVenta().subscribe(r => { this.puntosVenta = r.data ?? []; this.cdr.detectChanges(); });
  }

  seleccionarVenta(v: VentaFacturable): void {
    this.ventaSeleccionada = v;
    this.lineas = v.lineas
      .filter(l => l.cantidadPendiente > 0)
      .map(l => ({ ...l, cantidadAFacturar: l.cantidadPendiente, impuesto: 21 }));
    this.idempotencyKey = crypto.randomUUID();
    this.error = '';
    this.cdr.detectChanges();
  }

  cambiarCantidad(linea: LineaSeleccionable, valor: number): void {
    linea.cantidadAFacturar = Math.max(0, Math.min(valor, linea.cantidadPendiente));
    this.cdr.detectChanges();
  }

  emitir(): void {
    if (!this.ventaSeleccionada) { this.error = 'Elegí una venta para facturar.'; return; }
    if (this.facturaForm.invalid) { this.facturaForm.markAllAsTouched(); return; }

    const lineasAFacturar = this.lineas.filter(l => l.cantidadAFacturar > 0);
    if (lineasAFacturar.length === 0) { this.error = 'Cargá una cantidad mayor a cero en al menos una línea.'; return; }

    this.emitiendo = true; this.error = '';
    const dto: CreateFactura = {
      ventaId: this.ventaSeleccionada.ventaId,
      tipoComprobanteId: +this.facturaForm.value.tipoComprobanteId,
      puntoVentaId: +this.facturaForm.value.puntoVentaId,
      fechaVencimiento: this.facturaForm.value.fechaVencimiento || undefined,
      observaciones: this.facturaForm.value.observaciones || undefined,
      idempotencyKey: this.idempotencyKey,
      detalles: lineasAFacturar.map(l => ({
        detalleVentaId: l.detalleVentaId,
        cantidad: l.cantidadAFacturar,
        impuesto: l.impuesto,
        descuento: 0
      }))
    };

    this.facturaSvc.emitir(dto).subscribe({
      next: r => {
        this.emitiendo = false;
        this.exito = `Factura ${r.data?.numeroFactura} emitida correctamente.`;
        this.cdr.detectChanges();
        setTimeout(() => this.router.navigate(['/facturacion']), 1500);
      },
      error: e => {
        this.emitiendo = false;
        this.error = e.error?.mensaje ?? 'Error al emitir la factura.';
        this.cdr.detectChanges();
      }
    });
  }
}
