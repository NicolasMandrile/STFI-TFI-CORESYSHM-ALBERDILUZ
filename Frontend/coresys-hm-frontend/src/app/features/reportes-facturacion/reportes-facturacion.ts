import {
  Component, OnInit, OnDestroy, inject, ChangeDetectorRef, ViewChild, ElementRef
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { ReporteService } from '../../core/services/reporte.service';
import { FacturaService } from '../../core/services/factura.service';
import { ProductoService } from '../../core/services/producto.service';
import { ClienteService } from '../../core/services/cliente.service';
import {
  DesempenoCliente, DesempenoProducto, FacturacionPorPeriodo, FiltrosReporteFacturacion
} from '../../core/models/reportes/reporte.model';
import { VentaFacturable, TipoComprobante, PuntoVenta } from '../../core/models/facturacion/factura.model';
import { Producto } from '../../core/models/stock/producto.model';
import { Cliente } from '../../core/models/ventas/cliente.model';

Chart.register(...registerables);

type Tab = 'periodo' | 'clientes' | 'productos' | 'cartera';

@Component({
  selector: 'app-reportes-facturacion',
  standalone: false,
  templateUrl: './reportes-facturacion.html',
  styleUrl: './reportes-facturacion.scss'
})
export class ReportesFacturacionComponent implements OnInit, OnDestroy {
  private readonly svc        = inject(ReporteService);
  private readonly facturaSvc = inject(FacturaService);
  private readonly prodSvc    = inject(ProductoService);
  private readonly clienteSvc = inject(ClienteService);
  private readonly fb         = inject(FormBuilder);
  private readonly cdr        = inject(ChangeDetectorRef);

  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  tabActiva: Tab = 'periodo';
  cargando = false;
  error    = '';

  porPeriodo:      FacturacionPorPeriodo[] = [];
  desempenoClientes: DesempenoCliente[]    = [];
  desempenoProductos: DesempenoProducto[]  = [];
  cartera:          VentaFacturable[]      = [];

  productos: Producto[] = [];
  clientes:  Cliente[]  = [];
  tiposComprobante: TipoComprobante[] = [];
  puntosVenta: PuntoVenta[] = [];

  form!: FormGroup;

  readonly tabs: { id: Tab; label: string }[] = [
    { id: 'periodo',   label: 'Facturación por período' },
    { id: 'clientes',  label: 'Desempeño por cliente' },
    { id: 'productos', label: 'Desempeño por producto' },
    { id: 'cartera',   label: 'Cartera por facturar' },
  ];

  readonly granularidades = [
    { value: 'dia',    label: 'Día' },
    { value: 'semana', label: 'Semana' },
    { value: 'mes',    label: 'Mes' },
    { value: 'año',    label: 'Año' },
  ];

  ngOnInit(): void {
    const hoy = new Date();
    const inicio = new Date(hoy.getFullYear(), 0, 1);
    this.form = this.fb.group({
      desde: [this.toISO(inicio), Validators.required],
      hasta: [this.toISO(hoy), Validators.required],
      topN: [10, [Validators.required, Validators.min(1), Validators.max(50)]],
      granularidad: ['mes'],
      puntoVentaId: [null],
      tipoComprobanteId: [null],
      clienteId: [null],
      productoId: [null],
    });

    this.prodSvc.getAll().subscribe(r => { this.productos = r.data ?? []; this.cdr.detectChanges(); });
    this.clienteSvc.getAll().subscribe(r => { this.clientes = r.data ?? []; this.cdr.detectChanges(); });
    this.facturaSvc.getTiposComprobante().subscribe(r => { this.tiposComprobante = r.data ?? []; this.cdr.detectChanges(); });
    this.facturaSvc.getPuntosVenta().subscribe(r => { this.puntosVenta = r.data ?? []; this.cdr.detectChanges(); });

    this.cargar();
  }

  ngOnDestroy(): void { this.chart?.destroy(); }

  cambiarTab(tab: Tab): void {
    this.tabActiva = tab;
    this.error = '';
    this.chart?.destroy();
    this.chart = null;
    this.cdr.detectChanges();
    if (tab === 'periodo' && this.porPeriodo.length) {
      setTimeout(() => this.renderChart(), 50);
    }
  }

  cargar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.cargando = true;
    this.error = '';
    const f = this.filtros();

    switch (this.tabActiva) {
      case 'periodo':   this.cargarPeriodo(f); break;
      case 'clientes':  this.cargarClientes(f); break;
      case 'productos': this.cargarProductos(f); break;
      case 'cartera':   this.cargarCartera(f); break;
    }
  }

  private cargarPeriodo(f: FiltrosReporteFacturacion): void {
    this.svc.facturacionPorPeriodo(f).subscribe({
      next: r => { this.porPeriodo = r.data ?? []; this.fin(); setTimeout(() => this.renderChart(), 50); },
      error: () => this.finError()
    });
  }

  private cargarClientes(f: FiltrosReporteFacturacion): void {
    this.svc.desempenoClientes(f).subscribe({
      next: r => { this.desempenoClientes = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarProductos(f: FiltrosReporteFacturacion): void {
    this.svc.desempenoProductos(f).subscribe({
      next: r => { this.desempenoProductos = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarCartera(f: FiltrosReporteFacturacion): void {
    this.svc.carteraPorFacturar(f).subscribe({
      next: r => { this.cartera = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private fin(): void { this.cargando = false; this.cdr.detectChanges(); }
  private finError(): void {
    this.cargando = false;
    this.error = 'Error al cargar el reporte. Verificá los filtros e intentá de nuevo.';
    this.cdr.detectChanges();
  }

  private filtros(): FiltrosReporteFacturacion {
    const v = this.form.value;
    return {
      desde: v.desde, hasta: v.hasta, topN: v.topN, granularidad: v.granularidad,
      puntoVentaId: v.puntoVentaId ? +v.puntoVentaId : undefined,
      tipoComprobanteId: v.tipoComprobanteId ? +v.tipoComprobanteId : undefined,
      clienteId: v.clienteId ? +v.clienteId : undefined,
      productoId: v.productoId ? +v.productoId : undefined,
    };
  }

  private toISO(d: Date): string { return d.toISOString().substring(0, 10); }

  // ── KPIs (suma / conteo / promedio) ───────────────────────────────────────
  get kpiPeriodo() {
    const cant = this.porPeriodo.reduce((s, r) => s + r.cantidadComprobantes, 0);
    const suma = this.porPeriodo.reduce((s, r) => s + r.total, 0);
    return { cantidad: cant, suma, promedio: cant ? suma / cant : 0 };
  }
  get kpiClientes() {
    const cant = this.desempenoClientes.reduce((s, r) => s + r.cantidadComprobantes, 0);
    const suma = this.desempenoClientes.reduce((s, r) => s + r.montoTotal, 0);
    return { cantidad: cant, suma, promedio: cant ? suma / cant : 0 };
  }
  get kpiProductos() {
    const cant = this.desempenoProductos.reduce((s, r) => s + r.cantidadFacturada, 0);
    const suma = this.desempenoProductos.reduce((s, r) => s + r.montoTotal, 0);
    return { cantidad: cant, suma, promedio: cant ? suma / cant : 0 };
  }
  get kpiCartera() {
    const lineas = this.cartera.flatMap(v => v.lineas);
    const suma = lineas.reduce((s, l) => s + l.precioUnitario * l.cantidadPendiente, 0);
    return { cantidad: this.cartera.length, suma, promedio: this.cartera.length ? suma / this.cartera.length : 0 };
  }

  // ── Chart ──────────────────────────────────────────────────────────────────
  private renderChart(): void {
    this.chart?.destroy();
    const canvas = this.chartCanvas?.nativeElement;
    if (!canvas || this.tabActiva !== 'periodo' || !this.porPeriodo.length) return;

    this.chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.porPeriodo.map(p => p.periodo),
        datasets: [
          {
            label: 'Total facturado ($)',
            data: this.porPeriodo.map(p => p.total),
            backgroundColor: 'rgba(79,70,229,0.7)',
            borderColor: '#4f46e5',
            borderWidth: 1,
            yAxisID: 'y'
          },
          {
            label: 'Cant. comprobantes',
            data: this.porPeriodo.map(p => p.cantidadComprobantes),
            type: 'line',
            backgroundColor: 'rgba(16,185,129,0.2)',
            borderColor: '#10b981',
            borderWidth: 2,
            pointBackgroundColor: '#10b981',
            tension: 0.3,
            yAxisID: 'y1'
          }
        ]
      },
      options: {
        responsive: true,
        interaction: { mode: 'index', intersect: false },
        plugins: { legend: { position: 'top' } },
        scales: {
          y: { type: 'linear', display: true, position: 'left',
               ticks: { callback: (v) => `$${Number(v).toLocaleString('es-AR')}` } },
          y1: { type: 'linear', display: true, position: 'right', grid: { drawOnChartArea: false } }
        }
      }
    });
  }

  // ── CSV export ─────────────────────────────────────────────────────────────
  exportarCSV(): void {
    let csv = '';
    let nombre = 'reporte-facturacion';

    switch (this.tabActiva) {
      case 'periodo':
        csv = 'Período,Cant. comprobantes,Neto,IVA,Total\n'
          + this.porPeriodo.map(r => `"${r.periodo}",${r.cantidadComprobantes},${r.neto},${r.iva},${r.total}`).join('\n');
        nombre = 'facturacion-por-periodo';
        break;
      case 'clientes':
        csv = 'Cliente,Cant. comprobantes,Monto total,Ticket promedio\n'
          + this.desempenoClientes.map(r => `"${r.clienteNombre}",${r.cantidadComprobantes},${r.montoTotal},${r.ticketPromedio}`).join('\n');
        nombre = 'desempeno-por-cliente';
        break;
      case 'productos':
        csv = 'Código,Producto,Cant. facturada,Monto total\n'
          + this.desempenoProductos.map(r => `"${r.codigo}","${r.nombre}",${r.cantidadFacturada},${r.montoTotal}`).join('\n');
        nombre = 'desempeno-por-producto';
        break;
      case 'cartera':
        csv = 'Venta,Cliente,Producto,Cant. pendiente,Precio unitario,Importe pendiente\n'
          + this.cartera.flatMap(v => v.lineas.map(l =>
              `"${v.numeroVenta}","${v.clienteNombre}","${l.productoNombre}",${l.cantidadPendiente},${l.precioUnitario},${(l.precioUnitario * l.cantidadPendiente).toFixed(2)}`
            )).join('\n');
        nombre = 'cartera-por-facturar';
        break;
    }

    if (!csv) return;
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `${nombre}.csv`; a.click();
    URL.revokeObjectURL(url);
  }
}
