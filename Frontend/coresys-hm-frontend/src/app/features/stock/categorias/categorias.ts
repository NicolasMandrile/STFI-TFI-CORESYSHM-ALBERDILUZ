import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoriaService } from '../../../core/services/categoria.service';
import { Categoria } from '../../../core/models/stock/categoria.model';

@Component({
  selector: 'app-categorias',
  standalone: false,
  templateUrl: './categorias.html',
  styleUrl: './categorias.scss'
})
export class CategoriasComponent implements OnInit {
  private readonly categoriaSvc = inject(CategoriaService);
  private readonly fb           = inject(FormBuilder);
  private readonly cdr          = inject(ChangeDetectorRef);

  categorias:  Categoria[] = [];
  cargando     = false;
  modalAbierto = false;
  editando: Categoria | null = null;
  busqueda = '';

  form!: FormGroup;

  get categoriasFiltradas(): Categoria[] {
    if (!this.busqueda) return this.categorias;
    const q = this.busqueda.toLowerCase();
    return this.categorias.filter(c =>
      c.nombre.toLowerCase().includes(q) ||
      (c.descripcion?.toLowerCase().includes(q) ?? false)
    );
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      nombre:      ['', [Validators.required, Validators.maxLength(100)]],
      descripcion: ['', Validators.maxLength(300)]
    });
    this.cargar();
  }

  cargar(): void {
    this.cargando = true;
    this.categoriaSvc.getAll().subscribe({
      next: r  => { this.categorias = r.data ?? []; this.cargando = false; this.cdr.detectChanges(); },
      error: () => { this.cargando = false; this.cdr.detectChanges(); }
    });
  }

  abrirNuevo(): void {
    this.editando = null;
    this.form.reset();
    this.modalAbierto = true;
  }

  abrirEdicion(c: Categoria): void {
    this.editando = c;
    this.form.patchValue({ nombre: c.nombre, descripcion: c.descripcion ?? '' });
    this.modalAbierto = true;
  }

  guardar(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const val = this.form.value;
    const dto = { nombre: val.nombre.trim(), descripcion: val.descripcion?.trim() || undefined };

    if (this.editando) {
      this.categoriaSvc.update(this.editando.id, dto).subscribe(() => { this.cerrarModal(); this.cargar(); });
    } else {
      this.categoriaSvc.create(dto).subscribe(() => { this.cerrarModal(); this.cargar(); });
    }
  }

  eliminar(c: Categoria): void {
    if (!confirm(`¿Eliminar la categoría "${c.nombre}"?\nLos productos asignados quedarán sin categoría válida.`)) return;
    this.categoriaSvc.delete(c.id).subscribe(() => this.cargar());
  }

  cerrarModal(): void { this.modalAbierto = false; this.editando = null; }

  isInvalid(campo: string): boolean {
    const c = this.form.get(campo);
    return !!(c?.invalid && c?.touched);
  }
}
