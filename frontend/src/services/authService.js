import { USE_MOCK } from '../config';
import { http } from './http';
import * as mock from '../mocks/mockApi';

// Resposta esperada: { token, usuario: { id, nome, email, tipo: "Admin" | "Passageiro" } }

export const login = (email, senha) =>
  USE_MOCK
    ? mock.login(email, senha)
    : http('/auth/login', { method: 'POST', body: { email, senha }, auth: false });

// Cadastro público sempre cria Passageiro; Admin é criado pelo seed do back.
export const cadastrar = ({ nome, email, senha }) =>
  USE_MOCK
    ? mock.cadastrar({ nome, email, senha })
    : http('/auth/cadastro', { method: 'POST', body: { nome, email, senha }, auth: false });
