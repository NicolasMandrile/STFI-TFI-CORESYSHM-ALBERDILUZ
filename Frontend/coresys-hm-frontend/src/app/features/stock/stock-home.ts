import { Component } from '@angular/core';

@Component({
  selector: 'app-stock-home',
  standalone: false,
  template: `
    <div class="coming-soon">
      <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#4f46e5" stroke-width="1.5">
        <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
      </svg>
      <h2>Módulo Stock</h2>
      <p>Esta sección está en desarrollo. Próximamente disponible.</p>
    </div>
  `,
  styles: [`
    .coming-soon { display:flex;flex-direction:column;align-items:center;justify-content:center;
      padding:4rem 2rem;text-align:center;color:#64748b;gap:1rem; }
    h2{color:#1e293b;margin:0;}
    p{margin:0;font-size:.95rem;}
  `]
})
export class StockHomeComponent { }
