import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-50 px-4">
      <div class="max-w-md w-full bg-white rounded-xl shadow-sm border border-slate-200 p-8">
        <div class="mb-6 text-center">
          <h1 class="text-2xl font-bold text-slate-900">Cadastro de Gestor</h1>
          <p class="text-sm text-slate-500 mt-1">Crie sua conta para gerenciar suas Unidades</p>
        </div>

        @if (authStore.erro()) {
          <div class="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-sm">
            {{ authStore.erro() }}
          </div>
        }

        <form [formGroup]="form" (ngSubmit)="registrar()" class="space-y-4">
          <div>
            <label for="nome" class="block text-sm font-medium text-slate-700 mb-1">Nome completo</label>
            <input
              id="nome"
              type="text"
              formControlName="nome"
              placeholder="Seu nome"
              class="w-full px-3.5 py-2 rounded-lg border border-slate-300 text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-600"
            />
            @if (form.controls.nome.touched && form.controls.nome.invalid) {
              <span class="text-xs text-rose-600 mt-1 block">O nome deve ter no mínimo 3 caracteres.</span>
            }
          </div>

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
              placeholder="Mínimo 6 caracteres"
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
            {{ authStore.carregando() ? 'Criando conta...' : 'Registrar e Entrar' }}
          </button>
        </form>

        <p class="mt-6 text-center text-sm text-slate-600">
          Já possui uma conta?
          <a routerLink="/login" class="text-blue-600 font-medium hover:underline">Fazer login</a>
        </p>
      </div>
    </div>
  `
})
export class RegistroComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.authStore.limparErro();
  }

  registrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.authStore.registrar(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/dashboard'])
    });
  }
}
