export interface RegistrarUsuarioRequest {
  nome: string;
  email: string;
  senha: string;
}

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface UsuarioResponse {
  id: string;
  nome: string;
  email: string;
  criadoEm: string;
}

export interface AuthResponse {
  token: string;
  expiraEmSegundos: number;
  usuario: UsuarioResponse;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
}
