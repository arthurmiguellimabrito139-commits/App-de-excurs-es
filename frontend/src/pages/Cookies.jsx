import { Link } from 'react-router-dom';
import { EMPRESA } from '../config';
import { Atualizacao, Documento } from '../components/Documento.js';

export default function Cookies() {
  return (
    <Documento>
      <h1>Política de Cookies</h1>
      <Atualizacao>Última atualização: 1º de novembro de 2026</Atualizacao>

      <p>
        Cookies e tecnologias parecidas, como o <em>armazenamento local</em> do navegador, são pequenos dados que um
        site guarda no seu aparelho. Esta página explica o que o {EMPRESA.nome} guarda e por quê.
      </p>

      <h2>Resumo</h2>
      <ul>
        <li>
          <strong>Não usamos cookies de publicidade, de rastreamento nem de análise</strong> (como Google Analytics ou
          pixel do Facebook).
        </li>
        <li>Guardamos só o que é necessário para o site funcionar, como manter você logado.</li>
      </ul>

      <h2>O que guardamos no seu navegador</h2>
      <table>
        <thead>
          <tr>
            <th>Nome</th>
            <th>Tipo</th>
            <th>Para quê</th>
            <th>Duração</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td><code>token</code></td>
            <td>Armazenamento local · essencial</td>
            <td>Mantém você logado. Sem ele, seria preciso entrar de novo a cada página.</td>
            <td>Até sair da conta; o acesso vence em 60 minutos</td>
          </tr>
          <tr>
            <td><code>usuario</code></td>
            <td>Armazenamento local · essencial</td>
            <td>Nome, e-mail e perfil (passageiro ou administrador) para mostrar o menu certo.</td>
            <td>Até sair da conta</td>
          </tr>
          <tr>
            <td><code>avisoCookies</code></td>
            <td>Armazenamento local · essencial</td>
            <td>Lembra que você já viu o aviso de cookies, para não mostrá-lo de novo.</td>
            <td>Até limpar os dados do navegador</td>
          </tr>
        </tbody>
      </table>
      <p>
        Por serem essenciais ao funcionamento do site, esses itens não dependem de consentimento. Eles não são enviados
        a terceiros.
      </p>

      <h2>Terceiros</h2>
      <p>
        As fontes do site vêm do <strong>Google Fonts</strong>. Ao carregar a página, seu navegador se conecta aos
        servidores do Google, que recebem o seu endereço IP. Saiba mais na{' '}
        <a href="https://policies.google.com/privacy" target="_blank" rel="noopener noreferrer">
          política de privacidade do Google
        </a>
        .
      </p>

      <h2>Como apagar</h2>
      <p>
        Clicar em <strong>Sair</strong> apaga o <code>token</code> e o <code>usuario</code>. Você também pode apagar
        tudo nas configurações do navegador (em geral, em “Privacidade” → “Limpar dados de navegação”). Se bloquear o
        armazenamento local, o site continua abrindo, mas você precisará entrar de novo sempre que recarregar a página.
      </p>

      <h2>Dúvidas</h2>
      <p>
        Fale com a gente em{' '}
        <a href={`mailto:${EMPRESA.emailPrivacidade}`}>{EMPRESA.emailPrivacidade}</a>. Veja também a{' '}
        <Link to="/privacidade">Política de Privacidade</Link>.
      </p>
    </Documento>
  );
}
