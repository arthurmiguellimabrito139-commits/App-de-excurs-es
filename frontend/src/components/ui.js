// Peças pequenas reaproveitadas em todas as telas
import { Link } from 'react-router-dom';
import styled, { css } from 'styled-components';

export const Container = styled.div`
  width: 100%;
  max-width: ${({ theme }) => theme.larguraMax};
  margin: 0 auto;
  /* respeita o "notch" do iPhone deitado */
  padding-left: max(16px, env(safe-area-inset-left));
  padding-right: max(16px, env(safe-area-inset-right));

  ${({ theme }) => theme.midia.desktop} {
    padding-left: 24px;
    padding-right: 24px;
  }
`;

// Uso: <Botao $variante="primario">, <Botao as={Link} to="/" $tamanho="pequeno">
//      $bloco = largura total; $blocoNoMobile = largura total só no celular
export const Botao = styled.button`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  min-height: 40px;
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: 999px;
  padding: 9px 18px;
  font-weight: 600;
  font-size: 14px;
  background: transparent;
  color: ${({ theme }) => theme.cores.texto};
  cursor: pointer;
  text-decoration: none;
  white-space: nowrap;
  transition: background 0.15s, border-color 0.15s, color 0.15s, transform 0.1s;

  ${({ theme }) => theme.midia.hover} {
    &:hover:not(:disabled) {
      border-color: ${({ theme }) => theme.cores.primaria};
      color: ${({ theme }) => theme.cores.primaria};
    }
  }
  &:active:not(:disabled) { transform: translateY(1px); }
  &:disabled { opacity: 0.55; cursor: not-allowed; }

  /* dedo precisa de ~44px de alvo */
  ${({ theme }) => theme.midia.toque} { min-height: 44px; }

  ${({ $variante, theme }) => $variante === 'primario' && css`
    background: ${theme.cores.primaria};
    border-color: ${theme.cores.primaria};
    color: #fff;
    ${theme.midia.hover} {
      &:hover:not(:disabled) {
        background: ${theme.cores.primariaEscura};
        border-color: ${theme.cores.primariaEscura};
        color: #fff;
      }
    }
  `}

  ${({ $variante, theme }) => $variante === 'perigo' && css`
    border-color: ${theme.cores.erroFundo};
    color: ${theme.cores.erro};
    ${theme.midia.hover} {
      &:hover:not(:disabled) {
        background: ${theme.cores.erroFundo};
        border-color: ${theme.cores.erroFundo};
        color: ${theme.cores.erro};
      }
    }
  `}

  ${({ $tamanho, theme }) => $tamanho === 'pequeno' && css`
    min-height: 32px;
    padding: 5px 12px;
    font-size: 13px;
    ${theme.midia.toque} { min-height: 40px; padding: 8px 14px; }
  `}

  ${({ $bloco }) => $bloco && css`
    width: 100%;
    padding: 12px;
  `}

  ${({ $blocoNoMobile, theme }) => $blocoNoMobile && css`
    ${theme.midia.mobile} { width: 100%; }
  `}
`;

// <Selo $tipo="ok" | "aviso" | "erro">
const coresSelo = (theme, tipo) =>
  ({
    ok: [theme.cores.okFundo, theme.cores.ok],
    aviso: [theme.cores.avisoFundo, theme.cores.aviso],
    erro: [theme.cores.erroFundo, theme.cores.erro],
  })[tipo] ?? [theme.cores.primariaClara, theme.cores.primaria];

export const Selo = styled.span`
  display: inline-block;
  font-size: 12px;
  font-weight: 600;
  padding: 3px 10px;
  border-radius: 999px;
  white-space: nowrap;
  background: ${({ theme, $tipo }) => coresSelo(theme, $tipo)[0]};
  color: ${({ theme, $tipo }) => coresSelo(theme, $tipo)[1]};
`;

