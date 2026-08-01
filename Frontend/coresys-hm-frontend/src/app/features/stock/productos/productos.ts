import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ProductoService } from '../../../core/services/producto.service';
import { CategoriaService } from '../../../core/services/categoria.service';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { MovimientoStockService } from '../../../core/services/movimiento-stock.service';
import { Producto } from '../../../core/models/stock/producto.model';
import { Categoria } from '../../../core/models/stock/categoria.model';
import { Proveedor } from '../../../core/models/stock/proveedor.model';
import { TIPOS_MOVIMIENTO } from '../../../core/models/stock/movimiento-stock.model';

@Component({
  selector: 'app-productos',
  standalone: false,
  templateUrl: './productos.html',
  styleUrl: './productos.scss'
})
export class ProductosComponent implements OnInit {
  private readonly productoSvc   = inject(ProductoService);
  private readonly categoriaSvc  = inject(CategoriaService);
  private readonly proveedorSvc  = inject(ProveedorService);
  private readonly movimientoSvc = inject(MovimientoStockService);
  private readonly fb            = inject(FormBuilder);
  private readonly cdr           = inject(ChangeDetectorRef);

  productos:  Producto[]   = [];
  categorias: Categoria[]  = [];
  proveedores: Proveedor[] = [];
  productosConCategoriaInvalida: Producto[] = [];

  cargando       = false;
  modalAbierto   = false;
  modalStock     = false;
  editando: Producto | null = null;
  busqueda       = '';
  soloStockBajo  = false;
  categoriaFiltro = '';
  proveedorFiltro = '';
  tiposMovimiento = TIPOS_MOVIMIENTO;

  form!: FormGroup;
  stockForm!: FormGroup;
  productoSeleccionado: Producto | null = null;

  get productosFiltrados(): Producto[] {
    return this.productos.filter(p => {
      const matchBusqueda = !this.busqueda ||
        p.nombre.toLowerCase().includes(this.busqueda.toLowerCase()) ||
        p.codigo.toLowerCase().includes(this.busqueda.toLowerCase());
      const matchStock = !this.soloStockBajo || p.stockBajo;
      const matchCategoria = !this.categoriaFiltro || p.categoriaId === +this.categoriaFiltro;
      const matchProveedor = !this.proveedorFiltro || p.proveedorId === +this.proveedorFiltro;
      return matchBusqueda && matchStock && matchCategoria && matchProveedor;
    });
  }

  ngOnInit(): void {
    this.initForms();
    this.cargar();
  }

  private initForms(): void {
    this.form = this.fb.group({
      codigo:      ['', [Validators.required, Validators.maxLength(20)]],
      nombre:      ['', [Validators.required, Validators.maxLength(100)]],
      descripcion: [''],
      precioCompra:['', [Validators.required, Validators.min(0)]],
      precioVenta: ['', [Validators.required, Validators.min(0)]],
      stockMinimo: ['', [Validators.required, Validators.min(0)]],
      stockActual: ['', [Validators.required, Validators.min(0)]],
      categoriaId: ['', Validators.required],
      proveedorId: ['']
    });
    this.stockForm = this.fb.group({
      cantidad:       ['', [Validators.required, Validators.min(1)]],
      tipoMovimiento: ['ENTRADA', Validators.required],
      observacion:    ['']
    });
  }

  cargar(): void {
    forkJoin({
      productos:  this.productoSvc.getAll(),
      categorias: this.categoriaSvc.getAll()
    }).subscribe({
      next: ({ productos, categorias }) => {
        this.productos  = productos.data  ?? [];
        this.categorias = categorias.data ?? [];
        this.verificarCategorias();
        this.cdr.detectChanges();
      },
      error: () => this.cdr.detectChanges()
    });
    this.proveedorSvc.getAll().subscribe(r => { this.proveedores = r.data ?? []; this.cdr.detectChanges(); });
  }

  private verificarCategorias(): void {
    const idsValidos = new Set(this.categorias.map(c => c.id));
    this.productosConCategoriaInvalida = this.productos.filter(p => !idsValidos.has(p.categoriaId));
  }

  abrirNuevo(): void {
    this.editando = null;
    this.form.reset({ stockActual: 0, stockMinimo: 0, precioCompra: 0, precioVenta: 0 });
    this.form.get('codigo')?.enable();
    this.modalAbierto = true;
  }

  abrirEdicion(p: Producto): void {
    this.editando = p;
    this.form.patchValue({
      codigo: p.codigo, nombre: p.nombre, descripcion: p.descripcion,
      precioCompra: p.precioCompra, precioVenta: p.precioVenta,
      stockMinimo: p.stockMinimo, stockActual: p.stockActual,
      categoriaId: p.categoriaId, proveedorId: p.proveedorId ?? ''
    });
    this.form.get('codigo')?.disable();
    this.modalAbierto = true;
  }

  guardar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const val = this.form.getRawValue();
    if (this.editando) {
      this.productoSvc.update(this.editando.id, {
        nombre: val.nombre, descripcion: val.descripcion,
        precioCompra: +val.precioCompra, precioVenta: +val.precioVenta,
        stockMinimo: +val.stockMinimo, categoriaId: +val.categoriaId,
        proveedorId: val.proveedorId || undefined
      }).subscribe(() => { this.cerrarModal(); this.cargar(); });
    } else {
      this.productoSvc.create({
        codigo: val.codigo, nombre: val.nombre, descripcion: val.descripcion,
        precioCompra: +val.precioCompra, precioVenta: +val.precioVenta,
        stockMinimo: +val.stockMinimo, stockActual: +val.stockActual,
        categoriaId: +val.categoriaId, proveedorId: val.proveedorId || undefined
      }).subscribe(() => { this.cerrarModal(); this.cargar(); });
    }
  }

  eliminar(p: Producto): void {
    if (!confirm(`¿Eliminar el producto "${p.nombre}"?`)) return;
    this.productoSvc.delete(p.id).subscribe(() => this.cargar());
  }

  abrirAjusteStock(p: Producto): void {
    this.productoSeleccionado = p;
    this.stockForm.reset({ tipoMovimiento: 'ENTRADA' });
    this.modalStock = true;
  }

  registrarMovimiento(): void {
    if (this.stockForm.invalid || !this.productoSeleccionado) { this.stockForm.markAllAsTouched(); return; }
    const val = this.stockForm.value;
    this.movimientoSvc.registrar({
      productoId: this.productoSeleccionado.id,
      cantidad: +val.cantidad,
      tipoMovimiento: val.tipoMovimiento,
      observacion: val.observacion
    }).subscribe(() => { this.modalStock = false; this.cargar(); });
  }

  cerrarModal(): void { this.modalAbierto = false; this.editando = null; }

  isInvalid(campo: string): boolean {
    const c = this.form.get(campo);
    return !!(c?.invalid && c?.touched);
  }
}
