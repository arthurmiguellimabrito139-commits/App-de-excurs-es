import { Link } from 'react-router-dom';
import styled from 'styled-components';
import { EMPRESA } from '../config';
import { Container } from './ui.js';

const Barra = styled.footer`
  border-top: 1px solid ${({ theme }) => theme.cores.borda};
  background: ${({ theme }) => theme.cores.superficie};
  padding-bottom: env(safe-area-inset-bottom);
`;

const Interno = styled(Container)`
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px 20px;
  padding-top: 18px;
  padding-bottom: 18px;
  font-size: 14px;
  color: ${({ theme }) => theme.cores.textoSuave};

  nav { display: flex; gap: 18px; flex-wrap: wrap; }
  a {
    color: ${({ theme }) => theme.cores.textoSuave};
    text-decoration: none;
    &:hover { color: ${({ theme }) => theme.cores.texto}; text-decoration: underline; }
  }
`;

export default function RodapeSite() {
  return (
    <Barra>
      <Interno>
        <span>© {new Date().getFullYear()} {EMPRESA.nome}</span>
        <nav aria-label="Políticas">
          <Link to="/privacidade">Política de Privacidade</Link>
          <Link to="/cookies">Política de Cookies</Link>
        </nav>
      </Interno>
    </Barra>
  );
}
