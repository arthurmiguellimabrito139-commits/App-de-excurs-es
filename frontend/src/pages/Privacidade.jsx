import { Link } from 'react-router-dom';
import { EMPRESA } from '../config';
import { Atualizacao, Documento } from '../components/Documento.js';

export default function Privacidade() {
  const email = <a href={`mailto:${EMPRESA.emailPrivacidade}`}>{EMPRESA.emailPrivacidade}</a>;

  return (
    <Documento>
      <h1>Política de Privacidade</h1>
      <Atualizacao>Última atualização: 1º de novembro de 2026</Atualizacao>

      <p>
        Esta política explica quais dados pessoais o {EMPRESA.nome} coleta, para que usa e quais são os seus direitos,
        conforme a Lei Geral de Proteção de Dados (LGPD, Lei nº 13.709/2018).
      </p>

      <h2>1. Quem é o responsável</h2>
      <p>
        O controlador dos dados é o {EMPRESA.nome}. Dúvidas e pedidos sobre seus dados: {email}.
      </p>

      <h2>2. Quais dados coletamos</h2>
      <ul>
        <li>
          <strong>Cadastro:</strong> nome, e-mail e senha. A senha é guardada só como hash (BCrypt), nunca em texto — nem
          nós conseguimos lê-la.
        </li>
        <li>
          <strong>Compras:</strong> viagem escolhida, forma de pagamento, valor, parcelas, vencimentos e situação do
          pagamento.
        </li>
        <li>
          <strong>Dados técnicos:</strong> endereço IP, usado para limitar tentativas de login e acessos em excesso, e
          registros de erro do servidor.
        </li>
      </ul>
      <p>
        <strong>Não guardamos dados de cartão.</strong> Quando o pagamento por cartão estiver disponível, os dados serão
        digitados diretamente no ambiente do processador de pagamentos, que tem a sua própria política de privacidade.
      </p>

      <h2>3. Para que usamos e com que base legal</h2>
      <ul>
        <li>
          Criar e manter sua conta, vender a passagem e controlar as parcelas — <em>execução de contrato</em> (art. 7º,
          V, da LGPD).
        </li>
        <li>
          Proteger o site contra fraude e ataques (ex.: muitas tentativas de senha) — <em>legítimo interesse</em> (art.
          7º, IX).
        </li>
        <li>
          Guardar registros de vendas pelo prazo exigido em lei — <em>cumprimento de obrigação legal</em> (art. 7º, II).
        </li>
      </ul>
      <p>Não vendemos seus dados e não os usamos para publicidade.</p>

      <h2>4. Com quem compartilhamos</h2>
      <ul>
        <li>
          <strong>Organizador da excursão:</strong> o administrador responsável pela viagem vê o nome e o e-mail de quem
          comprou a passagem, para organizar o embarque.
        </li>
        <li>
          <strong>Processador de pagamentos</strong>, quando houver pagamento on-line.
        </li>
        <li>
          <strong>Hospedagem e infraestrutura</strong> do site, que guardam os dados em nosso nome.
        </li>
        <li>
          <strong>Google Fonts:</strong> as fontes do site são baixadas dos servidores do Google, que recebem o seu
          endereço IP nesse momento.
        </li>
        <li>
          <strong>Autoridades</strong>, quando houver obrigação legal ou ordem judicial.
        </li>
      </ul>

      <h2>5. Por quanto tempo guardamos</h2>
      <p>
        Os dados da conta ficam guardados enquanto ela existir. Registros de compras podem ser mantidos após o
        encerramento da conta pelo prazo exigido pela legislação fiscal e de defesa do consumidor. O controle de
        tentativas por IP é temporário e fica só na memória do servidor.
      </p>

      <h2>6. Seus direitos</h2>
      <p>Pela LGPD, você pode pedir a qualquer momento:</p>
      <ul>
        <li>confirmação de que tratamos seus dados e acesso a eles;</li>
        <li>correção de dados incompletos ou errados;</li>
        <li>exclusão da conta e dos dados que não precisamos guardar por lei;</li>
        <li>portabilidade dos dados;</li>
        <li>informação sobre com quem compartilhamos seus dados.</li>
      </ul>
      <p>
        Envie o pedido para {email} a partir do e-mail cadastrado. Você também pode reclamar à Autoridade Nacional de
        Proteção de Dados (ANPD).
      </p>

      <h2>7. Segurança</h2>
      <p>
        Usamos senhas com hash, sessões que expiram em 60 minutos, limite de tentativas de login, consultas protegidas
        contra injeção de SQL e acesso restrito por perfil (passageiro ou administrador). Nenhum sistema é 100% seguro;
        se houver um incidente que possa trazer risco a você, avisaremos e comunicaremos a ANPD.
      </p>

      <h2>8. Cookies</h2>
      <p>
        Veja como guardamos informações no seu navegador na <Link to="/cookies">Política de Cookies</Link>.
      </p>

      <h2>9. Mudanças nesta política</h2>
      <p>
        Se esta política mudar, a data no topo será atualizada. Mudanças importantes serão avisadas no site.
      </p>
    </Documento>
  );
}
