import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-50 px-4">
      <div class="max-w-md w-full bg-white rounded-xl shadow-sm border border-slate-200 p-8">
        <div class="mb-6 text-center">
          <h1 class="text-2xl font-bold text-slate-900">Previsão de Faturamento</h1>
          <p class="text-sm text-slate-500 mt-1">Acesso exclusivo para o Gestor</p>
        </div>

        @if (authStore.erro()) {
          <div class="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-sm">
            {{ authStore.erro() }}
          </div>
        }

        <form [formGroup]="form" (ngSubmit)="entrar()" class="space-y-4">
          <div>
            <label for="email" class="block text-sm font-medium text-slate-700 mb-1">E-mail</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="gestor@empresa.com"
              class="w-full px-3.5 py-2 rounded-lg border border-slate-300 text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-600"
            />
            @if (form.controls.email.touched && form.controls.email.invalid) {
              <span class="text-xs text-rose-600 mt-1 block">Informe um e-mail válido.</span>
            }
          </div>

          <div>
            <label for="senha" class="block text-sm font-medium text-slate-700 mb-1">Senha</label>
            <input
              id="senha"
              type="password"
              formControlName="senha"
              placeholder="••••••"
              class="w-full px-3.5 py-2 rounded-lg border border-slate-300 text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-600"
            />
            @if (form.controls.senha.touched && form.controls.senha.invalid) {
              <span class="text-xs text-rose-600 mt-1 block">A senha deve ter no mínimo 6 caracteres.</span>
            }
          </div>

          <button
            type="submit"
            [disabled]="form.invalid || authStore.carregando()"
            class="w-full py-2.5 px-4 rounded-lg bg-blue-600 text-white font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {{ authStore.carregando() ? 'Autenticando...' : 'Entrar' }}
          </button>
        </form>

        <p class="mt-6 text-center text-sm text-slate-600">
          Ainda não possui conta?
          <a routerLink="/registro" class="text-blue-600 font-medium hover:underline">Criar conta de Gestor</a>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.authStore.limparErro();
  }

  entrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.authStore.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/dashboard'])
    });
  }
}
