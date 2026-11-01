import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import styled from 'styled-components';
import * as viagemService from '../services/viagemService';
import { useAuth } from '../context/AuthContext';
import Galeria from '../components/Galeria';
import SimuladorPagamento from '../components/SimuladorPagamento';
import { Carregando, Erro } from '../components/Estado';
import { Acoes, Botao, Eyebrow, Voltar } from '../components/ui.js';
import { moeda, data } from '../utils/formato';

// Desktop: conteúdo + painel de 360px. Tablet: painel de 300px. Celular: uma coluna.
const Colunas = styled.div`
  display: grid;
  grid-template-columns: minmax(0, 1fr) 360px;
  gap: 32px;
  align-items: start;

  ${({ theme }) => theme.midia.tablet} {
    grid-template-columns: minmax(0, 1fr) 300px;
    gap: 20px;
  }
  ${({ theme }) => theme.midia.mobile} {
    grid-template-columns: minmax(0, 1fr);
    /* espaço para a barra de compra fixa não cobrir o fim da página */
    padding-bottom: ${({ $comBarra }) => ($comBarra ? '76px' : '0')};
  }
`;

const Cabecalho = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px 16px;
  margin: 24px 0 8px;
  flex-wrap: wrap;

  h1 { font-size: clamp(26px, 4vw, 40px); }

  ${({ theme }) => theme.midia.mobile} { margin-top: 16px; }
`;

const Fatos = styled.dl`
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  margin: 16px 0 28px;

  div {
    background: ${({ theme }) => theme.cores.superficie};
    border: 1px solid ${({ theme }) => theme.cores.borda};
    border-radius: 10px;
    padding: 10px 12px;
    min-width: 0;
  }
  dt {
    font-size: 12px;
    color: ${({ theme }) => theme.cores.textoSuave};
  }
  dd {
    margin: 0;
    font-weight: 600;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  /* no tablet a coluna de conteúdo é estreita: 2x2 */
  ${({ theme }) => theme.midia.tablet} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
  }
`;

const Descricao = styled.p`
  white-space: pre-line;
  max-width: 68ch;
`;

// ---------- Barra fixa de compra (só celular) ----------
const BarraCompra = styled.div`
  display: none;

  ${({ theme }) => theme.midia.mobile} {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 10;
    padding: 10px 16px;
    padding-bottom: max(10px, env(safe-area-inset-bottom));
    background: ${({ theme }) => theme.cores.superficie};
    border-top: 1px solid ${({ theme }) => theme.cores.borda};
    box-shadow: 0 -6px 20px rgba(31, 42, 43, 0.08);
    transform: translateY(${({ $visivel }) => ($visivel ? '0' : '110%')});
    transition: transform 0.2s;
  }
`;

const PrecoBarra = styled.div`
  display: flex;
  flex-direction: column;
  line-height: 1.2;
  min-width: 0;

  strong { font-size: 18px; }
  small {
    color: ${({ theme }) => theme.cores.textoSuave};
    font-size: 12px;
  }
`;

export default function ViagemDetalhe() {
  const { id } = useParams();
  const { usuario, ehAdmin } = useAuth();
  const [viagem, setViagem] = useState(null);
  const [erro, setErro] = useState('');
  const [painelVisivel, setPainelVisivel] = useState(false);

  useEffect(() => {
    viagemService.buscar(id).then(setViagem).catch((e) => setErro(e.message));
  }, [id]);

  // Esconde a barra de baixo quando o painel de pagamento já está na tela
  useEffect(() => {
    if (!viagem) return;
    const painel = document.getElementById('pagamento');
    if (!painel || !('IntersectionObserver' in window)) return;
    const obs = new IntersectionObserver(([e]) => setPainelVisivel(e.isIntersecting), { threshold: 0.15 });
    obs.observe(painel);
    return () => obs.disconnect();
  }, [viagem]);

  if (erro) return <Erro mensagem={erro} />;
  if (!viagem) return <Carregando />;

  const ehDono = ehAdmin && viagem.adminId === usuario?.id;
  const esgotada = viagem.vagasDisponiveis <= 0;

  const irParaPagamento = () =>
    document.getElementById('pagamento')?.scrollIntoView({ behavior: 'smooth', block: 'start' });

  return (
    <>
      <Voltar to="/">← Todas as viagens</Voltar>

      <Colunas $comBarra={!ehAdmin}>
        <div>
          <Galeria imagens={viagem.imagens} titulo={viagem.titulo} />

          <Cabecalho>
            <div>
              <Eyebrow>{viagem.destino}</Eyebrow>
              <h1>{viagem.titulo}</h1>
            </div>
            {ehDono && (
              <Acoes $semMargem>
                <Botao as={Link} to={`/admin/viagens/${viagem.id}/editar`} $tamanho="pequeno">
                  Editar
                </Botao>
                <Botao as={Link} to={`/admin/viagens/${viagem.id}/passageiros`} $tamanho="pequeno">
                  Passageiros
                </Botao>
              </Acoes>
            )}
          </Cabecalho>

          <Fatos>
            <div>
              <dt>Ida</dt>
              <dd>{data(viagem.dataSaida)}</dd>
            </div>
            <div>
              <dt>Volta</dt>
              <dd>{data(viagem.dataRetorno)}</dd>
            </div>
            <div>
              <dt>Valor</dt>
              <dd>{moeda(viagem.valor)}</dd>
            </div>
            <div>
              <dt>Vagas</dt>
              <dd>
                {viagem.vagasDisponiveis} de {viagem.vagas}
              </dd>
            </div>
          </Fatos>

          <h2>Sobre a viagem</h2>
          <Descricao>{viagem.descricao}</Descricao>
        </div>

        <SimuladorPagamento viagem={viagem} />
      </Colunas>

      {!ehAdmin && (
        <BarraCompra $visivel={!painelVisivel} aria-hidden={painelVisivel}>
          <PrecoBarra>
            <strong>{moeda(viagem.valor)}</strong>
            <small>{esgotada ? 'Esgotada' : `ou até ${viagem.maxParcelas}x · ${viagem.vagasDisponiveis} vagas`}</small>
          </PrecoBarra>
          <Botao $variante="primario" onClick={irParaPagamento} disabled={esgotada} tabIndex={painelVisivel ? -1 : 0}>
            {esgotada ? 'Esgotada' : 'Comprar'}
          </Botao>
        </BarraCompra>
      )}
    </>
  );
}
