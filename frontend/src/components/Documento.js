// Layout de texto longo (políticas de privacidade e cookies)
import styled from 'styled-components';
import { Cartao } from './ui.js';

export const Documento = styled(Cartao).attrs({ as: 'article' })`
  max-width: 780px;
  margin: 0 auto;
  padding: 32px 36px;
  line-height: 1.65;

  h1 {
    font-size: clamp(26px, 5vw, 34px);
    margin-bottom: 4px;
  }
  h2 {
    font-size: 20px;
    margin: 28px 0 8px;
  }
  p, ul { margin: 0 0 12px; }
  ul { padding-left: 20px; }
  li { margin-bottom: 4px; }
  a { color: ${({ theme }) => theme.cores.primaria}; }

  table {
    width: 100%;
    border-collapse: collapse;
    margin: 8px 0 16px;
    font-size: 14px;
  }
  th, td {
    text-align: left;
    vertical-align: top;
    padding: 8px;
    border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
  }
  th {
    font-size: 12px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: ${({ theme }) => theme.cores.textoSuave};
  }
  code { font-size: 13px; }

  ${({ theme }) => theme.midia.mobile} {
    padding: 20px 16px;
    table { display: block; overflow-x: auto; }
  }
`;

export const Atualizacao = styled.p`
  color: ${({ theme }) => theme.cores.textoSuave};
  font-size: 14px;
`;
