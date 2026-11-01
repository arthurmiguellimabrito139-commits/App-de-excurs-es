// Pequenos componentes de carregando / erro / vazio, reaproveitados nas páginas
import styled from 'styled-components';
import { Alerta } from './ui.js';

const Mensagem = styled.div`
  color: ${({ theme }) => theme.cores.textoSuave};
  padding: 40px 16px;
  text-align: center;
  ${({ $vazio, theme }) => $vazio && `border: 1px dashed ${theme.cores.borda}; border-radius: ${theme.raio};`}
`;

export const Carregando = ({ texto = 'Carregando…' }) => <Mensagem>{texto}</Mensagem>;

export const Erro = ({ mensagem }) => (mensagem ? <Alerta $tipo="erro">{mensagem}</Alerta> : null);

export const Vazio = ({ children }) => <Mensagem $vazio>{children}</Mensagem>;
