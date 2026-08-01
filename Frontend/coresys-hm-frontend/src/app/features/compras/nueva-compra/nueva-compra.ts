import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CompraService } from '../../../core/services/compra.service';
import { ProductoService } from '../../../core/services/producto.service';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { Proveedor } from '../../../core/models/stock/proveedor.model';
import { Producto } from '../../../core/models/stock/producto.model';
import { CreateDetalleCompra } from '../../../core/models/compras/compra.model';

interface ItemCompra extends CreateDetalleCompra {
  productoNombre: string;
  productoCodigo: string;
  subtotal: number;
}

@Component({
  selector: 'app-nueva-compra',
  standalone: false,
  templateUrl: './nueva-compra.html',
  styleUrl: './nueva-compra.scss'
})
export class NuevaCompraComponent implements OnInit {
  private readonly compraSvc    = inject(CompraService);
  private readonly productoSvc  = inject(ProductoService);
  private readonly proveedorSvc = inject(ProveedorService);
  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly cdr          = inject(ChangeDetectorRef);

  proveedores: Proveedor[]  = [];
  productos:   Producto[]   = [];
  items:       ItemCompra[] = [];

  guardando = false;
  error     = '';
  exito     = '';

  compraForm!: FormGroup;
  busquedaProducto = '';

  get proveedorSeleccionado(): number { return +( this.compraForm?.get('proveedorId')?.value ?? 0); }

  get productosFiltrados(): Producto[] {
    const provId = this.proveedorSeleccionado;
    let lista = provId ? this.productos.filter(p => p.proveedorId === provId) : [];
    if (!this.busquedaProducto) return lista;
    const q = this.busquedaProducto.toLowerCase();
    return lista.filter(p => p.nombre.toLowerCase().includes(q) || p.codigo.toLowerCase().includes(q));
  }

  get total(): number { return this.items.reduce((s, i) => s + i.subtotal, 0); }

  ngOnInit(): void {
    this.compraForm = this.fb.group({
      proveedorId:   ['', Validators.required],
      observaciones: ['']
    });
    this.compraForm.get('proveedorId')!.valueChanges.subscribe(() => {
      this.items = [];
      this.busquedaProducto = '';
      this.cdr.detectChanges();
    });
    this.proveedorSvc.getAll().subscribe(r => { this.proveedores = r.data ?? []; this.cdr.detectChanges(); });
    this.productoSvc.getAll().subscribe(r => { this.productos = r.data ?? []; this.cdr.detectChanges(); });
  }

  agregarProducto(p: Producto): void {
    const existe = this.items.find(i => i.productoId === p.id);
    if (existe) { existe.cantidad++; existe.subtotal = existe.cantidad * existe.precioUnitario; }
    else {
      this.items.push({
        productoId: p.id, productoNombre: p.nombre, productoCodigo: p.codigo,
        cantidad: 1, precioUnitario: p.precioCompra, subtotal: p.precioCompra
      });
    }
    this.cdr.detectChanges();
  }

  cambiarCantidad(item: ItemCompra, delta: number): void {
    const nueva = item.cantidad + delta;
    if (nueva < 1) { this.items = this.items.filter(i => i.productoId !== item.productoId); }
    else { item.cantidad = nueva; item.subtotal = item.cantidad * item.precioUnitario; }
    this.cdr.detectChanges();
  }

  cambiarPrecio(item: ItemCompra, precio: number): void {
    item.precioUnitario = precio;
    item.subtotal = item.cantidad * precio;
    this.cdr.detectChanges();
  }

  confirmar(): void {
    if (this.compraForm.invalid) { this.compraForm.markAllAsTouched(); return; }
    if (this.items.length === 0) { this.error = 'Agregue al menos un producto.'; return; }
    this.guardando = true; this.error = '';
    this.compraSvc.create({
      proveedorId:   +this.compraForm.value.proveedorId,
      observaciones: this.compraForm.value.observaciones,
      detalles: this.items.map(i => ({ productoId: i.productoId, cantidad: i.cantidad, precioUnitario: i.precioUnitario }))
    }).subscribe({
      next: r => {
        this.guardando = false;
        this.exito = `Compra ${r.data?.numeroCompra} registrada. Stock actualizado.`;
        this.items = []; this.compraForm.reset();
        this.cdr.detectChanges();
        setTimeout(() => this.router.navigate(['/compras']), 2000);
      },
      error: e => {
        this.guardando = false;
        this.error = e.error?.mensaje ?? 'Error al registrar la compra.';
        this.cdr.detectChanges();
      }
    });
  }
}
