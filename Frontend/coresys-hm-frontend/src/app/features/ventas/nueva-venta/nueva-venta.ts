import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VentaService } from '../../../core/services/venta.service';
import { ProductoService } from '../../../core/services/producto.service';
import { AuthService } from '../../../core/services/auth.service';
import { Cliente } from '../../../core/models/ventas/cliente.model';
import { Producto } from '../../../core/models/stock/producto.model';
import { Categoria } from '../../../core/models/stock/categoria.model';
import { ItemCarrito } from '../../../core/models/ventas/venta.model';

@Component({
  selector: 'app-nueva-venta',
  standalone: false,
  templateUrl: './nueva-venta.html',
  styleUrl: './nueva-venta.scss'
})
export class NuevaVentaComponent implements OnInit {
  private readonly ventaSvc     = inject(VentaService);
  private readonly productoSvc  = inject(ProductoService);
  private readonly authSvc      = inject(AuthService);
  private readonly fb          = inject(FormBuilder);
  private readonly router      = inject(Router);
  private readonly cdr         = inject(ChangeDetectorRef);

  /** Rol Cliente: registra su propia venta -- el backend fuerza el ClienteId, no hay selector. */
  readonly esCliente = this.authSvc.currentUser?.rol === 'Cliente';

  clientes:   Cliente[]     = [];
  productos:  Producto[]    = [];
  categorias: Categoria[]   = [];
  carrito:    ItemCarrito[] = [];

  cargando  = false;
  guardando = false;
  error     = '';
  exito     = '';

  ventaForm!: FormGroup;
  busquedaProducto = '';
  categoriaSeleccionada = '';
  modalCliente = false;
  clienteForm!: FormGroup;

  get clienteSeleccionado(): Cliente | undefined {
    const id = +this.ventaForm?.get('clienteId')?.value;
    return id ? this.clientes.find(c => c.id === id) : undefined;
  }

  get clienteSinDireccion(): boolean {
    const c = this.clienteSeleccionado;
    return !!c && (!c.direccion?.trim() || !c.localidad?.trim());
  }

  get productosFiltrados(): Producto[] {
    let lista = this.productos;
    if (this.categoriaSeleccionada) {
      const catId = +this.categoriaSeleccionada;
      lista = lista.filter(p => p.categoriaId === catId);
    }
    if (this.busquedaProducto) {
      const q = this.busquedaProducto.toLowerCase();
      lista = lista.filter(p =>
        p.nombre.toLowerCase().includes(q) || p.codigo.toLowerCase().includes(q)
      );
    }
    return lista;
  }

  get subtotal(): number { return this.carrito.reduce((s, i) => s + i.precioUnitario * i.cantidad, 0); }
  get pctDescuento(): number { return +(this.ventaForm?.get('descuento')?.value ?? 0); }
  get descuento(): number { return Math.round(this.subtotal * (this.pctDescuento / 100) * 100) / 100; }
  get total(): number { return Math.max(0, this.subtotal - this.descuento); }

  ngOnInit(): void {
    this.ventaForm = this.fb.group({
      clienteId:     ['', this.esCliente ? [] : Validators.required],
      descuento:     [0, [Validators.min(0), Validators.max(100)]],
      observaciones: ['']
    });
    this.clienteForm = this.fb.group({
      nombre: ['', Validators.required], apellido: ['', Validators.required],
      dni: [''], telefono: [''], email: [''], localidad: [''], direccion: ['']
    });
    this.cargar();
  }

  private cargar(): void {
    // Rol Cliente no tiene permiso clientes.view (solo vería su propio registro, que el backend
    // ya resuelve solo -- no hace falta listar clientes acá).
    if (!this.esCliente) {
      this.ventaSvc.getClientes().subscribe(r => { this.clientes = r.data ?? []; this.cdr.detectChanges(); });
    }
    this.productoSvc.getAll().subscribe(r => {
      this.productos = (r.data ?? []).filter(p => p.stockActual > 0);
      // Se arma acá (no via CategoriaService) porque el rol Cliente no tiene permiso
      // categorias.view: cada producto ya trae categoriaId/categoriaNombre.
      const porId = new Map(this.productos.map(p => [p.categoriaId, { id: p.categoriaId, nombre: p.categoriaNombre }]));
      this.categorias = Array.from(porId.values()).sort((a, b) => a.nombre.localeCompare(b.nombre));
      this.cdr.detectChanges();
    });
  }

  agregarAlCarrito(p: Producto): void {
    const existe = this.carrito.find(i => i.productoId === p.id);
    if (existe) {
      if (existe.cantidad < p.stockActual) existe.cantidad++;
    } else {
      this.carrito.push({
        productoId: p.id, productoNombre: p.nombre,
        productoCodigo: p.codigo, cantidad: 1,
        precioUnitario: p.precioVenta, stockDisponible: p.stockActual
      });
    }
    this.error = '';
    this.cdr.detectChanges();
  }

  cambiarCantidad(item: ItemCarrito, delta: number): void {
    const nueva = item.cantidad + delta;
    if (nueva < 1) { this.quitarDelCarrito(item); return; }
    if (nueva > item.stockDisponible) { this.error = `Stock insuficiente para "${item.productoNombre}".`; return; }
    item.cantidad = nueva;
    this.cdr.detectChanges();
  }

  quitarDelCarrito(item: ItemCarrito): void {
    this.carrito = this.carrito.filter(i => i.productoId !== item.productoId);
    this.cdr.detectChanges();
  }

  confirmarVenta(): void {
    if (this.ventaForm.invalid) { this.ventaForm.markAllAsTouched(); return; }
    if (this.carrito.length === 0) { this.error = 'Agregue al menos un producto.'; return; }
    this.guardando = true; this.error = '';
    this.ventaSvc.create({
      // Rol Cliente: el backend ignora este valor y fuerza el cliente vinculado al login.
      clienteId:     this.esCliente ? 0 : +this.ventaForm.value.clienteId,
      descuento:     this.descuento,
      observaciones: this.ventaForm.value.observaciones,
      detalles: this.carrito.map(i => ({ productoId: i.productoId, cantidad: i.cantidad }))
    }).subscribe({
      next: r => {
        this.guardando = false;
        this.exito = `Venta ${r.data?.numeroVenta} registrada correctamente.`;
        this.carrito = []; this.ventaForm.reset({ descuento: 0 });
        this.cdr.detectChanges();
        setTimeout(() => this.router.navigate(['/ventas']), 2000);
      },
      error: e => {
        this.guardando = false;
        this.error = e.error?.mensaje ?? 'Error al registrar la venta.';
        this.cdr.detectChanges();
      }
    });
  }

  guardarCliente(): void {
    if (this.clienteForm.invalid) { this.clienteForm.markAllAsTouched(); return; }
    this.ventaSvc.createCliente(this.clienteForm.value).subscribe(r => {
      this.clientes.push(r.data!);
      this.ventaForm.get('clienteId')?.setValue(r.data!.id);
      this.modalCliente = false;
      this.clienteForm.reset();
      this.cdr.detectChanges();
    });
  }
}
