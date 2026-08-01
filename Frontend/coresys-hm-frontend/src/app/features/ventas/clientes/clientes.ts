import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VentaService } from '../../../core/services/venta.service';
import { Cliente } from '../../../core/models/ventas/cliente.model';

@Component({
  selector: 'app-clientes',
  standalone: false,
  templateUrl: './clientes.html',
  styleUrl: './clientes.scss'
})
export class ClientesComponent implements OnInit {
  private readonly ventaSvc = inject(VentaService);
  private readonly fb       = inject(FormBuilder);
  private readonly cdr      = inject(ChangeDetectorRef);

  clientes:    Cliente[] = [];
  cargando     = false;
  modalAbierto = false;
  busqueda     = '';

  form!: FormGroup;

  get clientesFiltrados(): Cliente[] {
    if (!this.busqueda) return this.clientes;
    const q = this.busqueda.toLowerCase();
    return this.clientes.filter(c =>
      c.nombre.toLowerCase().includes(q)                ||
      c.apellido.toLowerCase().includes(q)              ||
      (c.dni?.toLowerCase().includes(q)       ?? false) ||
      (c.email?.toLowerCase().includes(q)     ?? false) ||
      (c.localidad?.toLowerCase().includes(q) ?? false)
    );
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      nombre:    ['', Validators.required],
      apellido:  ['', Validators.required],
      dni:       [''],
      telefono:  [''],
      email:     ['', Validators.email],
      direccion: [''],
      localidad: ['']
    });
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    this.ventaSvc.getClientes().subscribe({
      next: r  => { this.clientes = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  abrirNuevo(): void {
    this.form.reset();
    this.modalAbierto = true;
  }

  guardar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const val = this.form.value;
    this.ventaSvc.createCliente({
      nombre:    val.nombre.trim(),
      apellido:  val.apellido.trim(),
      dni:       val.dni?.trim()       || undefined,
      telefono:  val.telefono?.trim()  || undefined,
      email:     val.email?.trim()     || undefined,
      direccion: val.direccion?.trim() || undefined,
      localidad: val.localidad?.trim() || undefined
    }).subscribe(() => { this.cerrarModal(); this.cargar(); });
  }

  cerrarModal(): void { this.modalAbierto = false; }

  isInvalid(campo: string): boolean {
    const c = this.form.get(campo);
    return !!(c?.invalid && c?.touched);
  }
}
