// Cores, fontes e medidas do app, num lugar só.
// Nos componentes: ${({ theme }) => theme.cores.primaria}
//
// Responsivo (mobile-first não, desktop-first): escreva o estilo de desktop
// e sobrescreva embaixo com as media queries prontas:
//   ${({ theme }) => theme.midia.mobile} { ... }
export const theme = {
  cores: {
    fundo: '#f6f1e9',
    superficie: '#fffdf9',
    borda: '#e6ddcf',
    texto: '#1f2a2b',
    textoSuave: '#6b7271',

    primaria: '#0f5c57',
    primariaEscura: '#0a4440',
    primariaClara: '#e1efec',
    destaque: '#e0663d',

    ok: '#2f7d4f',
    okFundo: '#e3f2e8',
    aviso: '#a0620b',
    avisoFundo: '#fbefd9',
    erro: '#b3362b',
    erroFundo: '#fbe4e1',
  },
  fontes: {
    titulo: '"Fraunces", Georgia, serif',
    texto: '"Inter", system-ui, sans-serif',
  },
  sombra: '0 1px 2px rgba(31, 42, 43, 0.06), 0 8px 24px rgba(31, 42, 43, 0.06)',
  raio: '14px',
  larguraMax: '1200px',
  alturaNavbar: '64px',

  // Faixas de tela:
  //   celular pequeno  ≤ 480px  (iPhone SE, Androids compactos)
  //   mobile           ≤ 767px  (todos os celulares, inclusive deitados)
  //   tablet           ≤ 1023px (iPad em pé, tablets Android)
  //   desktop          ≥ 1024px
  midia: {
    celular: '@media (max-width: 480px)',
    mobile: '@media (max-width: 767px)',
    tablet: '@media (max-width: 1023px)',
    desktop: '@media (min-width: 1024px)',
    // dispositivo com mouse (para efeitos de hover não "grudarem" no toque)
    hover: '@media (hover: hover) and (pointer: fine)',
    // dedo: alvos de toque maiores
    toque: '@media (pointer: coarse)',
  },
};
