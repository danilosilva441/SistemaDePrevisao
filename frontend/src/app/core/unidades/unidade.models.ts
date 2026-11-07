export interface CriarUnidadeRequest {
  nome: string;
  descricao?: string;
  endereco?: string;
  metaMensal: number;
  dataInicio?: string;
  dataFim?: string;
}

export interface UnidadeResponse {
  id: string;
  nome: string;
  descricao?: string;
  endereco?: string;
  metaMensal: number;
  projecaoFaturamento: number;
  faturamentoAcumuladoMes: number;
  percentualAtingido: number;
  dataInicio: string;
  dataFim?: string;
  isAtivo: boolean;
}