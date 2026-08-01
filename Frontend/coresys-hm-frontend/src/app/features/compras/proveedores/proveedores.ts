import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { Proveedor } from '../../../core/models/stock/proveedor.model';

@Component({
  selector: 'app-proveedores',
  standalone: false,
  templateUrl: './proveedores.html',
  styleUrl: './proveedores.scss'
})
export class ProveedoresComponent implements OnInit {
  private readonly proveedorSvc = inject(ProveedorService);
  private readonly fb           = inject(FormBuilder);
  private readonly cdr          = inject(ChangeDetectorRef);

  proveedores: Proveedor[] = [];
  cargando     = false;
  modalAbierto = false;
  editando: Proveedor | null = null;
  busqueda     = '';

  form!: FormGroup;

  get proveedoresFiltrados(): Proveedor[] {
    if (!this.busqueda) return this.proveedores;
    const q = this.busqueda.toLowerCase();
    return this.proveedores.filter(p =>
      p.razonSocial.toLowerCase().includes(q)        ||
      (p.cuit?.toLowerCase().includes(q)     ?? false) ||
      (p.email?.toLowerCase().includes(q)    ?? false) ||
      (p.contacto?.toLowerCase().includes(q) ?? false)
    );
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      razonSocial: ['', Validators.required],
      cuit:        [''],
      telefono:    [''],
      email:       ['', Validators.email],
      direccion:   [''],
      contacto:    ['']
    });
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    this.proveedorSvc.getAll().subscribe({
      next: r  => { this.proveedores = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  abrirNuevo(): void {
    this.editando = null;
    this.form.reset();
    this.modalAbierto = true;
  }

  abrirEdicion(p: Proveedor): void {
    this.editando = p;
    this.form.patchValue({
      razonSocial: p.razonSocial, cuit: p.cuit, telefono: p.telefono,
      email: p.email, direccion: p.direccion, contacto: p.contacto
    });
    this.modalAbierto = true;
  }

  guardar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const val = this.form.value;
    const dto = {
      razonSocial: val.razonSocial.trim(),
      cuit:        val.cuit?.trim()      || '',
      telefono:    val.telefono?.trim()  || undefined,
      email:       val.email?.trim()     || undefined,
      direccion:   val.direccion?.trim() || undefined,
      contacto:    val.contacto?.trim()  || undefined
    };
    const accion = this.editando
      ? this.proveedorSvc.update(this.editando.id, dto)
      : this.proveedorSvc.create(dto);
    accion.subscribe(() => { this.cerrarModal(); this.cargar(); });
  }

  eliminar(p: Proveedor): void {
    if (!confirm(`¿Eliminar el proveedor "${p.razonSocial}"?`)) return;
    this.proveedorSvc.delete(p.id).subscribe(() => this.cargar());
  }

  cerrarModal(): void { this.modalAbierto = false; this.editando = null; }

  isInvalid(campo: string): boolean {
    const c = this.form.get(campo);
    return !!(c?.invalid && c?.touched);
  }
}
