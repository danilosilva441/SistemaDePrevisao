import { Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { UnidadeStore } from '../../core/unidades/unidade.store';
import { FaturamentoStore } from '../../core/faturamentos/faturamento.store';

@Component({
  selector: 'app-painel-caixa',
  standalone: true,
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe, UpperCasePipe],
  template: `
    <div class="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden mt-6">
      <!-- Header do Caixa -->
      <div class="bg-slate-900 px-6 py-4 flex items-center justify-between">
        <div>
          <h2 class="text-lg font-bold text-white">Operação do Caixa</h2>
          <p class="text-slate-400 text-sm">
            Unidade: {{ unidadeStore.unidadeAtiva()?.nome || 'Nenhuma selecionada' }}
          </p>
        </div>
        <div class="flex items-center gap-3">
          <label class="text-slate-300 text-sm font-medium">Data:</label>
          <input
            type="date"
            [value]="dataSelecionada()"
            (change)="mudarData($event)"
            class="px-3 py-1.5 rounded-lg border-0 bg-slate-800 text-white text-sm focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div class="p-6">
        @if (unidadeStore.unidadeAtiva(); as unidade) {
          @if (faturamentoStore.carregando() && !faturamentoStore.faturamento()) {
            <div class="text-center py-8 text-slate-500">Carregando informações do caixa...</div>
          } @else {

            <!-- CAIXA NÃO ABERTO -->
            @if (!faturamentoStore.faturamento()) {
              <div class="max-w-md mx-auto py-8">
                <div class="text-center mb-6">
                  <div class="w-12 h-12 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-3">
                    <span class="text-2xl">📦</span>
                  </div>
                  <h3 class="text-lg font-bold text-slate-900">Caixa Fechado</h3>
                  <p class="text-sm text-slate-500">Abra o caixa para iniciar os lançamentos do dia.</p>
                </div>

                <form [formGroup]="formAbrir" (ngSubmit)="abrirCaixa()" class="space-y-4">
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-1">Fundo de Caixa (Troco) *</label>
                    <input type="number" formControlName="fundoDeCaixa" class="w-full px-3 py-2 rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-600 focus:outline-none" />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-slate-700 mb-1">Observações Iniciais</label>
                    <input type="text" formControlName="observacoes" class="w-full px-3 py-2 rounded-lg border border-slate-300 focus:ring-2 focus:ring-blue-600 focus:outline-none" />
                  </div>
                  <button type="submit" [disabled]="formAbrir.invalid || faturamentoStore.carregando()" class="w-full py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50">
                    Abrir Caixa do Dia
                  </button>
                </form>
              </div>
            }

            <!-- CAIXA ABERTO OU FECHADO (Tem dados) -->
            @if (faturamentoStore.faturamento(); as caixa) {
              <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <!-- Coluna Esquerda: Resumo -->
                <div class="space-y-4">
                  <div class="bg-slate-50 rounded-xl p-4 border border-slate-200">
                    <div class="flex justify-between items-center mb-4">
                      <span class="text-sm font-medium text-slate-500">Status</span>
                      <span class="px-2.5 py-1 text-xs font-bold rounded-full"
                        [class.bg-emerald-100]="caixa.statusCaixa === 'Aberto'"
                        [class.text-emerald-700]="caixa.statusCaixa === 'Aberto'"
                        [class.bg-slate-200]="caixa.statusCaixa !== 'Aberto'"
                        [class.text-slate-700]="caixa.statusCaixa !== 'Aberto'">
                        {{ caixa.statusCaixa | uppercase }}
                      </span>
                    </div>
                    <div class="space-y-2 text-sm">
                      <div class="flex justify-between">
                        <span class="text-slate-500">Fundo de Caixa:</span>
                        <span class="font-medium">{{ caixa.fundoDeCaixa | currency:'BRL' }}</span>
                      </div>
                      <div class="flex justify-between">
                        <span class="text-slate-500">Total Lançamentos:</span>
                        <span class="font-bold text-slate-900">{{ caixa.totalParciais | currency:'BRL' }}</span>
                      </div>
                      @if (caixa.statusCaixa !== 'Aberto') {
                        <div class="pt-2 mt-2 border-t border-slate-200 flex justify-between">
                          <span class="text-slate-500">Valor Conferido:</span>
                          <span class="font-bold text-slate-900">{{ caixa.valorConferido | currency:'BRL' }}</span>
                        </div>
                        <div class="flex justify-between">
                          <span class="text-slate-500">Diferença:</span>
                          <span class="font-bold" [class.text-rose-600]="caixa.diferenca < 0" [class.text-emerald-600]="caixa.diferenca >= 0">
                            {{ caixa.diferenca | currency:'BRL' }}
                          </span>
                        </div>
                      }
                    </div>
                  </div>

                  <!-- Formulário de Fechamento (Só aparece se estiver Aberto) -->
                  @if (caixa.statusCaixa === 'Aberto') {
                    <div class="bg-amber-50 rounded-xl p-4 border border-amber-200">
                      <h4 class="font-bold text-amber-900 mb-3 text-sm">Encerrar Expediente</h4>
                      <form [formGroup]="formFechar" (ngSubmit)="fecharCaixa()">
                        <div class="mb-3">
                          <label class="block text-xs font-medium text-amber-800 mb-1">Valor em Caixa (Total + Troco) *</label>
                          <input type="number" formControlName="valorConferido" class="w-full px-3 py-1.5 text-sm rounded-lg border border-amber-300 bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none" />
                        </div>
                        <button type="submit" [disabled]="formFechar.invalid || faturamentoStore.carregando()" class="w-full py-1.5 bg-amber-600 text-white text-sm font-medium rounded-lg hover:bg-amber-700 disabled:opacity-50">
                          Fechar Caixa
                        </button>
                      </form>
                    </div>
                  }
                </div>

                <!-- Coluna Direita: Lançamentos Parciais -->
                <div class="lg:col-span-2">

                  @if (caixa.statusCaixa === 'Aberto') {
                    <form [formGroup]="formParcial" (ngSubmit)="adicionarParcial()" class="bg-blue-50 rounded-xl p-4 border border-blue-100 mb-6 flex flex-wrap gap-3 items-end">
                      <div class="flex-1 min-w-[120px]">
                        <label class="block text-xs font-medium text-blue-900 mb-1">Valor *</label>
                        <input type="number" formControlName="valor" class="w-full px-3 py-1.5 text-sm rounded-lg border border-blue-200 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                      </div>
                      <div class="w-24">
                        <label class="block text-xs font-medium text-blue-900 mb-1">Início *</label>
                        <input type="time" formControlName="horaInicio" class="w-full px-2 py-1.5 text-sm rounded-lg border border-blue-200 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                      </div>
                      <div class="w-24">
                        <label class="block text-xs font-medium text-blue-900 mb-1">Fim *</label>
                        <input type="time" formControlName="horaFim" class="w-full px-2 py-1.5 text-sm rounded-lg border border-blue-200 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                      </div>
                      <div class="flex-1 min-w-[120px]">
                        <label class="block text-xs font-medium text-blue-900 mb-1">Origem / Turno *</label>
                        <input type="text" formControlName="origem" placeholder="Ex: Turno Manhã" class="w-full px-3 py-1.5 text-sm rounded-lg border border-blue-200 focus:ring-2 focus:ring-blue-500 focus:outline-none" />
                      </div>
                      <button type="submit" [disabled]="formParcial.invalid || faturamentoStore.carregando()" class="px-4 py-1.5 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 disabled:opacity-50">
                        Adicionar
                      </button>
                    </form>
                  }

                  <h3 class="font-bold text-slate-900 mb-3">Lançamentos Registrados ({{ caixa.parciais.length }})</h3>

                  @if (caixa.parciais.length === 0) {
                    <p class="text-sm text-slate-500 italic">Nenhum lançamento registrado neste dia.</p>
                  } @else {
                    <div class="bg-white border border-slate-200 rounded-lg overflow-hidden">
                      <table class="w-full text-left text-sm">
                        <thead class="bg-slate-50 border-b border-slate-200 text-slate-600">
                          <tr>
                            <th class="px-4 py-2 font-medium">Período</th>
                            <th class="px-4 py-2 font-medium">Origem</th>
                            <th class="px-4 py-2 font-medium text-right">Valor</th>
                          </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-100">
                          @for (p of caixa.parciais; track p.id) {
                            <tr class="hover:bg-slate-50 transition-colors">
                              <td class="px-4 py-2">{{ p.horaInicio | date:'HH:mm' }} às {{ p.horaFim | date:'HH:mm' }}</td>
                              <td class="px-4 py-2">{{ p.origem }}</td>
                              <td class="px-4 py-2 font-medium text-right text-slate-900">{{ p.valor | currency:'BRL' }}</td>
                            </tr>
                          }
                        </tbody>
                      </table>
                    </div>
                  }
                </div>
              </div>
            }
          }
        } @else {
          <div class="text-center py-12 text-slate-500">
            <span class="text-3xl block mb-2">🏢</span>
            Selecione uma unidade no cabeçalho para gerenciar o caixa.
          </div>
        }
      </div>
    </div>
  `
})
export class PainelCaixaComponent {
  readonly unidadeStore = inject(UnidadeStore);
  readonly faturamentoStore = inject(FaturamentoStore);
  private readonly fb = inject(FormBuilder);

