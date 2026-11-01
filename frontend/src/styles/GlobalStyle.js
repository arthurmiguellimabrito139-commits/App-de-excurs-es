import { createGlobalStyle } from 'styled-components';

// Estilos base da página (as fontes vêm do Google Fonts, no index.html)
export const GlobalStyle = createGlobalStyle`
  *, *::before, *::after { box-sizing: border-box; }

  html {
    -webkit-text-size-adjust: 100%;
    text-size-adjust: 100%;
    scroll-behavior: smooth;
    /* âncoras (#pagamento) não ficam escondidas atrás da navbar fixa */
    scroll-padding-top: calc(${({ theme }) => theme.alturaNavbar} + 16px);
  }

  body {
    margin: 0;
    min-width: 320px;
    background: ${({ theme }) => theme.cores.fundo};
    color: ${({ theme }) => theme.cores.texto};
    font: 400 15px/1.55 ${({ theme }) => theme.fontes.texto};
    -webkit-font-smoothing: antialiased;
    -webkit-tap-highlight-color: transparent;
    overflow-wrap: break-word;
  }

  h1, h2, h3 {
    font-family: ${({ theme }) => theme.fontes.titulo};
    line-height: 1.15;
    margin: 0 0 0.5rem;
    text-wrap: balance;
  }

  a { color: ${({ theme }) => theme.cores.primaria}; }
  img { display: block; max-width: 100%; }
  button, input, select, textarea { font: inherit; color: inherit; }

  /* iOS dá zoom na página quando o campo tem fonte < 16px */
  ${({ theme }) => theme.midia.toque} {
    input, select, textarea { font-size: 16px; }
  }

  :focus-visible {
    outline: 2px solid ${({ theme }) => theme.cores.primaria};
    outline-offset: 2px;
  }

  @media (prefers-reduced-motion: reduce) {
    html { scroll-behavior: auto; }
    * { transition: none !important; animation: none !important; }
  }
`;
