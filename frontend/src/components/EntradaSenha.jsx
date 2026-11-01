// Campo de senha com botão de olho para mostrar/ocultar o que foi digitado.
// Uso igual ao <Entrada>: <EntradaSenha value={...} onChange={...} autoComplete="current-password" />
import { useState } from 'react';
import styled from 'styled-components';
import { Entrada } from './ui.js';

const Caixa = styled.div`
  position: relative;

  ${Entrada} { padding-right: 46px; }
`;

const BotaoOlho = styled.button`
  position: absolute;
  top: 0;
  right: 0;
  height: 100%;
  width: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 0;
  background: transparent;
  color: ${({ theme }) => theme.cores.textoSuave};
  cursor: pointer;
  border-radius: 0 10px 10px 0;

  &:hover { color: ${({ theme }) => theme.cores.texto}; }
  &:focus-visible {
    outline: 2px solid ${({ theme }) => theme.cores.primaria};
    outline-offset: -2px;
  }
`;

const Olho = ({ aberto }) => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
    strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
    <circle cx="12" cy="12" r="3" />
    {!aberto && <line x1="3" y1="3" x2="21" y2="21" />}
  </svg>
);

export default function EntradaSenha(props) {
  const [visivel, setVisivel] = useState(false);

  return (
    <Caixa>
      <Entrada {...props} type={visivel ? 'text' : 'password'} />
      <BotaoOlho
        type="button"
        onClick={() => setVisivel(!visivel)}
        aria-label={visivel ? 'Ocultar senha' : 'Mostrar senha'}
        aria-pressed={visivel}
        title={visivel ? 'Ocultar senha' : 'Mostrar senha'}
      >
        <Olho aberto={!visivel} />
      </BotaoOlho>
    </Caixa>
  );
}