// <Alerta $tipo="erro" | "ok">
export const Alerta = styled.p`
  padding: 10px 14px;
  border-radius: 10px;
  margin: 12px 0;
  background: ${({ theme, $tipo }) => ($tipo === 'ok' ? theme.cores.okFundo : theme.cores.erroFundo)};
  color: ${({ theme, $tipo }) => ($tipo === 'ok' ? theme.cores.ok : theme.cores.erro)};
`;

export const Nota = styled.div`
  font-size: 13px;
  color: ${({ theme }) => theme.cores.textoSuave};
  background: ${({ theme }) => theme.cores.fundo};
  border-radius: 10px;
  padding: 10px 12px;
  margin: 12px 0;
`;

export const Cartao = styled.section`
  background: ${({ theme }) => theme.cores.superficie};
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: ${({ theme }) => theme.raio};
  box-shadow: ${({ theme }) => theme.sombra};
  padding: 20px;
  min-width: 0;

  ${({ theme }) => theme.midia.celular} { padding: 16px; }
`;

// "LENÇÓIS, BA" em cima do título
export const Eyebrow = styled.p`
  margin: 0 0 4px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: ${({ theme }) => theme.cores.destaque};
`;

export const TextoSuave = styled.span`
  color: ${({ theme }) => theme.cores.textoSuave};
`;

// ---------- Formulários ----------
export const Formulario = styled.form`
  display: flex;
  flex-direction: column;
`;

// Grade de 6 colunas para formulários: <Campo $span={3}> ocupa metade.
// No tablet o que for menor que 3 vira 3 (ou use $spanTablet); no celular tudo vira 6.
export const GradeCampos = styled.div`
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  column-gap: 16px;
`;

// <Campo><span>Rótulo</span><Entrada /></Campo>
export const Campo = styled.label`
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 14px;
  min-width: 0;
  grid-column: span ${({ $span = 6 }) => $span};

  & > span {
    font-size: 13px;
    font-weight: 600;
    color: ${({ theme }) => theme.cores.textoSuave};
  }

  /* tablet: campos curtos viram meia linha, os médios viram linha inteira */
  ${({ theme }) => theme.midia.tablet} {
    grid-column: span ${({ $span = 6, $spanTablet }) => $spanTablet ?? ($span >= 4 ? 6 : 3)};
  }
  ${({ theme }) => theme.midia.celular} {
    grid-column: span 6;
  }
`;

const estiloEntrada = css`
  width: 100%;
  min-width: 0;
  min-height: 42px;
  background: ${({ theme }) => theme.cores.superficie};
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: 10px;
  padding: 10px 12px;

  &:focus {
    outline: 2px solid ${({ theme }) => theme.cores.primaria};
    outline-offset: -1px;
    border-color: transparent;
  }
  &:disabled { opacity: 0.6; }

  ${({ theme }) => theme.midia.toque} { min-height: 46px; }
`;

export const Entrada = styled.input`${estiloEntrada}`;
export const Selecao = styled.select`${estiloEntrada}`;
export const AreaTexto = styled.textarea`
  ${estiloEntrada}
  resize: vertical;
`;

// ---------- Layout de página ----------
export const TituloPagina = styled.h1`
  font-size: clamp(26px, 5vw, 34px);
  margin-bottom: 20px;
`;

export const TituloComAcao = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 4px 12px;
  flex-wrap: wrap;
  margin-bottom: 20px;

  ${TituloPagina} { margin-bottom: 0; }

  ${({ theme }) => theme.midia.celular} {
    & > a, & > button { width: 100%; }
  }
`;

// $fim = alinha à direita; $empilharNoMobile = botões um embaixo do outro no celular
export const Acoes = styled.div`
  display: flex;
  gap: 8px;
  flex-wrap: ${({ $semQuebra }) => ($semQuebra ? 'nowrap' : 'wrap')};
  justify-content: ${({ $fim }) => ($fim ? 'flex-end' : 'flex-start')};
  margin-top: ${({ $semMargem }) => ($semMargem ? '0' : '8px')};

  ${({ $empilharNoMobile, theme }) => $empilharNoMobile && css`
    ${theme.midia.mobile} {
      flex-direction: column-reverse;
      flex-wrap: nowrap;
      & > * { width: 100%; }
    }
  `}
