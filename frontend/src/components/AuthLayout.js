// Estrutura compartilhada pelas telas de Login e Cadastro
import styled from 'styled-components';
import { Cartao } from './ui.js';

export const Centro = styled.div`
  display: flex;
  justify-content: center;
  padding-top: 24px;

  ${({ theme }) => theme.midia.celular} { padding-top: 0; }
`;

export const CartaoAuth = styled(Cartao)`
  width: 100%;
  max-width: 400px;
`;

export const Rodape = styled.p`
  text-align: center;
  color: ${({ theme }) => theme.cores.textoSuave};
  margin: 14px 0 0;
`;
