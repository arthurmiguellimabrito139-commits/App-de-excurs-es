import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import styled from 'styled-components';
import { useAuth } from '../context/AuthContext';
import * as pagamentoService from '../services/pagamentoService';
import { moeda, data } from '../utils/formato';
import { Alerta, Botao, Campo, Cartao, Nota, Selecao } from './ui.js';

const Painel = styled(Cartao).attrs({ as: 'aside', id: 'pagamento' })`
  position: sticky;
  top: calc(${({ theme }) => theme.alturaNavbar} + 20px);

  h2 { font-size: 22px; }

  ${({ theme }) => theme.midia.mobile} {
    position: static;
  }
`;

const Metodos = styled.div`
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin: 12px 0 16px;
`;

const Metodo = styled.label`
  display: grid;
  grid-template-columns: auto 1fr;
  column-gap: 10px;
  align-items: center;
  border: 1px solid ${({ theme, $ativo }) => ($ativo ? theme.cores.primaria : theme.cores.borda)};
  background: ${({ theme, $ativo }) => ($ativo ? theme.cores.primariaClara : 'transparent')};
  border-radius: 10px;
  padding: 10px 12px;
  cursor: pointer;

  input {
    grid-row: span 2;
    accent-color: ${({ theme }) => theme.cores.primaria};
  }
  strong { font-weight: 600; }
  small {
    font-size: 12px;
    color: ${({ theme }) => theme.cores.textoSuave};
  }
`;

const Resumo = styled.div`
  border-top: 1px solid ${({ theme }) => theme.cores.borda};
  padding-top: 12px;
  margin-bottom: 12px;
`;

const Linha = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  padding: 3px 0;

  strong { font-size: 22px; }
`;

const Diferenca = styled.p`
  margin: 2px 0 8px;
  font-size: 13px;
  text-align: right;
  color: ${({ theme, $economia }) => ($economia ? theme.cores.ok : theme.cores.aviso)};
`;

const ListaParcelas = styled.details`
  summary {
    cursor: pointer;
    font-size: 14px;
    color: ${({ theme }) => theme.cores.primaria};
  }
  ul {
    list-style: none;
    padding: 0;
    margin: 8px 0 0;
    font-size: 13px;
    max-height: 180px;
    overflow-y: auto;
  }
  li {
    display: flex;
    justify-content: space-between;
    padding: 4px 0;
    border-bottom: 1px dashed ${({ theme }) => theme.cores.borda};
  }
`;

/**
 * Lista as formas de pagamento (strategies do back), deixa o passageiro escolher
 * método + nº de parcelas, mostra a simulação e confirma a compra.
 */
export default function SimuladorPagamento({ viagem }) {
  const { usuario, ehPassageiro, ehAdmin } = useAuth();
  const navigate = useNavigate();

  const [metodos, setMetodos] = useState([]);
  const [metodo, setMetodo] = useState('pix');
  const [numParcelas, setNumParcelas] = useState(1);
  const [simulacao, setSimulacao] = useState(null);
  const [erro, setErro] = useState('');
  const [comprando, setComprando] = useState(false);

  useEffect(() => {
    pagamentoService.metodos().then(setMetodos).catch((e) => setErro(e.message));
  }, []);

  // Recalcula sempre que muda método ou parcelas
  useEffect(() => {
    let ativo = true;
    setErro('');
    pagamentoService
      .simular(viagem.id, metodo, numParcelas)
      .then((s) => ativo && setSimulacao(s))
      .catch((e) => ativo && setErro(e.message));
    return () => {
      ativo = false;
    };
  }, [viagem.id, metodo, numParcelas]);

  const trocarMetodo = (m) => {
    setMetodo(m);
    setNumParcelas(1);
  };

  const confirmar = async () => {
    setComprando(true);
    setErro('');
    try {
      await pagamentoService.comprar(viagem.id, metodo, numParcelas);
      navigate('/minhas-compras', { state: { compraNova: true } });
    } catch (e) {
      setErro(e.message);
    } finally {
      setComprando(false);
    }
  };

  const maxParcelas = simulacao?.maxParcelas ?? 1;
  const esgotada = viagem.vagasDisponiveis <= 0;

  return (
    <Painel>
      <h2>Pagamento</h2>

      <Metodos role="radiogroup" aria-label="Forma de pagamento">
        {metodos.map((m) => (
          <Metodo key={m.metodo} $ativo={metodo === m.metodo}>
            <input
              type="radio"
              name="metodo"
              value={m.metodo}
              checked={metodo === m.metodo}
              onChange={() => trocarMetodo(m.metodo)}
            />
            <strong>{m.nome}</strong>
            <small>{m.descricao}</small>
          </Metodo>
        ))}
      </Metodos>

      <Campo>
        <span>Parcelas</span>
        <Selecao value={numParcelas} onChange={(e) => setNumParcelas(Number(e.target.value))} disabled={maxParcelas <= 1}>
          {Array.from({ length: maxParcelas }, (_, i) => i + 1).map((n) => (
            <option key={n} value={n}>
              {n}x
            </option>
          ))}
        </Selecao>
      </Campo>

      {simulacao && (
        <Resumo>
          <Linha>
            <span>Valor da viagem</span>
            <span>{moeda(viagem.valor)}</span>
          </Linha>
          <Linha>
            <span>Total</span>
            <strong>{moeda(simulacao.total)}</strong>
          </Linha>
          {simulacao.total !== viagem.valor && (
            <Diferenca $economia={simulacao.total < viagem.valor}>
              {simulacao.total < viagem.valor ? 'Economia de ' : 'Juros de '}
              {moeda(Math.abs(simulacao.total - viagem.valor))}
            </Diferenca>
          )}

          {simulacao.parcelas.length > 1 && (
            <ListaParcelas>
              <summary>
                {simulacao.numParcelas}x de {moeda(simulacao.parcelas[0].valor)} — ver vencimentos
              </summary>
              <ul>
                {simulacao.parcelas.map((p) => (
                  <li key={p.numero}>
                    <span>
                      {p.numero}ª · {data(p.vencimento)}
                    </span>
                    <span>{moeda(p.valor)}</span>
                  </li>
                ))}
              </ul>
            </ListaParcelas>
          )}
        </Resumo>
      )}

      {metodo === 'cartao' && (
        <Nota>
          Os dados do cartão serão coletados pelo gateway de pagamento (ex.: Mercado Pago) — o app nunca guarda número
          de cartão.
        </Nota>
      )}

      {erro && <Alerta $tipo="erro">{erro}</Alerta>}

      {esgotada ? (
        <Botao $variante="primario" $bloco disabled>
          Esgotada
        </Botao>
      ) : ehPassageiro ? (
        <Botao $variante="primario" $bloco onClick={confirmar} disabled={comprando || !simulacao}>
          {comprando ? 'Processando…' : `Comprar por ${simulacao ? moeda(simulacao.total) : '…'}`}
        </Botao>
      ) : ehAdmin ? (
        <Nota>Você está como administrador — só passageiros compram passagens.</Nota>
      ) : (
        <Botao as={Link} to="/login" state={{ de: `/viagens/${viagem.id}` }} $variante="primario" $bloco>
          {usuario ? 'Entrar como passageiro' : 'Entre para comprar'}
        </Botao>
      )}
    </Painel>
  );
}
