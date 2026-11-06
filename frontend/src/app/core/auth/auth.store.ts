import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, tap, throwError } from 'rxjs';
import {
  AuthResponse,
  LoginRequest,
  ProblemDetails,
  RegistrarUsuarioRequest,
  UsuarioResponse
} from './auth.models';

const TOKEN_KEY = 'pf_token';
const USUARIO_KEY = 'pf_usuario';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // Signals privados de estado
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _usuario = signal<UsuarioResponse | null>(this.lerUsuarioStorage());
  private readonly _carregando = signal<boolean>(false);
  private readonly _erro = signal<string | null>(null);

  // Signals públicos somente leitura
  readonly token = this._token.asReadonly();
  readonly usuario = this._usuario.asReadonly();
  readonly carregando = this._carregando.asReadonly();
  readonly erro = this._erro.asReadonly();
  readonly estaAutenticado = computed(() => !!this._token());

  login(payload: LoginRequest): Observable<AuthResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<AuthResponse>('/api/auth/login', payload).pipe(
      tap((res) => this.definirSessao(res)),
      catchError((err: HttpErrorResponse) => this.tratarErro(err))
    );
  }

  registrar(payload: RegistrarUsuarioRequest): Observable<AuthResponse> {
    this._carregando.set(true);
    this._erro.set(null);

    return this.http.post<AuthResponse>('/api/auth/registro', payload).pipe(
      tap((res) => this.definirSessao(res)),
      catchError((err: HttpErrorResponse) => this.tratarErro(err))
    );
  }

  carregarPerfil(): Observable<UsuarioResponse> {
    return this.http.get<UsuarioResponse>('/api/auth/me').pipe(
      tap((usuario) => {
        this._usuario.set(usuario);
        localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario));
      }),
      catchError((err: HttpErrorResponse) => {
        this.sair();
        return throwError(() => err);
      })
    );
  }

  sair(): void {
    this._token.set(null);
    this._usuario.set(null);
    this._erro.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    this.router.navigate(['/login']);
  }

  limparErro(): void {
    this._erro.set(null);
  }

  private definirSessao(res: AuthResponse): void {
    this._token.set(res.token);
    this._usuario.set(res.usuario);
    this._carregando.set(false);
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USUARIO_KEY, JSON.stringify(res.usuario));
  }

  private tratarErro(err: HttpErrorResponse): Observable<never> {
    this._carregando.set(false);
    const problem = err.error as ProblemDetails | undefined;
    const mensagem = problem?.detail || 'Não foi possível concluir a operação. Tente novamente.';
    this._erro.set(mensagem);
    return throwError(() => err);
  }

  private lerUsuarioStorage(): UsuarioResponse | null {
    const raw = localStorage.getItem(USUARIO_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UsuarioResponse;
    } catch {
      return null;
    }
  }
}
