import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import * as authService from '../services/authService';
import { lerToken, lerUsuario, salvarSessao, limparSessao } from '../utils/armazenamento';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(lerToken);
  const [usuario, setUsuario] = useState(lerUsuario);

  const aplicarSessao = ({ token, usuario }) => {
    salvarSessao(token, usuario);
    setToken(token);
    setUsuario(usuario);
    return usuario;
  };

  const login = async (email, senha) => aplicarSessao(await authService.login(email, senha));
  const cadastrar = async (dados) => aplicarSessao(await authService.cadastrar(dados));

  const logout = useCallback(() => {
    limparSessao();
    setToken(null);
    setUsuario(null);
  }, []);

  // O http.js dispara esse evento quando a API devolve 401
  useEffect(() => {
    window.addEventListener('auth:expirada', logout);
    return () => window.removeEventListener('auth:expirada', logout);
  }, [logout]);

  const valor = useMemo(
    () => ({
      token,
      usuario,
      ehAdmin: usuario?.tipo === 'Admin',
      ehPassageiro: usuario?.tipo === 'Passageiro',
      login,
      cadastrar,
      logout,
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [token, usuario, logout],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export const useAuth = () => useContext(AuthContext);
