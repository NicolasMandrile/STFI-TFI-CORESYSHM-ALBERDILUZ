import {
  Component, OnInit, OnDestroy, AfterViewInit,
  inject, ChangeDetectorRef, ViewChild, ElementRef
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { ReporteService } from '../../core/services/reporte.service';
import { ProductoService } from '../../core/services/producto.service';
import { ProveedorService } from '../../core/services/proveedor.service';
import {
  ComprasPorPeriodo, EvolucionPrecioCompra, FiltrosReporteCompras,
  ProductoMasComprado, RankingProveedor, SugerenciaReposicion
} from '../../core/models/reportes/reporte.model';
import { Producto } from '../../core/models/stock/producto.model';
import { Proveedor } from '../../core/models/stock/proveedor.model';

Chart.register(...registerables);

type Tab = 'periodo' | 'proveedores' | 'productos' | 'evolucion' | 'reposicion';

@Component({
  selector: 'app-reportes-compras',
  standalone: false,
  templateUrl: './reportes-compras.html',
  styleUrl:    './reportes-compras.scss'
})
export class ReportesComprasComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly svc       = inject(ReporteService);
  private readonly prodSvc   = inject(ProductoService);
  private readonly provSvc   = inject(ProveedorService);
  private readonly fb        = inject(FormBuilder);
  private readonly cdr       = inject(ChangeDetectorRef);

  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;

  tabActiva: Tab = 'periodo';
  cargando  = false;
  error     = '';

  // datos
  comprasPeriodo:    ComprasPorPeriodo[]    = [];
  rankingProveedores:RankingProveedor[]     = [];
  productosComprados:ProductoMasComprado[]  = [];
  evolucionPrecio:   EvolucionPrecioCompra[]= [];
  sugerencias:       SugerenciaReposicion[] = [];

  // catálogos para selectores
  productos:  Producto[]  = [];
  proveedores:Proveedor[] = [];

  form!: FormGroup;

  readonly tabs: { id: Tab; label: string }[] = [
    { id: 'periodo',    label: 'Total por período'         },
    { id: 'proveedores',label: 'Ranking proveedores'       },
    { id: 'productos',  label: 'Productos más comprados'   },
    { id: 'evolucion',  label: 'Evolución del precio'      },
    { id: 'reposicion', label: 'Sugerencia de reposición'  },
  ];

  readonly granularidades = [
    { value: 'dia',   label: 'Día'    },
    { value: 'semana',label: 'Semana' },
    { value: 'mes',   label: 'Mes'    },
    { value: 'año',   label: 'Año'    },
  ];

  ngOnInit(): void {
    const hoy   = new Date();
    const inicio = new Date(hoy.getFullYear(), 0, 1);
    this.form = this.fb.group({
      desde:        [this.toISO(inicio), Validators.required],
      hasta:        [this.toISO(hoy),    Validators.required],
      topN:         [10, [Validators.required, Validators.min(1), Validators.max(50)]],
      ordenarPor:   ['cantidad'],
      granularidad: ['mes'],
      productoId:   [null],
      proveedorId:  [null]
    });

    // Cargar catálogos para los selectores
    this.prodSvc.getAll().subscribe({ next: r => this.productos  = r.data ?? [] });
    this.provSvc.getAll().subscribe({ next: r => this.proveedores = r.data ?? [] });

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
    if ((tab === 'periodo' && this.comprasPeriodo.length) ||
        (tab === 'evolucion' && this.evolucionPrecio.length)) {
      setTimeout(() => this.renderChart(), 50);
    }
  }

  cargar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.cargando = true;
    this.error = '';
    const f = this.filtros();

    switch (this.tabActiva) {
      case 'periodo':     this.cargarPeriodo(f);     break;
      case 'proveedores': this.cargarProveedores(f); break;
      case 'productos':   this.cargarProductos(f);   break;
      case 'evolucion':   this.cargarEvolucion(f);   break;
      case 'reposicion':  this.cargarReposicion(f);  break;
    }
  }

  private cargarPeriodo(f: FiltrosReporteCompras): void {
    this.svc.comprasPorPeriodo(f).subscribe({
      next: r => {
        this.comprasPeriodo = r.data ?? [];
        this.fin();
        setTimeout(() => this.renderChart(), 50);
      },
      error: () => this.finError()
    });
  }

  private cargarProveedores(f: FiltrosReporteCompras): void {
    this.svc.rankingProveedores(f).subscribe({
      next: r  => { this.rankingProveedores = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarProductos(f: FiltrosReporteCompras): void {
    this.svc.productosMasComprados(f).subscribe({
      next: r  => { this.productosComprados = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private cargarEvolucion(f: FiltrosReporteCompras): void {
    if (!f.productoId) {
      this.cargando = false;
      this.error = 'Seleccioná un producto para ver la evolución del precio.';
      return;
    }
    this.svc.evolucionPrecioCompra(f).subscribe({
      next: r => {
        this.evolucionPrecio = r.data ?? [];
        this.fin();
        setTimeout(() => this.renderChart(), 50);
      },
      error: () => this.finError()
    });
  }

  private cargarReposicion(f: FiltrosReporteCompras): void {
    this.svc.sugerenciasReposicion(f).subscribe({
      next: r  => { this.sugerencias = r.data ?? []; this.fin(); },
      error: () => this.finError()
    });
  }

  private fin(): void { this.cargando = false; this.cdr.detectChanges(); }
  private finError(): void {
    this.cargando = false;
    this.error = 'Error al cargar el reporte. Verificá los filtros e intentá de nuevo.';
    this.cdr.detectChanges();
  }

  private filtros(): FiltrosReporteCompras {
    const v = this.form.value;
    return {
      desde:       v.desde,
      hasta:       v.hasta,
      topN:        v.topN,
      ordenarPor:  v.ordenarPor,
      granularidad:v.granularidad,
      productoId:  v.productoId  ? +v.productoId  : undefined,
      proveedorId: v.proveedorId ? +v.proveedorId : undefined
    };
  }

  private toISO(d: Date): string { return d.toISOString().substring(0, 10); }

  // ── Charts ─────────────────────────────────────────────────────────────────
  private renderChart(): void {
    this.chart?.destroy();
    const canvas = this.chartCanvas?.nativeElement;
    if (!canvas) return;

    if (this.tabActiva === 'periodo' && this.comprasPeriodo.length) {
      this.chart = new Chart(canvas, {
        type: 'bar',
        data: {
          labels: this.comprasPeriodo.map(c => c.periodo),
          datasets: [
            {
              label: 'Total gastado ($)',
              data: this.comprasPeriodo.map(c => c.totalGastado),
              backgroundColor: 'rgba(234,88,12,0.7)',
              borderColor: '#ea580c',
              borderWidth: 1,
              yAxisID: 'y'
            },
            {
              label: 'Cant. compras',
              data: this.comprasPeriodo.map(c => c.cantidadCompras),
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

    if (this.tabActiva === 'evolucion' && this.evolucionPrecio.length) {
      this.chart = new Chart(canvas, {
        type: 'line',
        data: {
          labels: this.evolucionPrecio.map(e => e.fecha),
          datasets: [{
            label: 'Precio unitario ($)',
            data: this.evolucionPrecio.map(e => e.precioUnitario),
            backgroundColor: 'rgba(79,70,229,0.1)',
            borderColor: '#4f46e5',
            borderWidth: 2,
            pointBackgroundColor: '#4f46e5',
            pointRadius: 5,
            tension: 0.2,
            fill: true
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { position: 'top' },
                     tooltip: { callbacks: { label: ctx => `$${(ctx.parsed.y ?? 0).toLocaleString('es-AR')}` } } },
          scales: {
            y: { ticks: { callback: (v) => `$${Number(v).toLocaleString('es-AR')}` } }
          }
        }
      });
    }
  }

  // ── CSV Export ────────────────────────────────────────────────────────────
  exportarCSV(): void {
    let csv = '';
    let nombre = 'reporte-compras';

    switch (this.tabActiva) {
      case 'periodo':
        csv = 'Período,Cant. compras,Total gastado\n'
          + this.comprasPeriodo.map(r =>
              `"${r.periodo}",${r.cantidadCompras},${r.totalGastado}`).join('\n');
        nombre = 'compras-por-periodo';
        break;
      case 'proveedores':
        csv = 'Razón social,CUIT,Cant. compras,Monto total,Ticket promedio\n'
          + this.rankingProveedores.map(r =>
              `"${r.razonSocial}","${r.cuit}",${r.cantidadCompras},${r.montoTotal},${r.ticketPromedio}`).join('\n');
        nombre = 'ranking-proveedores';
        break;
      case 'productos':
        csv = 'Código,Producto,Cant. comprada,Monto total\n'
          + this.productosComprados.map(r =>
              `"${r.codigo}","${r.nombre}",${r.cantidadComprada},${r.montoTotal}`).join('\n');
        nombre = 'productos-mas-comprados';
        break;
      case 'evolucion':
        csv = 'Fecha,Precio unitario,N° Compra\n'
          + this.evolucionPrecio.map(r =>
              `"${r.fecha}",${r.precioUnitario},"${r.numeroCompra}"`).join('\n');
        nombre = 'evolucion-precio-compra';
        break;
      case 'reposicion':
        csv = 'Código,Producto,Stock actual,Stock mínimo,Diferencia,Proveedor,Último precio\n'
          + this.sugerencias.map(r =>
              `"${r.codigo}","${r.nombre}",${r.stockActual},${r.stockMinimo},${r.diferencia},"${r.proveedorNombre}",${r.ultimoPrecioCompra}`).join('\n');
        nombre = 'sugerencias-reposicion';
        break;
    }

    if (!csv) return;
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url; a.download = `${nombre}.csv`; a.click();
    URL.revokeObjectURL(url);
  }

  // ── Totales helpers ────────────────────────────────────────────────────────
  sumCompras(field: 'totalGastado' | 'cantidadCompras'): number {
    return this.comprasPeriodo.reduce((acc, r) => acc + r[field], 0);
  }
}
