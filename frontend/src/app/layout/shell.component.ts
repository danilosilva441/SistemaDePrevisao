import { Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../core/auth/auth.store';
import { UnidadeStore } from '../core/unidades/unidade.store';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen bg-slate-50 flex flex-col">
      <header class="bg-white border-b border-slate-200 px-6 py-3.5 flex items-center justify-between">
        <div class="flex items-center gap-6">
          <span class="font-bold text-lg text-slate-900">Previsão de Faturamento</span>
          <nav class="flex items-center gap-4 text-sm font-medium text-slate-600">
            <a routerLink="/dashboard" routerLinkActive="text-blue-600" class="hover:text-slate-900">
              Dashboard
            </a>
          </nav>
        </div>

        <div class="flex items-center gap-6">
          <!-- Seletor Rápido de Unidade Ativa -->
          @if (unidadeStore.unidades().length > 0) {
            <div class="flex items-center gap-2">
              <label for="unidadeAtiva" class="text-sm text-slate-500 font-medium">Unidade:</label>
              <select
                id="unidadeAtiva"
                class="text-sm border-slate-300 rounded-md focus:ring-blue-600 focus:border-blue-600 py-1.5 pl-3 pr-8 bg-slate-50"
                [value]="unidadeStore.unidadeAtiva()?.id"
                (change)="mudarUnidade($event)">
                @for (u of unidadeStore.unidades(); track u.id) {
                  <option [value]="u.id">{{ u.nome }}</option>
                }
              </select>
            </div>
          }

          <div class="flex items-center gap-4 border-l border-slate-200 pl-6">
            @if (authStore.usuario(); as gestor) {
              <span class="text-sm text-slate-600">
                Gestor: <strong class="text-slate-900">{{ gestor.nome }}</strong>
              </span>
            }
            <button
              type="button"
              (click)="authStore.sair()"
              class="text-sm px-3 py-1.5 rounded-lg border border-slate-300 text-slate-700 hover:bg-slate-100 transition-colors"
            >
              Sair
            </button>
          </div>
        </div>
      </header>

      <main class="flex-1 p-6 max-w-7xl w-full mx-auto">
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  readonly unidadeStore = inject(UnidadeStore);

  ngOnInit(): void {
    if (!this.authStore.usuario()) {
      this.authStore.carregarPerfil().subscribe();
    }
    // Dispara a busca das unidades ao montar o layout principal
    this.unidadeStore.listar().subscribe();
  }

  mudarUnidade(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.unidadeStore.selecionarUnidade(select.value);
  }
}