  // Inicializa a data atual no fuso local no formato yyyy-MM-dd
  readonly dataSelecionada = signal<string>(new Date().toLocaleDateString('en-CA'));

  // Formulários
  readonly formAbrir = this.fb.nonNullable.group({
    fundoDeCaixa: [0, [Validators.required, Validators.min(0)]],
    observacoes: ['']
  });

  readonly formParcial = this.fb.nonNullable.group({
    valor: [0, [Validators.required, Validators.min(0.01)]],
    horaInicio: ['', Validators.required],
    horaFim: ['', Validators.required],
    origem: ['', Validators.required]
  });

  readonly formFechar = this.fb.nonNullable.group({
    valorConferido: [0, [Validators.required, Validators.min(0)]]
  });

  constructor() {
    // Efeito Reativo: Busca os dados automaticamente sempre que a Unidade ou a Data mudarem
    effect(() => {
      const unidadeId = this.unidadeStore.unidadeAtiva()?.id;
      const data = this.dataSelecionada();

      if (unidadeId && data) {
        this.faturamentoStore.carregarPorData(unidadeId, data).subscribe();
      } else {
        this.faturamentoStore.limparFaturamento();
      }
    });
  }

  mudarData(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.value) {
      this.dataSelecionada.set(input.value);
    }
  }

  abrirCaixa(): void {
    if (this.formAbrir.invalid) return;
    const unidadeId = this.unidadeStore.unidadeAtiva()?.id;
    if (!unidadeId) return;

    const payload = {
      data: this.dataSelecionada(),
      ...this.formAbrir.getRawValue()
    };

    this.faturamentoStore.abrirCaixa(unidadeId, payload).subscribe({
      next: () => this.formAbrir.reset({ fundoDeCaixa: 0 })
    });
  }

  adicionarParcial(): void {
    if (this.formParcial.invalid) return;
    const unidadeId = this.unidadeStore.unidadeAtiva()?.id;
    if (!unidadeId) return;

    const raw = this.formParcial.getRawValue();
    // Monta o DateTime ISO combinando a data selecionada com a hora digitada
    const payload = {
      valor: raw.valor,
      horaInicio: new Date(`${this.dataSelecionada()}T${raw.horaInicio}:00`).toISOString(),
      horaFim: new Date(`${this.dataSelecionada()}T${raw.horaFim}:00`).toISOString(),
      origem: raw.origem
    };

    this.faturamentoStore.adicionarParcial(unidadeId, this.dataSelecionada(), payload).subscribe({
      next: () => this.formParcial.reset({ valor: 0, horaInicio: '', horaFim: '', origem: '' })
    });
  }

  fecharCaixa(): void {
    if (this.formFechar.invalid) return;
    const unidadeId = this.unidadeStore.unidadeAtiva()?.id;
    if (!unidadeId) return;

    this.faturamentoStore.fecharCaixa(unidadeId, this.dataSelecionada(), this.formFechar.getRawValue()).subscribe({
      next: () => this.formFechar.reset({ valorConferido: 0 })
    });
  }
}
