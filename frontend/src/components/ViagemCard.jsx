import { Link } from 'react-router-dom';
import styled from 'styled-components';
import { moeda, data } from '../utils/formato';
import { Eyebrow, Selo, TextoSuave } from './ui.js';

const Card = styled(Link)`
  background: ${({ theme }) => theme.cores.superficie};
  border: 1px solid ${({ theme }) => theme.cores.borda};
  border-radius: ${({ theme }) => theme.raio};
  box-shadow: ${({ theme }) => theme.sombra};
  overflow: hidden;
  text-decoration: none;
  color: inherit;
  transition: transform 0.15s;

  ${({ theme }) => theme.midia.hover} {
    &:hover { transform: translateY(-3px); }
  }
`;

const Imagem = styled.div`
  position: relative;
  aspect-ratio: 16 / 10;
  background: ${({ theme }) => theme.cores.primariaClara};

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  ${Selo} {
    position: absolute;
    top: 12px;
    left: 12px;
  }
`;

const SemImagem = styled.div`
  display: grid;
  place-items: center;
  height: 100%;
  color: ${({ theme }) => theme.cores.textoSuave};
`;

const Corpo = styled.div`
  padding: 16px 18px 18px;

  h3 { font-size: 21px; }
  ${({ theme }) => theme.midia.celular} { padding: 14px 16px 16px; }
`;

const Preco = styled.div`
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-top: 12px;

  strong { font-size: 20px; }
  span {
    color: ${({ theme }) => theme.cores.textoSuave};
    font-size: 13px;
  }
`;

export default function ViagemCard({ viagem }) {
  const capa = viagem.imagens?.[0]?.url;
  const esgotada = viagem.vagasDisponiveis <= 0;

  return (
    <Card to={`/viagens/${viagem.id}`}>
      <Imagem>
        {capa ? <img src={capa} alt={viagem.titulo} loading="lazy" /> : <SemImagem>Sem imagem</SemImagem>}
        {esgotada ? (
          <Selo $tipo="erro">Esgotada</Selo>
        ) : (
          viagem.vagasDisponiveis <= 5 && <Selo $tipo="aviso">Últimas {viagem.vagasDisponiveis} vagas</Selo>
        )}
      </Imagem>
      <Corpo>
        <Eyebrow>{viagem.destino}</Eyebrow>
        <h3>{viagem.titulo}</h3>
        <TextoSuave>
          {data(viagem.dataSaida)} → {data(viagem.dataRetorno)}
        </TextoSuave>
        <Preco>
          <strong>{moeda(viagem.valor)}</strong>
          {viagem.maxParcelas > 1 && <span>ou até {viagem.maxParcelas}x</span>}
        </Preco>
      </Corpo>
    </Card>
  );
}
