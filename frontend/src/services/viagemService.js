import { USE_MOCK } from '../config';
import { http } from './http';
import * as mock from '../mocks/mockApi';

/**
 * Monta o multipart/form-data que o ASP.NET recebe com [FromForm] ViagemFormDto.
 * - campos simples: titulo, destino, descricao, dataSaida, dataRetorno, valor, vagas, maxParcelas
 * - imagens: vários arquivos com a mesma chave (List<IFormFile> Imagens)
 * - imagensRemover: ids das imagens já salvas que o admin tirou (List<int>)
 */
function montarForm(dados, novasImagens = [], imagensRemover = []) {
  const form = new FormData();
  Object.entries(dados).forEach(([chave, valor]) => form.append(chave, valor ?? ''));
  novasImagens.forEach((arquivo) => form.append('imagens', arquivo));
  imagensRemover.forEach((id) => form.append('imagensRemover', id));
  return form;
}

export const listar = () => (USE_MOCK ? mock.listarViagens() : http('/viagens', { auth: false }));

export const buscar = (id) => (USE_MOCK ? mock.buscarViagem(id) : http(`/viagens/${id}`, { auth: false }));

export const criar = (dados, novasImagens) =>
  USE_MOCK
    ? mock.criarViagem(dados, novasImagens)
    : http('/viagens', { method: 'POST', body: montarForm(dados, novasImagens) });

export const atualizar = (id, dados, novasImagens, imagensRemover) =>
  USE_MOCK
    ? mock.atualizarViagem(id, dados, novasImagens, imagensRemover)
    : http(`/viagens/${id}`, { method: 'PUT', body: montarForm(dados, novasImagens, imagensRemover) });

export const excluir = (id) =>
  USE_MOCK ? mock.excluirViagem(id) : http(`/viagens/${id}`, { method: 'DELETE' });

// Só Admin
export const passageiros = (id) =>
  USE_MOCK ? mock.passageirosDaViagem(id) : http(`/viagens/${id}/passageiros`);
