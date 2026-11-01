# Embarque — app de excursões (front-end)

React + Vite. Passageiro vê e paga viagens; Admin publica viagens (valor, imagens, descrição, vagas, parcelas máximas).
Pagamento com parcelamento via **Strategy** (Pix, Cartão, Boleto) calculado no back-end em C#.

## Rodar

```bash
npm install
cp .env.example .env     # no Windows: copy .env.example .env
npm run dev              # http://localhost:5173
```

O `.env` padrão usa a **API em C#** (`VITE_USE_MOCK=false`, `VITE_API_URL=http://localhost:5002/api`).
Veja o README da raiz para subir a API.

Para ver o front sem o back, use `VITE_USE_MOCK=true`: tudo roda no navegador com dados simulados.
Contas de teste do modo simulado (senha `123456`): `admin@teste.com` e `passageiro@teste.com`.

O contrato completo (rotas e JSON) está em **[API.md](API.md)**.

## Estrutura

```
src/
├── config.js                 # USE_MOCK e API_URL
├── main.jsx / App.jsx        # rotas
├── context/AuthContext.jsx   # token + usuário, login/logout
├── services/                 # única camada que fala com a API
│   ├── http.js               # fetch + Bearer token + tratamento de erro
│   ├── authService.js
│   ├── viagemService.js
│   └── pagamentoService.js
├── mocks/                    # API falsa (mesmo formato da real)
│   ├── mockApi.js
│   └── pagamentoStrategies.js   # espelho do Strategy do C#
├── styles/
│   ├── theme.js              # cores, fontes, raio, sombra e breakpoints (ThemeProvider)
│   └── GlobalStyle.js        # estilos base (createGlobalStyle)
├── components/
│   ├── ui.js                 # peças reaproveitadas: Botao, Cartao, Campo, Entrada, Selo, Tabela…
│   ├── AuthLayout.js         # moldura de Login/Cadastro
│   └── Navbar, RotaProtegida, ViagemCard, Galeria, SimuladorPagamento, Estado
├── pages/
│   ├── Viagens.jsx, ViagensDisponiveis.jsx, ViagemDetalhe.jsx, Login.jsx, Cadastro.jsx
│   ├── MinhasCompras.jsx     # passageiro
│   └── admin/                # PainelAdmin, ViagemForm (criar/editar), Passageiros
└── utils/                    # formatação de moeda/data, regra "à venda", localStorage
```

## Estilos (styled-components)

Mesmo padrão do Mesa Posta:
- cores e medidas só no `theme.js` → nos componentes: `${({ theme }) => theme.cores.primaria}`;
- peças comuns em `components/ui.js`, com variações por props transitórias (`$`):
  `<Botao $variante="primario" | "perigo" $tamanho="pequeno" $bloco>`, `<Selo $tipo="ok" | "aviso" | "erro">`;
- botão que vira link: `<Botao as={Link} to="/admin">`;
- estilos específicos ficam no topo do próprio arquivo da página/componente.

## Responsivo

Faixas definidas em `theme.midia` (use dentro do styled: `${({ theme }) => theme.midia.mobile} { ... }`):

| Chave | Tela | O que muda |
|---|---|---|
| `celular` | ≤ 480px | formulários em 1 coluna, botões de título em largura total |
| `mobile` | ≤ 767px | menu "hambúrguer", tabelas viram cards, galeria com deslize, barra fixa de compra |
| `tablet` | ≤ 1023px | detalhe com painel de pagamento de 300px, formulário em 2 colunas |
| `desktop` | ≥ 1024px | layout completo |
| `hover` / `toque` | mouse / dedo | efeitos de hover só com mouse; alvos de toque ≥ 44px e campos com 16px (evita zoom no iOS) |

Tabela que vira card no celular: `<Tabela $empilhar>` + `data-rotulo="Nome"` em cada `<td>`
(`data-destaque` para a célula que ocupa a linha inteira). Testado de 320px a 1920px e com o celular deitado.

