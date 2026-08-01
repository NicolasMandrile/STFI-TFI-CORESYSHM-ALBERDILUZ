import { Component, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: false,
  templateUrl: './layout.html',
  styleUrl: './layout.scss'
})
export class LayoutComponent {
  private readonly authService = inject(AuthService);
  private readonly sanitizer   = inject(DomSanitizer);
  currentUser$ = this.authService.currentUser$;
  sidebarOpen  = true;

  readonly navItems: { section: string | null; label: string; icon: string; route: string; permiso?: string }[] = [
    { section: null,         label: 'Dashboard',    icon: 'grid',       route: '/dashboard' },
    { section: 'Stock',      label: 'Productos',    icon: 'box',        route: '/stock/productos' },
    { section: null,         label: 'Categorías',   icon: 'tag',        route: '/stock/categorias' },
    { section: null,         label: 'Movimientos',  icon: 'activity',   route: '/stock/movimientos' },
    { section: 'Ventas',     label: 'Nueva Venta',  icon: 'shopping-cart', route: '/ventas/nueva' },
    { section: null,         label: 'Historial',    icon: 'list',       route: '/ventas' },
    { section: null,         label: 'Clientes',     icon: 'users',      route: '/ventas/clientes' },
    { section: 'Compras',    label: 'Nueva Compra', icon: 'truck',      route: '/compras/nueva' },
    { section: null,         label: 'Historial',    icon: 'list',       route: '/compras' },
    { section: null,         label: 'Proveedores',  icon: 'users',      route: '/compras/proveedores' },
    { section: 'Facturación',label: 'Facturas',     icon: 'file-text',  route: '/facturacion' },
    { section: 'Reportes',  label: 'Ventas',         icon: 'bar-chart',     route: '/reportes' },
    { section: null,        label: 'Compras',        icon: 'trending-down', route: '/reportes-compras' },
    { section: 'Seguridad', label: 'Usuarios',       icon: 'users',    route: '/usuarios',  permiso: 'usuarios.view' },
    { section: null,        label: 'Roles',          icon: 'tag',      route: '/roles',     permiso: 'security.view' },
    { section: null,        label: 'Auditoría',      icon: 'activity', route: '/auditoria', permiso: 'security.view' },
    { section: null,        label: 'Mi Perfil',      icon: 'list',     route: '/perfil' },
  ];

  readonly icons: Record<string, SafeHtml>;

  constructor() {
    const raw: Record<string, string> = {
      'grid':          '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>',
      'box':           '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>',
      'tag':           '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/></svg>',
      'activity':      '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>',
      'shopping-cart': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg>',
      'list':          '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>',
      'users':         '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
      'file-text':     '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>',
      'truck':         '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="1" y="3" width="15" height="13"/><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/></svg>',
      'bar-chart':     '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/><line x1="2" y1="20" x2="22" y2="20"/></svg>',
      'trending-down': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="23 18 13.5 8.5 8.5 13.5 1 6"/><polyline points="17 18 23 18 23 12"/></svg>',
    };
    this.icons = Object.fromEntries(
      Object.entries(raw).map(([k, v]) => [k, this.sanitizer.bypassSecurityTrustHtml(v)])
    );
  }

  getIcon(name: string): SafeHtml {
    return this.icons[name] ?? '';
  }

  logout(): void {
    this.authService.logout();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }
}
