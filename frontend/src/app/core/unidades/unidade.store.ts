import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { CriarUnidadeRequest, UnidadeResponse } from './unidade.models';

const UNIDADE_ATIVA_KEY = 'pf_unidade_ativa';

@Injectable({ providedIn: 'root' })
export class UnidadeStore {
  private readonly http = inject(HttpClient);

  // Signals de Estado
  private readonly _unidades = signal<UnidadeResponse[]>([]);
  private readonly _unidadeAtivaId = signal<string | null>(localStorage.getItem(UNIDADE_ATIVA_KEY));
  private readonly _carregando = signal<boolean>(false);
  private readonly _erro = signal<string | null>(null);

  // Signals Públicos Somente-Leitura
  readonly unidades = this._unidades.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly erro = this._erro.asReadonly();

  // Computed Signal: Encontra o objeto completo da unidade ativa selecionada
  readonly unidadeAtiva = computed(() => {
    const id = this._unidadeAtivaId();
    if (!id) return null;
    return this._unidades().find(u => u.id === id) || null;
  });

  listar(): Observable<UnidadeResponse[]> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.get<UnidadeResponse[]>('/api/unidades').pipe(
      tap((lista) => {
        this._unidades.set(lista);
        this._carregando.set(false);
        this.verificarSelecaoAutomatica(lista);
      }),
      catchError(err => this.tratarErro(err))
    );
  }

  criar(payload: CriarUnidadeRequest): Observable<UnidadeResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<UnidadeResponse>('/api/unidades', payload).pipe(
      tap((novaUnidade) => {
        // Atualiza a lista na memória e seleciona a nova unidade automaticamente
        this._unidades.update(lista => [...lista, novaUnidade]);
        this.selecionarUnidade(novaUnidade.id);
        this._carregando.set(false);
      }),
      catchError(err => this.tratarErro(err))
    );
  }

  selecionarUnidade(id: string): void {
    this._unidadeAtivaId.set(id);
    localStorage.setItem(UNIDADE_ATIVA_KEY, id);
  }

  limparErro(): void {
    this._erro.set(null);
  }

  private verificarSelecaoAutomatica(lista: UnidadeResponse[]): void {
    const idAtivoAtual = this._unidadeAtivaId();
    // Se não tem nenhuma ativa no localstorage, ou a que tem não existe mais, pega a primeira
    if (lista.length > 0 && (!idAtivoAtual || !lista.some(u => u.id === idAtivoAtual))) {
      this.selecionarUnidade(lista[0].id);
    }
  }

  private tratarErro(err: HttpErrorResponse): Observable<never> {
    this._carregando.set(false);
    // RFC 7807 (ProblemDetails)
    const problem = err.error;
    const mensagem = problem?.detail || 'Erro ao processar a requisição.';
    this._erro.set(mensagem);
    return throwError(() => err);
  }
}
