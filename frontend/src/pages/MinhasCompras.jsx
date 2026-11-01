import { useEffect, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import styled from 'styled-components';
import * as pagamentoService from '../services/pagamentoService';
import { Carregando, Erro, Vazio } from '../components/Estado';
import { Alerta, Botao, Cartao, Eyebrow, Selo, Tabela, TextoSuave, TituloPagina } from '../components/ui.js';
import { moeda, data, NOMES_METODO } from '../utils/formato';

const Lista = styled.div`
  display: flex;
  flex-direction: column;
  gap: 20px;
`;

const Topo = styled.div`
  display: flex;
  gap: 16px;
  align-items: flex-start;

  ${({ theme }) => theme.midia.mobile} {
    flex-wrap: wrap;
    gap: 8px 12px;
  }
`;

const Capa = styled.img`
  flex-shrink: 0;
  width: 96px;
  height: 72px;
  object-fit: cover;
  border-radius: 10px;

  ${({ theme }) => theme.midia.mobile} {
    width: 64px;
    height: 56px;
  }
`;

const Info = styled.div`
  flex: 1;
  min-width: 0;

  /* no celular o texto fica com toda a largura ao lado da foto e o selo desce */
  ${({ theme }) => theme.midia.mobile} {
    flex-basis: calc(100% - 76px);
  }

  h2 {
    font-size: clamp(19px, 4vw, 22px);
    margin: 0;
  }
  h2 a {
    color: inherit;
    text-decoration: none;
  }
`;

const Progresso = styled.div`
  height: 6px;
  border-radius: 999px;
  background: ${({ theme }) => theme.cores.fundo};
  margin: 16px 0;
  overflow: hidden;

  div {
    height: 100%;
    background: ${({ theme }) => theme.cores.primaria};
    transition: width 0.3s;
  }
`;

const Direita = styled.td`
  text-align: right;
`;

const Esconder = styled.span`
  display: none;
  ${({ theme }) => theme.midia.mobile} { display: inline; }
`;

/**
 * No celular cada parcela vira 2 linhas compactas:
 *   29/10/2026          R$ 472,50
 *   1ª parcela              [Paga]
 *   [        Pagar           ]   ← só na próxima em aberto
 */
const TabelaParcelas = styled(Tabela)`
  ${({ theme }) => theme.midia.mobile} {
    thead { display: none; }
    tbody { display: block; }

    tr {
      display: grid;
      grid-template-columns: 1fr auto;
      grid-template-areas:
        'venc valor'
        'num status'
        'btn btn';
      column-gap: 12px;
      padding: 10px 0;
      border-bottom: 1px solid ${({ theme }) => theme.cores.borda};
    }
    tr:last-child { border-bottom: 0; }

    td {
      padding: 0;
      border: 0;
    }
    td:nth-child(1) {
      grid-area: num;
      font-size: 13px;
      color: ${({ theme }) => theme.cores.textoSuave};
    }
    td:nth-child(2) { grid-area: venc; font-weight: 600; }
    td:nth-child(3) { grid-area: valor; font-weight: 600; text-align: right; }
    td:nth-child(4) { grid-area: status; text-align: right; }
    td:nth-child(5) { grid-area: btn; }
    td:nth-child(5):not(:empty) { padding-top: 10px; }
    td:nth-child(5) button { width: 100%; }
  }
`;

// No celular o selo de status sai da direita e vai para baixo do título
const SeloStatus = styled(Selo)`
  ${({ theme }) => theme.midia.mobile} { margin-left: 76px; }
`;

export default function MinhasCompras() {
  const location = useLocation();
  const [compras, setCompras] = useState(null);
  const [erro, setErro] = useState('');
  const [pagando, setPagando] = useState(null);

  const carregar = () =>
    pagamentoService
      .minhasCompras()
      .then(setCompras)
      .catch((e) => setErro(e.message));

  useEffect(() => {
    carregar();
  }, []);

  const pagar = async (parcelaId) => {
    setPagando(parcelaId);
    setErro('');
    try {
      await pagamentoService.pagarParcela(parcelaId);
      await carregar();
    } catch (e) {
      setErro(e.message);
    } finally {
      setPagando(null);
    }
  };

  if (!compras && !erro) return <Carregando />;

  return (
    <>
      <TituloPagina>Minhas compras</TituloPagina>
      {location.state?.compraNova && <Alerta $tipo="ok">Compra registrada! Suas parcelas estão abaixo.</Alerta>}
      <Erro mensagem={erro} />

      {compras?.length === 0 && (
        <Vazio>
          Você ainda não comprou nenhuma viagem. <Link to="/a-venda">Ver viagens à venda</Link>
        </Vazio>
      )}

      <Lista>
        {compras?.map((c) => {
          const pagas = c.parcelas.filter((p) => p.status === 'paga').length;
          const proxima = c.parcelas.find((p) => p.status === 'aberta');
          const progresso = (pagas / c.parcelas.length) * 100;

          return (
            <Cartao as="article" key={c.id}>
              <Topo>
                {c.viagem?.imagem && <Capa src={c.viagem.imagem} alt="" />}
                <Info>
                  <Eyebrow>{c.viagem?.destino}</Eyebrow>
                  <h2>
                    <Link to={`/viagens/${c.viagem?.id}`}>{c.viagem?.titulo}</Link>
                  </h2>
                  <TextoSuave>
                    Saída {data(c.viagem?.dataSaida)} · {NOMES_METODO[c.metodo]} em {c.numParcelas}x · total{' '}
                    {moeda(c.valorTotal)}
                  </TextoSuave>
                </Info>
                <SeloStatus $tipo={c.status === 'pago' ? 'ok' : 'aviso'}>
                  {c.status === 'pago' ? 'Quitada' : `${pagas}/${c.parcelas.length} pagas`}
                </SeloStatus>
              </Topo>

              <Progresso aria-label={`${Math.round(progresso)}% pago`}>
                <div style={{ width: `${progresso}%` }} />
              </Progresso>

              <TabelaParcelas>
                <thead>
                  <tr>
                    <th>Parcela</th>
                    <th>Vencimento</th>
                    <th>Valor</th>
                    <th>Status</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {c.parcelas.map((p) => (
                    <tr key={p.id}>
                      <td>
                        {p.numero}ª<Esconder> parcela</Esconder>
                      </td>
                      <td>{data(p.vencimento)}</td>
                      <td>{moeda(p.valor)}</td>
                      <td>
                        <Selo $tipo={p.status === 'paga' ? 'ok' : undefined}>
                          {p.status === 'paga' ? 'Paga' : 'Aberta'}
                        </Selo>
                      </td>
                      <Direita>
                        {proxima?.id === p.id && (
                          <Botao
                            $variante="primario"
                            $tamanho="pequeno"
                            onClick={() => pagar(p.id)}
                            disabled={pagando === p.id}
                          >
                            {pagando === p.id ? 'Pagando…' : 'Pagar'}
                          </Botao>
                        )}
                      </Direita>
                    </tr>
                  ))}
                </tbody>
              </TabelaParcelas>
            </Cartao>
          );
        })}
      </Lista>
    </>
  );
}
