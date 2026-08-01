import { Component } from '@angular/core';

@Component({
  selector: 'app-facturacion-home',
  standalone: false,
  template: `
    <div class="coming-soon">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#4f46e5" stroke-width="1.5">
        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
        <polyline points="14 2 14 8 20 8"/>
        <line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>
      </svg>
      <h2>Módulo Facturación</h2>
      <p>Esta sección está en desarrollo. Próximamente disponible.</p>
    </div>
  `,
  styles: [`
    .coming-soon{display:flex;flex-direction:column;align-items:center;justify-content:center;
      padding:4rem 2rem;text-align:center;color:#64748b;gap:1rem;}
    h2{color:#1e293b;margin:0;}p{margin:0;font-size:.95rem;}
  `]
})
export class FacturacionHomeComponent { }
