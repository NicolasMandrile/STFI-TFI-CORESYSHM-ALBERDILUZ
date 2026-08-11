import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ClienteService } from '../../../core/services/cliente.service';
import { CondicionFiscalService } from '../../../core/services/condicion-fiscal.service';
import { Cliente } from '../../../core/models/ventas/cliente.model';
import { CondicionFiscal } from '../../../core/models/common/condicion-fiscal.model';
import { HistorialCambio } from '../../../core/models/common/historial-cambio.model';

@Component({
  selector: 'app-clientes',
  standalone: false,
  templateUrl: './clientes.html',
  styleUrl: './clientes.scss'
})
export class ClientesComponent implements OnInit {
  private readonly clienteSvc = inject(ClienteService);
  private readonly condFiscalSvc = inject(CondicionFiscalService);
  private readonly fb  = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  clientes: Cliente[] = [];
  condicionesFiscales: CondicionFiscal[] = [];
  cargando     = false;
  modalAbierto = false;
  busqueda     = '';
  editando: Cliente | null = null;

  historialAbierto = false;
  historial: HistorialCambio[] = [];

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
      nombre:            ['', Validators.required],
      apellido:          ['', Validators.required],
      dni:               [''],
      cuit:              [''],
      telefono:          [''],
      email:             ['', Validators.email],
      direccion:         [''],
      localidad:         [''],
      condicionFiscalId: ['']
    });
    this.cargar();
    this.condFiscalSvc.getAll().subscribe(r => { this.condicionesFiscales = r.data ?? []; this.cdr.detectChanges(); });
  }

  cargar(): void {
    this.cargando = true;
    this.clienteSvc.getAll().subscribe({
      next: r  => { this.clientes = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  completitudClass(pct: number): string {
    if (pct >= 80) return 'tag-ok';
    if (pct >= 40) return 'tag-warn';
    return 'tag-danger';
  }

  abrirNuevo(): void {
    this.editando = null;
    this.form.reset();
    this.modalAbierto = true;
  }

  abrirEdicion(c: Cliente): void {
    this.editando = c;
    this.form.reset({
      nombre: c.nombre, apellido: c.apellido, dni: c.dni ?? '', cuit: c.cuit ?? '',
      telefono: c.telefono ?? '', email: c.email ?? '', direccion: c.direccion ?? '',
      localidad: c.localidad ?? '', condicionFiscalId: c.condicionFiscalId ?? ''
    });
    this.modalAbierto = true;
  }

  guardar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const val = this.form.value;
    const dto = {
      nombre:            val.nombre.trim(),
      apellido:          val.apellido.trim(),
      dni:               val.dni?.trim()       || undefined,
      cuit:              val.cuit?.trim()      || undefined,
      telefono:          val.telefono?.trim()  || undefined,
      email:             val.email?.trim()     || undefined,
      direccion:         val.direccion?.trim() || undefined,
      localidad:         val.localidad?.trim() || undefined,
      condicionFiscalId: val.condicionFiscalId ? +val.condicionFiscalId : undefined
    };

    const op = this.editando ? this.clienteSvc.update(this.editando.id, dto) : this.clienteSvc.create(dto);
    op.subscribe({
      next: () => { this.cerrarModal(); this.cargar(); },
      error: e => alert(e.error?.mensaje ?? 'No se pudo guardar el cliente.')
    });
  }

  eliminar(c: Cliente): void {
    if (!confirm(`¿Dar de baja a ${c.nombre} ${c.apellido}? Se conserva su historial de ventas/facturas.`)) return;
    this.clienteSvc.delete(c.id).subscribe({
      next: () => this.cargar(),
      error: e => alert(e.error?.mensaje ?? 'No se pudo eliminar el cliente.')
    });
  }

  verHistorial(c: Cliente): void {
    this.clienteSvc.getHistorial(c.id).subscribe(r => {
      this.historial = r.data ?? [];
      this.historialAbierto = true;
      this.cdr.detectChanges();
    });
  }

  cerrarModal(): void { this.modalAbierto = false; this.editando = null; }

  isInvalid(campo: string): boolean {
    const c = this.form.get(campo);
    return !!(c?.invalid && c?.touched);
  }
}
