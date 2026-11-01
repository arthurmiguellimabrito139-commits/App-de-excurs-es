// Aviso no rodapé da tela. Como o site só usa armazenamento essencial,
// é um aviso informativo (sem "aceitar/recusar"): some depois do "Entendi".
import { useState } from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';
import { Botao } from './ui.js';
import { avisoCookiesVisto, marcarAvisoCookiesVisto } from '../utils/armazenamento';

const Caixa = styled.div`
  position: fixed;
  left: 16px;
  right: 16px;
  bottom: max(16px, env(safe-area-inset-bottom));
  z-index: 30;
  max-width: 560px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  gap: 12px 16px;
  padding: 14px 16px;
  background: ${({ theme }) => theme.cores.superficie};
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: ${({ theme }) => theme.raio};
  box-shadow: 0 8px 32px rgba(31, 42, 43, 0.18);
  font-size: 14px;

  p { margin: 0; flex: 1; }
  a { color: ${({ theme }) => theme.cores.primaria}; }

  ${({ theme }) => theme.midia.celular} {
    flex-direction: column;
    align-items: stretch;
  }
`;

export default function AvisoCookies() {
  const [visivel, setVisivel] = useState(() => !avisoCookiesVisto());
  if (!visivel) return null;

  const fechar = () => {
    marcarAvisoCookiesVisto();
    setVisivel(false);
  };

  return (
    <Caixa role="region" aria-label="Aviso de cookies">
      <p>
        Usamos só o armazenamento essencial para manter você logado — nada de rastreamento ou publicidade. Saiba mais na{' '}
        <Link to="/cookies">Política de Cookies</Link>.
      </p>
      <Botao type="button" $variante="primario" $tamanho="pequeno" onClick={fechar}>
        Entendi
      </Botao>
    </Caixa>
  );
}
