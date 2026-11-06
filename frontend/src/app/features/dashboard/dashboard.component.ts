import { Component, inject } from '@angular/core';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="bg-white rounded-xl border border-slate-200 p-6 shadow-sm">
      <h2 class="text-xl font-semibold text-slate-900">Painel do Gestor</h2>
      <p class="text-slate-600 mt-2">
        Autenticação concluída com sucesso para <strong>{{ authStore.usuario()?.email }}</strong>.
      </p>
    </div>
  `
})
export class DashboardComponent {
  readonly authStore = inject(AuthStore);
}
