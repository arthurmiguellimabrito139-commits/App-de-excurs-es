import { API_URL } from '../config';
import { lerToken } from '../utils/armazenamento';

/**
 * Wrapper do fetch:
 * - coloca o "Authorization: Bearer <token>" automaticamente
 * - envia JSON ou FormData (upload de imagens)
 * - transforma erro da API em Error com mensagem legível
 */
export async function http(caminho, { method = 'GET', body, auth = true } = {}) {
  const headers = {};
  const ehForm = body instanceof FormData;

  if (body && !ehForm) headers['Content-Type'] = 'application/json';

  const token = lerToken();
  if (auth && token) headers.Authorization = `Bearer ${token}`;

  const resposta = await fetch(`${API_URL}${caminho}`, {
    method,
    headers,
    body: body ? (ehForm ? body : JSON.stringify(body)) : undefined,
  });

  if (resposta.status === 401) {
    // Token expirado ou inválido: o AuthContext escuta e faz logout
    window.dispatchEvent(new Event('auth:expirada'));
  }

  if (!resposta.ok) {
    let mensagem = `Erro ${resposta.status}`;
    try {
      const dados = await resposta.json();
      // { erro } é o formato combinado; "title" vem do ProblemDetails do ASP.NET
      mensagem = dados.erro || dados.title || mensagem;
    } catch {
      /* resposta sem corpo */
    }
    throw new Error(mensagem);
  }

  if (resposta.status === 204) return null;
  return resposta.json();
}
