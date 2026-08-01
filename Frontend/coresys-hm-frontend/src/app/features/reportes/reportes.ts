import {
  Component, OnInit, OnDestroy, AfterViewInit,
  inject, ChangeDetectorRef, ViewChild, ElementRef
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { ReporteService } from '../../core/services/reporte.service';
import {
  ProductoMasVendido, VentasPorPeriodo,
  RankingCliente, TicketPromedio, MargenProducto
} from '../../core/models/reportes/reporte.model';

Chart.register(...registerables);

type Tab = 'productos' | 'periodo' | 'clientes' | 'ticket' | 'margen';

@Component({
  selector: 'app-reportes',
  standalone: false,
  templateUrl: './reportes.html',
  styleUrl:    './reportes.scss'
})
export class ReportesComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly svc = inject(ReporteService);
  private readonly fb  = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  tabActiva: Tab = 'productos';
  cargando = false;
  error    = '';

  // datos
  productosVendidos:  ProductoMasVendido[]  = [];
  ventasPeriodo:      VentasPorPeriodo[]    = [];
  rankingClientes:    RankingCliente[]      = [];
  ticketPromedio:     TicketPromedio | null = null;
  margenProductos:    MargenProducto[]      = [];

  form!: FormGroup;

  readonly tabs: { id: Tab; label: string }[] = [
    { id: 'productos', label: 'Productos más vendidos' },
    { id: 'periodo',   label: 'Ventas por período'     },
    { id: 'clientes',  label: 'Ranking de clientes'    },
    { id: 'ticket',    label: 'Ticket promedio'        },
    { id: 'margen',    label: 'Margen por producto'    },
  ];

  readonly granularidades = [
    { value: 'dia',   label: 'Día'   },
    { value: 'semana',label: 'Semana'},
    { value: 'mes',   label: 'Mes'   },
    { value: 'año',   label: 'Año'   },
  ];

  ngOnInit(): void {
    const hoy   = new Date();
    const inicio = new Date(hoy.getFullYear(), 0, 1);
    this.form = this.fb.group({
      desde:       [this.toISO(inicio), Validators.required],
      hasta:       [this.toISO(hoy),    Validators.required],
      topN:        [10, [Validators.required, Validators.min(1), Validators.max(50)]],
      ordenarPor:  ['cantidad'],
      granularidad:['mes']
    });
    this.cargar();
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void { this.chart?.destroy(); }

  cambiarTab(tab: Tab): void {
    this.tabActiva = tab;
    this.error = '';
    this.chart?.destroy();
    this.chart = null;
    this.cdr.detectChanges();
    if (tab === 'periodo' && this.ventasPeriodo.length) {
      setTimeout(() => this.renderChart(), 50);
    }
  }

  cargar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.cargando = true;
    this.error = '';
    const f = this.filtros();

    switch (this.tabActiva) {
      case 'productos': this.cargarProductos(f); break;
      case 'periodo':   this.cargarPeriodo(f);   break;
      case 'clientes':  this.cargarClientes(f);  break;
      case 'ticket':    this.cargarTicket(f);     break;
      case 'margen':    this.cargarMargen(f);     break;
    }
  }

  private cargarProductos(f: any): void {
    this.svc.productosMasVendidos(f).subscribe({
      next: r  => { this.productosVendidos = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarPeriodo(f: any): void {
    this.svc.ventasPorPeriodo(f).subscribe({
      next: r => {
        this.ventasPeriodo = r.data ?? [];
        this.fin();
        setTimeout(() => this.renderChart(), 50);
      },
      error: () => this.finError()
    });
  }

  private cargarClientes(f: any): void {
    this.svc.rankingClientes(f).subscribe({
      next: r  => { this.rankingClientes = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarTicket(f: any): void {
    this.svc.ticketPromedio(f).subscribe({
      next: r  => { this.ticketPromedio = r.data ?? null; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarMargen(f: any): void {
    this.svc.margenProductos(f).subscribe({
      next: r  => { this.margenProductos = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private fin(): void { this.cargando = false; this.cdr.detectChanges(); }
  private finError(): void {
    this.cargando = false;
    this.error = 'Error al cargar el reporte. Verificá los filtros e intentá de nuevo.';
    this.cdr.detectChanges();
  }

  private filtros() {
    const v = this.form.value;
    return { desde: v.desde, hasta: v.hasta, topN: v.topN,
             ordenarPor: v.ordenarPor, granularidad: v.granularidad };
  }

  private toISO(d: Date): string {
    return d.toISOString().substring(0, 10);
  }

  // ── Chart ─────────────────────────────────────────────────────────────────
  private renderChart(): void {
    this.chart?.destroy();
    const canvas = this.chartCanvas?.nativeElement;
    if (!canvas || !this.ventasPeriodo.length) return;

    this.chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: this.ventasPeriodo.map(v => v.periodo),
        datasets: [
          {
            label: 'Total facturado ($)',
            data: this.ventasPeriodo.map(v => v.totalFacturado),
            backgroundColor: 'rgba(79,70,229,0.7)',
            borderColor: '#4f46e5',
            borderWidth: 1,
            yAxisID: 'y'
          },
          {
            label: 'Cant. ventas',
            data: this.ventasPeriodo.map(v => v.cantidadVentas),
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
          y:  { type: 'linear', display: true, position: 'left',
                ticks: { callback: (v) => `$${Number(v).toLocaleString('es-AR')}` } },
          y1: { type: 'linear', display: true, position: 'right',
                grid: { drawOnChartArea: false } }
        }
      }
    });
  }

  // ── CSV Export ────────────────────────────────────────────────────────────
  exportarCSV(): void {
    let csv = '';
    let nombre = 'reporte';

    switch (this.tabActiva) {
      case 'productos':
        csv = 'Código,Nombre,Cantidad vendida,Total facturado\n'
          + this.productosVendidos.map(r =>
              `"${r.codigo}","${r.nombre}",${r.cantidadVendida},${r.totalFacturado}`).join('\n');
        nombre = 'productos-mas-vendidos';
        break;
      case 'periodo':
        csv = 'Período,Cantidad ventas,Total facturado\n'
          + this.ventasPeriodo.map(r =>
              `"${r.periodo}",${r.cantidadVentas},${r.totalFacturado}`).join('\n');
        nombre = 'ventas-por-periodo';
        break;
      case 'clientes':
        csv = 'Apellido,Nombre,Cant. compras,Monto total,Ticket promedio\n'
          + this.rankingClientes.map(r =>
              `"${r.apellido}","${r.nombre}",${r.cantidadCompras},${r.montoTotal},${r.ticketPromedio}`).join('\n');
        nombre = 'ranking-clientes';
        break;
      case 'ticket':
        if (!this.ticketPromedio) return;
        csv = 'Cant. ventas,Total facturado,Ticket promedio\n'
          + `${this.ticketPromedio.cantidadVentas},${this.ticketPromedio.totalFacturado},${this.ticketPromedio.ticketPromedio}`;
        nombre = 'ticket-promedio';
        break;
      case 'margen':
        csv = 'Código,Nombre,Cant. vendida,Ingreso total,Costo total,Margen total,Margen %\n'
          + this.margenProductos.map(r =>
              `"${r.codigo}","${r.nombre}",${r.cantidadVendida},${r.ingresoTotal},${r.costoTotal},${r.margenTotal},${r.margenPorcentual}`).join('\n');
        nombre = 'margen-productos';
        break;
    }

    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url; a.download = `${nombre}.csv`; a.click();
    URL.revokeObjectURL(url);
  }
}
