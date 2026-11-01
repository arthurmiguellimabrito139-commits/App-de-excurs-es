import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { USE_MOCK } from '../config';
import { Acoes, Alerta, Botao, Campo, Entrada, Formulario, Nota } from '../components/ui.js';
import { CartaoAuth, Centro, Rodape } from '../components/AuthLayout.js';
import EntradaSenha from '../components/EntradaSenha.jsx';

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);

  const enviar = async (e) => {
    e.preventDefault();
    setErro('');
    setEnviando(true);
    try {
      const usuario = await login(email, senha);
      const destino = location.state?.de || (usuario.tipo === 'Admin' ? '/admin' : '/');
      navigate(destino, { replace: true });
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  };

  const preencher = (em) => {
    setEmail(em);
    setSenha('123456');
  };

  return (
    <Centro>
      <CartaoAuth as={Formulario} onSubmit={enviar}>
        <h1>Entrar</h1>

        <Campo>
          <span>E-mail</span>
          <Entrada type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" />
        </Campo>
        <Campo>
          <span>Senha</span>
          <EntradaSenha
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            required
            autoComplete="current-password"
          />
        </Campo>

        {erro && <Alerta $tipo="erro">{erro}</Alerta>}

        <Botao type="submit" $variante="primario" $bloco disabled={enviando}>
          {enviando ? 'Entrando…' : 'Entrar'}
        </Botao>

        <Rodape>
          Não tem conta? <Link to="/cadastro">Cadastre-se</Link>
        </Rodape>

        {USE_MOCK && (
          <Nota>
            <strong>Modo simulado</strong> — contas de teste (senha 123456):
            <Acoes>
              <Botao type="button" $tamanho="pequeno" onClick={() => preencher('admin@teste.com')}>
                Admin
              </Botao>
              <Botao type="button" $tamanho="pequeno" onClick={() => preencher('passageiro@teste.com')}>
                Passageiro
              </Botao>
            </Acoes>
          </Nota>
        )}
      </CartaoAuth>
    </Centro>
  );
}
