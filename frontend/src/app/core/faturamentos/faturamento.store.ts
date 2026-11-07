import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, tap, throwError } from 'rxjs';
import {
  AbrirCaixaRequest,
  AdicionarParcialRequest,
  FaturamentoDiarioResponse,
  FecharCaixaRequest
} from './faturamento.models';

@Injectable({ providedIn: 'root' })
export class FaturamentoStore {
  private readonly http = inject(HttpClient);

  // Signals de Estado
  private readonly _faturamento = signal<FaturamentoDiarioResponse | null>(null);
  private readonly _carregando = signal<boolean>(false);
  private readonly _erro = signal<string | null>(null);

  // Signals Públicos Somente-Leitura
  readonly faturamento = this._faturamento.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly erro = this._erro.asReadonly();

  carregarPorData(unidadeId: string, data: string): Observable<FaturamentoDiarioResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.get<FaturamentoDiarioResponse>(`/api/unidades/${unidadeId}/faturamentos/${data}`).pipe(
      tap(res => {
        this._faturamento.set(res);
        this._carregando.set(false);
      }),
      catchError((err: HttpErrorResponse) => {
        // Se for 404, apenas não há caixa aberto hoje (não é um erro a ser exibido pro usuário)
        const isNotFound = err.status === 404;
        return this.tratarErro(err, isNotFound);
      })
    );
  }

  abrirCaixa(unidadeId: string, payload: AbrirCaixaRequest): Observable<FaturamentoDiarioResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<FaturamentoDiarioResponse>(`/api/unidades/${unidadeId}/faturamentos/abrir`, payload).pipe(
      tap(res => {
        this._faturamento.set(res);
        this._carregando.set(false);
      }),
      catchError(err => this.tratarErro(err))
    );
  }

  adicionarParcial(unidadeId: string, data: string, payload: AdicionarParcialRequest): Observable<FaturamentoDiarioResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<FaturamentoDiarioResponse>(`/api/unidades/${unidadeId}/faturamentos/${data}/parciais`, payload).pipe(
      tap(res => {
        this._faturamento.set(res);
        this._carregando.set(false);
      }),
      catchError(err => this.tratarErro(err))
    );
  }

  fecharCaixa(unidadeId: string, data: string, payload: FecharCaixaRequest): Observable<FaturamentoDiarioResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<FaturamentoDiarioResponse>(`/api/unidades/${unidadeId}/faturamentos/${data}/fechar`, payload).pipe(
      tap(res => {
        this._faturamento.set(res);
        this._carregando.set(false);
      }),
      catchError(err => this.tratarErro(err))
    );
  }

  limparFaturamento(): void {
    this._faturamento.set(null);
    this._erro.set(null);
  }

  private tratarErro(err: HttpErrorResponse, silencioso: boolean = false): Observable<never> {
    this._carregando.set(false);

    if (silencioso) {
      this._faturamento.set(null);
      return throwError(() => err);
    }

    const problem = err.error;
    const mensagem = problem?.detail || 'Erro ao processar a requisição.';
    this._erro.set(mensagem);
    return throwError(() => err);
  }
}
