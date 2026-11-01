import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import styled from 'styled-components';
import { Alerta, Botao, Campo, Entrada, Formulario } from '../components/ui.js';
import { CartaoAuth, Centro, Rodape } from '../components/AuthLayout.js';
import EntradaSenha from '../components/EntradaSenha.jsx';

const Termos = styled.p`
  margin: 0 0 12px;
  font-size: 13px;
  color: ${({ theme }) => theme.cores.textoSuave};
  a { color: ${({ theme }) => theme.cores.primaria}; }
`;

const Explicacao = styled.p`
  margin: 0 0 16px;
  color: ${({ theme }) => theme.cores.textoSuave};
`;

export default function Cadastro() {
  const { cadastrar } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ nome: '', email: '', senha: '', confirmar: '' });
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);

  const mudar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const enviar = async (e) => {
    e.preventDefault();
    if (form.senha.length < 8 || form.senha.length > 72) return setErro('A senha precisa ter entre 8 e 72 caracteres.');
    if (form.senha !== form.confirmar) return setErro('As senhas não conferem.');
    setErro('');
    setEnviando(true);
    try {
      await cadastrar({ nome: form.nome, email: form.email, senha: form.senha });
      navigate('/', { replace: true });
    } catch (err) {
      setErro(err.message);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Centro>
      <CartaoAuth as={Formulario} onSubmit={enviar}>
        <h1>Criar conta</h1>
        <Explicacao>Contas novas entram como passageiro.</Explicacao>

        <Campo>
          <span>Nome</span>
          <Entrada name="nome" value={form.nome} onChange={mudar} required />
        </Campo>
        <Campo>
          <span>E-mail</span>
          <Entrada name="email" type="email" value={form.email} onChange={mudar} required autoComplete="email" />
        </Campo>
        <Campo>
          <span>Senha</span>
          <EntradaSenha name="senha" value={form.senha} onChange={mudar} required autoComplete="new-password" />
        </Campo>
        <Campo>
          <span>Confirmar senha</span>
          <EntradaSenha
            name="confirmar"
            value={form.confirmar}
            onChange={mudar}
            required
            autoComplete="new-password"
          />
        </Campo>

        {erro && <Alerta $tipo="erro">{erro}</Alerta>}

        <Termos>
          Ao criar a conta, você declara que leu a <Link to="/privacidade">Política de Privacidade</Link> e a{' '}
          <Link to="/cookies">Política de Cookies</Link>.
        </Termos>

        <Botao type="submit" $variante="primario" $bloco disabled={enviando}>
          {enviando ? 'Criando…' : 'Criar conta'}
        </Botao>
        <Rodape>
          Já tem conta? <Link to="/login">Entrar</Link>
        </Rodape>
      </CartaoAuth>
    </Centro>
  );
}
