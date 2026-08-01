import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RoleAdminService } from '../../../core/services/role-admin.service';
import { Role } from '../../../core/models/roles/role.model';

interface CategoriaPermisos {
  nombre: string;
  claves: string[];
}

const CATEGORIA_LABELS: Record<string, string> = {
  dashboard:   'Dashboard',
  usuarios:    'Usuarios',
  roles:       'Roles y permisos',
  security:    'Seguridad',
  productos:   'Productos',
  categorias:  'Categorías',
  proveedores: 'Proveedores',
  stock:       'Stock',
  ventas:      'Ventas',
  clientes:    'Clientes',
  compras:     'Compras',
  facturas:    'Facturación',
  reportes:    'Reportes'
};

const PERMISO_LABELS: Record<string, string> = {
  'dashboard.view':          'Ver el panel principal',
  'usuarios.view':           'Ver usuarios',
  'usuarios.create':         'Crear usuarios',
  'usuarios.edit':           'Editar usuarios',
  'usuarios.delete':         'Eliminar usuarios',
  'usuarios.reset_password': 'Restablecer contraseñas',
  'roles.view':              'Ver roles',
  'roles.create':            'Crear roles',
  'roles.edit':              'Editar roles',
  'roles.delete':            'Eliminar roles',
  'security.view':           'Ver auditoría del sistema',
  'security.manage':         'Administrar la seguridad del sistema',
  'productos.view':          'Ver productos',
  'productos.create':        'Crear productos',
  'productos.edit':          'Editar productos',
  'productos.delete':        'Eliminar productos',
  'categorias.view':         'Ver categorías',
  'categorias.create':       'Crear categorías',
  'categorias.edit':         'Editar categorías',
  'categorias.delete':       'Eliminar categorías',
  'proveedores.view':        'Ver proveedores',
  'proveedores.create':      'Crear proveedores',
  'proveedores.edit':        'Editar proveedores',
  'proveedores.delete':      'Eliminar proveedores',
  'stock.view':              'Ver stock',
  'stock.registrar':         'Registrar movimientos de stock',
  'ventas.view':             'Ver ventas',
  'ventas.create':           'Registrar ventas',
  'ventas.anular':           'Anular ventas',
  'clientes.view':           'Ver clientes',
  'clientes.create':         'Crear clientes',
  'clientes.edit':           'Editar clientes',
  'compras.view':            'Ver compras',
  'compras.create':          'Registrar compras',
  'compras.anular':          'Anular compras',
  'facturas.view':           'Ver facturas',
  'facturas.anular':         'Anular facturas',
  'reportes.ver':            'Ver reportes',
  'reportes.exportar':       'Exportar reportes'
};

@Component({
  selector: 'app-roles-list',
  standalone: false,
  templateUrl: './roles-list.html',
  styleUrl: './roles-list.scss'
})
export class RolesListComponent implements OnInit {
  private readonly roleAdminService = inject(RoleAdminService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  roles: Role[] = [];
  categorias: CategoriaPermisos[] = [];
  cargando = true;
  error = '';

  mostrarForm = false;
  editandoId: number | null = null;
  editandoNombre = '';
  editandoIsSeeded = false;
  soloLectura = false;
  guardando = false;
  formError = '';
  permisosSeleccionados = new Set<string>();

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    isActive: [true]
  });

  ngOnInit(): void {
    this.roleAdminService.getCatalogoPermisos().subscribe(res => {
      this.categorias = this.agruparPorCategoria(res.data ?? []);
      this.cdr.detectChanges();
    });
    this.cargar();
  }

  private agruparPorCategoria(claves: string[]): CategoriaPermisos[] {
    const grupos = new Map<string, string[]>();
    for (const clave of claves) {
      const categoria = clave.split('.')[0];
      if (!grupos.has(categoria)) grupos.set(categoria, []);
      grupos.get(categoria)!.push(clave);
    }
    return Array.from(grupos.entries()).map(([nombre, claves]) => ({ nombre, claves }));
  }

  cargar(): void {
    this.cargando = true;
    this.roleAdminService.getAll().subscribe({
      next: res => {
        this.roles = res.data ?? [];
        this.cargando = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.cargando = false;
        this.cdr.detectChanges();
      }
    });
  }

  abrirCrear(): void {
    this.editandoId = null;
    this.editandoNombre = '';
    this.editandoIsSeeded = false;
    this.soloLectura = false;
    this.formError = '';
    this.guardando = false;
    this.form.reset({ name: '', description: '', isActive: true });
    this.form.enable();
    this.permisosSeleccionados = new Set();
    this.mostrarForm = true;
  }

  abrirEditar(role: Role): void {
    this.editandoId = role.id;
    this.editandoNombre = role.name;
    this.editandoIsSeeded = role.isSeeded;
    this.soloLectura = role.isSystem; // Administrador: solo lectura, nunca editable
    this.formError = '';
    this.guardando = false;
    this.form.reset({ name: role.name, description: role.description, isActive: role.isActive });
    // Administrador no guarda su catálogo en Permissions (siempre tiene acceso total sin importar
    // esa columna -- ver RolePermissions.RoleGrants) así que acá se muestra el catálogo completo,
    // no el valor (vacío) de role.permissions.
    this.permisosSeleccionados = role.isSystem
      ? new Set(this.categorias.flatMap(c => c.claves))
      : new Set(role.permissions);
    if (this.soloLectura) this.form.disable(); else this.form.enable();
    this.mostrarForm = true;
  }

  cancelar(): void {
    this.mostrarForm = false;
  }

  labelCategoria(nombre: string): string {
    return CATEGORIA_LABELS[nombre] ?? nombre;
  }

  labelPermiso(clave: string): string {
    return PERMISO_LABELS[clave] ?? clave;
  }

  togglePermiso(clave: string): void {
    if (this.soloLectura) return;
    if (this.permisosSeleccionados.has(clave)) this.permisosSeleccionados.delete(clave);
    else this.permisosSeleccionados.add(clave);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formError = 'Completá correctamente los campos requeridos (nombre).';
      return;
    }
    this.guardando = true;
    this.formError = '';
    const valor = this.form.value;
    const permissions = Array.from(this.permisosSeleccionados);

    const onDone = (exitoso: boolean, mensaje?: string) => {
      this.guardando = false;
      if (exitoso) {
        this.mostrarForm = false;
        this.cargar();
      } else {
        this.formError = mensaje ?? 'No se pudo guardar el rol.';
      }
      this.cdr.detectChanges();
    };

    if (this.editandoId) {
      this.roleAdminService.update(this.editandoId, {
        name: valor.name, description: valor.description, isActive: valor.isActive, permissions
      }).subscribe({
        next: res => onDone(res.exitoso, res.mensaje),
        error: err => onDone(false, err.error?.mensaje)
      });
    } else {
      this.roleAdminService.create({ name: valor.name, description: valor.description, permissions }).subscribe({
        next: res => onDone(res.exitoso, res.mensaje),
        error: err => onDone(false, err.error?.mensaje)
      });
    }
  }

  eliminar(role: Role): void {
    if (role.isSystem || role.isSeeded) return;
    if (role.cantidadUsuarios > 0) {
      this.error = 'No se puede eliminar un rol con usuarios asignados.';
      return;
    }
    if (!window.confirm(`¿Eliminar el rol "${role.name}"?`)) return;

    this.roleAdminService.delete(role.id).subscribe({
      next: res => {
        if (res.exitoso) this.cargar();
        else {
          this.error = res.mensaje ?? 'No se pudo eliminar el rol.';
          this.cdr.detectChanges();
        }
      }
    });
  }
}
