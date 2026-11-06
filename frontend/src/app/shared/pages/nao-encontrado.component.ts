import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nao-encontrado',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen flex flex-col items-center justify-center bg-slate-50 px-4 text-center">
      <h1 class="text-5xl font-bold text-slate-900">404</h1>
      <p class="text-slate-600 mt-2 mb-6">A página que você tentou acessar não existe.</p>
      <a routerLink="/dashboard" class="px-4 py-2 rounded-lg bg-blue-600 text-white font-medium hover:bg-blue-700">
        Voltar ao início
      </a>
    </div>
  `
})
export class NaoEncontradoComponent {}
