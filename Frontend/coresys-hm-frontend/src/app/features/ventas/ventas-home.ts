import { Component } from '@angular/core';

@Component({
  selector: 'app-ventas-home',
  standalone: false,
  template: `
    <div class="coming-soon">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#4f46e5" stroke-width="1.5">
        <circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>
        <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>
      </svg>
      <h2>Módulo Ventas</h2>
      <p>Esta sección está en desarrollo. Próximamente disponible.</p>
    </div>
  `,
  styles: [`
    .coming-soon{display:flex;flex-direction:column;align-items:center;justify-content:center;
      padding:4rem 2rem;text-align:center;color:#64748b;gap:1rem;}
    h2{color:#1e293b;margin:0;}p{margin:0;font-size:.95rem;}
  `]
})
export class VentasHomeComponent { }
