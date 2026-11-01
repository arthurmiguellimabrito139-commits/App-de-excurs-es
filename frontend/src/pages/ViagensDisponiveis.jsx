import { useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';
import * as viagemService from '../services/viagemService';
import ViagemCard from '../components/ViagemCard';
import { Carregando, Erro, Vazio } from '../components/Estado';
import { Entrada, Grade, Selecao, TituloPagina } from '../components/ui.js';
import { estaAVenda } from '../utils/viagem';

const ORDENACOES = {
  data: { rotulo: 'Saída mais próxima', fn: (a, b) => a.dataSaida.localeCompare(b.dataSaida) },
  menorPreco: { rotulo: 'Menor preço', fn: (a, b) => a.valor - b.valor },
  maiorPreco: { rotulo: 'Maior preço', fn: (a, b) => b.valor - a.valor },
  vagas: { rotulo: 'Últimas vagas', fn: (a, b) => a.vagasDisponiveis - b.vagasDisponiveis },
};

const Subtitulo = styled.p`
  margin: -12px 0 20px;
  color: ${({ theme }) => theme.cores.textoSuave};
`;

const Filtros = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
  flex-wrap: wrap;

  input {
    flex: 1 1 260px;
    border-radius: 999px;
    padding: 10px 16px;
  }
  select {
    flex: 0 0 auto;
    width: auto;
    border-radius: 999px;
    padding: 10px 16px;
  }

  ${({ theme }) => theme.midia.celular} {
    margin-bottom: 18px;
    select { flex: 1 1 100%; }
  }
`;

export default function ViagensDisponiveis() {
  const [viagens, setViagens] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [busca, setBusca] = useState('');
  const [ordem, setOrdem] = useState('data');

  useEffect(() => {
    viagemService
      .listar()
      .then((lista) => setViagens(lista.filter((v) => estaAVenda(v))))
      .catch((e) => setErro(e.message))
      .finally(() => setCarregando(false));
  }, []);

  const exibidas = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    return viagens
      .filter((v) => !termo || `${v.titulo} ${v.destino}`.toLowerCase().includes(termo))
      .sort(ORDENACOES[ordem].fn);
  }, [viagens, busca, ordem]);

  return (
    <>
      <TituloPagina>Viagens à venda</TituloPagina>
      {!carregando && (
        <Subtitulo>
          {viagens.length} {viagens.length === 1 ? 'viagem com vagas abertas' : 'viagens com vagas abertas'}
        </Subtitulo>
      )}

      <Filtros>
        <Entrada
          type="search"
          placeholder="Buscar por destino ou nome"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
        />
        <Selecao value={ordem} onChange={(e) => setOrdem(e.target.value)} aria-label="Ordenar por">
          {Object.entries(ORDENACOES).map(([chave, { rotulo }]) => (
            <option key={chave} value={chave}>
              {rotulo}
            </option>
          ))}
        </Selecao>
      </Filtros>

      <Erro mensagem={erro} />
      {carregando ? (
        <Carregando texto="Buscando viagens à venda…" />
      ) : exibidas.length === 0 ? (
        <Vazio>{viagens.length ? 'Nenhuma viagem bate com a busca.' : 'Nenhuma viagem à venda no momento.'}</Vazio>
      ) : (
        <Grade>
          {exibidas.map((v) => (
            <ViagemCard key={v.id} viagem={v} />
          ))}
        </Grade>
      )}
    </>
  );
}
