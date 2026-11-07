export interface AbrirCaixaRequest {
  data: string; // Formato yyyy-MM-dd
  fundoDeCaixa: number;
  observacoes?: string;
}

export interface AdicionarParcialRequest {
  valor: number;
  horaInicio: string; // ISO (ex: 2026-11-07T08:00:00Z)
  horaFim: string;
  origem: string;
}

export interface FecharCaixaRequest {
  valorConferido: number;
  observacoes?: string;
}

export interface FaturamentoParcialResponse {
  id: string;
  valor: number;
  horaInicio: string;
  horaFim: string;
  origem: string;
  isAtivo: boolean;
}

export interface FaturamentoDiarioResponse {
  id: string;
  unidadeId: string;
  data: string; // yyyy-MM-dd
  statusCaixa: 'Aberto' | 'Fechado' | 'Conferido';
  fundoDeCaixa: number;
  valorConferido?: number;
  totalParciais: number;
  diferenca: number;
  observacoes?: string;
  dataFechamento?: string;
  parciais: FaturamentoParcialResponse[];
}
