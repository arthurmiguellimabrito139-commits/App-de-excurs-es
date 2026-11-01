import { USE_MOCK } from '../config';
import { http } from './http';
import * as mock from '../mocks/mockApi';

// Lista das strategies registradas no back: [{ metodo, nome, descricao }]
export const metodos = () =>
  USE_MOCK ? mock.metodosPagamento() : http('/pagamentos/metodos', { auth: false });

// O cálculo SEMPRE vem do back (é lá que vive o Strategy); o front só exibe.
export const simular = (viagemId, metodo, numParcelas) =>
  USE_MOCK
    ? mock.simular(viagemId, metodo, numParcelas)
    : http(`/viagens/${viagemId}/simular?metodo=${metodo}&parcelas=${numParcelas}`, { auth: false });

// Só Passageiro. O valor NÃO é enviado: o back busca no banco.
export const comprar = (viagemId, metodo, numParcelas) =>
  USE_MOCK
    ? mock.comprar(viagemId, metodo, numParcelas)
    : http(`/viagens/${viagemId}/comprar`, { method: 'POST', body: { metodo, numParcelas } });

export const minhasCompras = () => (USE_MOCK ? mock.minhasCompras() : http('/compras/minhas'));

export const pagarParcela = (parcelaId) =>
  USE_MOCK ? mock.pagarParcela(parcelaId) : http(`/parcelas/${parcelaId}/pagar`, { method: 'POST' });
