const moedaFmt = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

export const moeda = (valor) => moedaFmt.format(Number(valor) || 0);

// Datas "YYYY-MM-DD" vindas da API: força UTC para não voltar um dia por causa do fuso
export function data(valor) {
  if (!valor) return '—';
  const d = new Date(valor.length === 10 ? `${valor}T00:00:00Z` : valor);
  return d.toLocaleDateString('pt-BR', { timeZone: valor.length === 10 ? 'UTC' : undefined });
}

export const NOMES_METODO = {
  pix: 'Pix',
  cartao: 'Cartão de crédito',
  boleto: 'Boleto',
};