`;

export const Voltar = styled(Link)`
  display: inline-flex;
  align-items: center;
  min-height: 32px;
  margin-bottom: 12px;
  color: ${({ theme }) => theme.cores.textoSuave};
  text-decoration: none;
  &:hover { color: ${({ theme }) => theme.cores.texto}; }
`;

// Grade de cards de viagem: 1 coluna no celular, 2 no tablet, 3–4 no desktop
export const Grade = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 270px), 1fr));
  gap: 22px;

  ${({ theme }) => theme.midia.mobile} { gap: 16px; }
`;

// ---------- Números do painel ----------
// Se ajusta sozinho: 3 lado a lado quando cabe, senão quebra
export const Estatisticas = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 200px), 1fr));
  gap: 16px;
  margin-bottom: 20px;

  ${({ theme }) => theme.midia.mobile} { gap: 10px; }
`;

export const Estat = styled(Cartao)`
  span {
    display: block;
    font-size: 13px;
    color: ${({ theme }) => theme.cores.textoSuave};
  }
  strong {
    font-family: ${({ theme }) => theme.fontes.titulo};
    font-size: clamp(22px, 4vw, 28px);
  }

  ${({ theme }) => theme.midia.mobile} {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    gap: 12px;
    padding: 12px 16px;
  }
`;

// ---------- Tabelas ----------
export const TabelaRolavel = styled(Cartao)`
  overflow-x: auto;
  padding: 4px 8px;

  /* no celular a tabela vira cards, então o cartão de fora some */
  ${({ $empilhar, theme }) => $empilhar && css`
    ${theme.midia.mobile} {
      background: none;
      border: 0;
      box-shadow: none;
      padding: 0;
      overflow: visible;
    }
  `}
`;

/**
 * Tabela normal no desktop.
 * Com $empilhar, no celular cada linha vira um card e cada célula mostra
 * seu rótulo à esquerda — o rótulo vem do atributo data-rotulo do <td>:
 *   <td data-rotulo="Vencimento">29/10/2026</td>
 * Células com data-destaque ocupam a linha toda (ex.: nome, botões).
 */
export const Tabela = styled.table`
  width: 100%;
  border-collapse: collapse;

  th {
    text-align: left;
    font-size: 12px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: ${({ theme }) => theme.cores.textoSuave};
    font-weight: 600;
    padding: 10px 8px;
    border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
    white-space: nowrap;
  }
  td {
    padding: 10px 8px;
    border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
    vertical-align: middle;
  }
  tr:last-child td { border-bottom: 0; }
  small { color: ${({ theme }) => theme.cores.textoSuave}; }

  ${({ $empilhar, $semCard, theme }) => $empilhar && css`
    ${theme.midia.mobile} {
      thead { display: none; }
      tbody { display: flex; flex-direction: column; gap: ${$semCard ? '0' : '12px'}; }

      tr {
        display: flex;
        flex-direction: column;
        ${$semCard
          ? css`
              border-bottom: 1px solid ${theme.cores.borda};
              padding: 10px 0;
              &:last-child { border-bottom: 0; }
            `
          : css`
              background: ${theme.cores.superficie};
              border: 1px solid ${theme.cores.borda};
              border-radius: ${theme.raio};
              box-shadow: ${theme.sombra};
              padding: 12px 16px;
            `}
      }

      td {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        padding: 5px 0;
        border: 0;
        text-align: right;
      }
      td[data-rotulo]::before {
        content: attr(data-rotulo);
        flex-shrink: 0;
        text-align: left;
        font-size: 12px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        color: ${theme.cores.textoSuave};
      }
      td[data-destaque] {
        display: block;
        text-align: left;
      }
      td:empty { display: none; }
    }
  `}
`;
