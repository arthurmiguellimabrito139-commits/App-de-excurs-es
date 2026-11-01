// localStorage pode falhar (aba anônima, bloqueio), então tudo fica em try/catch.

export function lerToken() {
  try {
    return localStorage.getItem('token');
  } catch {
    return null;
  }
}

export function lerUsuario() {
  try {
    const bruto = localStorage.getItem('usuario');
    return bruto ? JSON.parse(bruto) : null;
  } catch {
    return null;
  }
}

export function salvarSessao(token, usuario) {
  try {
    localStorage.setItem('token', token);
    localStorage.setItem('usuario', JSON.stringify(usuario));
  } catch {
    /* segue só em memória */
  }
}

export function limparSessao() {
  try {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
  } catch {
    /* nada */
  }
}

export function avisoCookiesVisto() {
  try {
    return localStorage.getItem('avisoCookies') === 'visto';
  } catch {
    return false;
  }
}

export function marcarAvisoCookiesVisto() {
  try {
    localStorage.setItem('avisoCookies', 'visto');
  } catch {
    /* mostra de novo na próxima visita */
  }
}
