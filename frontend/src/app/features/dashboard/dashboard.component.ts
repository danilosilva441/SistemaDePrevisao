import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { UnidadeStore } from '../../core/unidades/unidade.store';
import { CurrencyPipe, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, DecimalPipe],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <h2 class="text-2xl font-bold text-slate-900">Minhas Unidades</h2>
        <button 
          (click)="mostrarFormulario.set(!mostrarFormulario())"
          class="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors">
          {{ mostrarFormulario() ? 'Cancelar' : '+ Nova Unidade' }}
        </button>
      </div>

      <!-- Formulário Inline de Criação -->
      @if (mostrarFormulario()) {
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <h3 class="text-lg font-semibold text-slate-900 mb-4">Cadastrar Nova Unidade</h3>
          
          @if (unidadeStore.erro()) {
            <div class="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-sm">
              {{ unidadeStore.erro() }}
            </div>
          }

          <form [formGroup]="form" (ngSubmit)="salvar()" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Nome da Unidade *</label>
              <input type="text" formControlName="nome" class="w-full px-3 py-2 rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-600 focus:outline-none" placeholder="Ex: Estacionamento Centro" />
            </div>
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Meta Mensal (R$) *</label>
              <input type="number" formControlName="metaMensal" class="w-full px-3 py-2 rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-600 focus:outline-none" placeholder="50000" />
            </div>
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-slate-700 mb-1">Endereço</label>
              <input type="text" formControlName="endereco" class="w-full px-3 py-2 rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-600 focus:outline-none" placeholder="Rua exemplo, 123" />
            </div>
            
            <div class="md:col-span-2 flex justify-end mt-2">
              <button 
                type="submit" 
                [disabled]="form.invalid || unidadeStore.carregando()"
                class="px-6 py-2 bg-slate-900 text-white text-sm font-medium rounded-lg hover:bg-slate-800 disabled:opacity-50 transition-colors">
                {{ unidadeStore.carregando() ? 'Salvando...' : 'Salvar Unidade' }}
              </button>
            </div>
          </form>
        </div>
      }

      <!-- Grid de Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @for (u of unidadeStore.unidades(); track u.id) {
          <div class="bg-white rounded-xl border border-slate-200 shadow-sm p-5 hover:shadow-md transition-shadow relative overflow-hidden" 
               [class.border-blue-500]="u.id === unidadeStore.unidadeAtiva()?.id">
            
            @if (u.id === unidadeStore.unidadeAtiva()?.id) {
              <div class="absolute top-0 right-0 bg-blue-500 text-white text-[10px] font-bold px-2 py-1 rounded-bl-lg">
                SELECIONADA
              </div>
            }

            <h3 class="text-lg font-bold text-slate-900 truncate pr-12">{{ u.nome }}</h3>
            <p class="text-sm text-slate-500 truncate mb-4">{{ u.endereco || 'Sem endereço cadastrado' }}</p>

            <div class="space-y-3">
              <div>
                <div class="flex justify-between text-sm mb-1">
                  <span class="text-slate-600">Atingimento da Meta</span>
                  <span class="font-medium text-slate-900">{{ u.percentualAtingido | number:'1.0-1' }}%</span>
                </div>
                <div class="w-full bg-slate-100 rounded-full h-2">
                  <div class="bg-blue-600 h-2 rounded-full transition-all duration-500" 
                       [style.width.%]="u.percentualAtingido > 100 ? 100 : u.percentualAtingido"></div>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4 pt-4 border-t border-slate-100">
                <div>
                  <p class="text-xs text-slate-500">Acumulado Mês</p>
                  <p class="font-bold text-slate-900">{{ u.faturamentoAcumuladoMes | currency:'BRL':'symbol' }}</p>
                </div>
                <div>
                  <p class="text-xs text-slate-500">Meta Mensal</p>
                  <p class="font-bold text-slate-900">{{ u.metaMensal | currency:'BRL':'symbol' }}</p>
                </div>
              </div>
            </div>
          </div>
        }
        
        @if (unidadeStore.unidades().length === 0 && !unidadeStore.carregando()) {
          <div class="col-span-full text-center py-12 bg-white rounded-xl border border-slate-200 border-dashed">
            <p class="text-slate-500">Nenhuma unidade cadastrada. Comece clicando no botão acima.</p>
          </div>
        }
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  readonly unidadeStore = inject(UnidadeStore);
  private readonly fb = inject(FormBuilder);

  readonly mostrarFormulario = signal(false);

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.minLength(2)]],
    metaMensal: [0, [Validators.required, Validators.min(0)]],
    endereco: ['']
  });

  ngOnInit(): void {
    this.unidadeStore.limparErro();
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.unidadeStore.criar(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset();
        this.mostrarFormulario.set(false);
      }
    });
  }
}