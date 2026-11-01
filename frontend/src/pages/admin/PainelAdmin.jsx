import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import styled from 'styled-components';
import * as viagemService from '../../services/viagemService';
import { useAuth } from '../../context/AuthContext';
import { Carregando, Erro, Vazio } from '../../components/Estado';
import {
  Acoes, Botao, Estat, Estatisticas, Tabela, TabelaRolavel, TituloComAcao, TituloPagina,
} from '../../components/ui.js';
import { moeda, data } from '../../utils/formato';

const ViagemLink = styled(Link)`
  display: flex;
  align-items: center;
  gap: 10px;
  color: inherit;
  text-decoration: none;

  ${({ theme }) => theme.midia.mobile} {
    padding-bottom: 6px;
    strong { font-size: 16px; }
  }

  img {
    flex-shrink: 0;
    width: 56px;
    height: 40px;
    object-fit: cover;
    border-radius: 6px;
  }
  span {
    display: flex;
    flex-direction: column;
  }
`;

// Desktop: botões numa linha à direita. Celular: três botões dividindo a largura do card.
const AcoesLinha = styled(Acoes)`
  flex-wrap: nowrap;

  ${({ theme }) => theme.midia.mobile} {
    justify-content: stretch;
    padding-top: 8px;
    & > * { flex: 1; }
  }
`;

export default function PainelAdmin() {
  const { usuario } = useAuth();
  const [viagens, setViagens] = useState(null);
  const [erro, setErro] = useState('');

  const carregar = () =>
    viagemService
      .listar()
      .then((lista) => setViagens(lista.filter((v) => v.adminId === usuario.id)))
      .catch((e) => setErro(e.message));

  useEffect(() => {
    carregar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const excluir = async (v) => {
    if (!window.confirm(`Excluir "${v.titulo}"? Essa ação não pode ser desfeita.`)) return;
    setErro('');
    try {
      await viagemService.excluir(v.id);
      await carregar();
    } catch (e) {
      setErro(e.message);
    }
  };

  if (!viagens && !erro) return <Carregando />;

  const vendidas = (viagens ?? []).reduce((s, v) => s + (v.vagas - v.vagasDisponiveis), 0);
  const receitaPrevista = (viagens ?? []).reduce((s, v) => s + (v.vagas - v.vagasDisponiveis) * v.valor, 0);

  return (
    <>
      <TituloComAcao>
        <TituloPagina>Painel do administrador</TituloPagina>
        <Botao as={Link} to="/admin/viagens/nova" $variante="primario">
          + Nova viagem
        </Botao>
      </TituloComAcao>

      <Estatisticas>
        <Estat>
          <span>Viagens publicadas</span>
          <strong>{viagens?.length ?? 0}</strong>
        </Estat>
        <Estat>
          <span>Passagens vendidas</span>
          <strong>{vendidas}</strong>
        </Estat>
        <Estat>
          <span>Receita bruta (preço base)</span>
          <strong>{moeda(receitaPrevista)}</strong>
        </Estat>
      </Estatisticas>

      <Erro mensagem={erro} />

      {viagens?.length === 0 ? (
        <Vazio>
          Nenhuma viagem publicada. <Link to="/admin/viagens/nova">Publicar a primeira</Link>
        </Vazio>
      ) : (
        <TabelaRolavel $empilhar>
          <Tabela $empilhar>
            <thead>
              <tr>
                <th>Viagem</th>
                <th>Saída</th>
                <th>Valor</th>
                <th>Vendidas</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {viagens?.map((v) => (
                <tr key={v.id}>
                  <td data-destaque>
                    <ViagemLink to={`/viagens/${v.id}`}>
                      {v.imagens[0] && <img src={v.imagens[0].url} alt="" />}
                      <span>
                        <strong>{v.titulo}</strong>
                        <small>{v.destino}</small>
                      </span>
                    </ViagemLink>
                  </td>
                  <td data-rotulo="Saída">{data(v.dataSaida)}</td>
                  <td data-rotulo="Valor">
                    <span>
                      {moeda(v.valor)}
                      <small> até {v.maxParcelas}x</small>
                    </span>
                  </td>
                  <td data-rotulo="Vendidas">
                    {v.vagas - v.vagasDisponiveis}/{v.vagas}
                  </td>
                  <td data-destaque>
                    <AcoesLinha $fim $semMargem>
                      <Botao as={Link} to={`/admin/viagens/${v.id}/passageiros`} $tamanho="pequeno">
                        Passageiros
                      </Botao>
                      <Botao as={Link} to={`/admin/viagens/${v.id}/editar`} $tamanho="pequeno">
                        Editar
                      </Botao>
                      <Botao $variante="perigo" $tamanho="pequeno" onClick={() => excluir(v)}>
                        Excluir
                      </Botao>
                    </AcoesLinha>
                  </td>
                </tr>
              ))}
            </tbody>
          </Tabela>
        </TabelaRolavel>
      )}
    </>
  );
}
