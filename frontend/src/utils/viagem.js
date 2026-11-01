// Regra de "à venda": tem vaga e ainda não saiu.
// No back, o ideal é ter um filtro igual: GET /viagens?disponiveis=true
export function estaAVenda(viagem, hoje = new Date().toISOString().slice(0, 10)) {
  return viagem.vagasDisponiveis > 0 && (!viagem.dataSaida || viagem.dataSaida.slice(0, 10) >= hoje);
}
