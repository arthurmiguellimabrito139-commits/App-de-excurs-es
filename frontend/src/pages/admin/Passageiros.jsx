import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import styled from 'styled-components';
import * as viagemService from '../../services/viagemService';
import { Carregando, Erro, Vazio } from '../../components/Estado';
import {
  Estat, Estatisticas, Selo, Tabela, TabelaRolavel, TituloPagina, Voltar,
} from '../../components/ui.js';
import { moeda, data, NOMES_METODO } from '../../utils/formato';

const Email = styled.small`
  word-break: break-all;
`;

export default function Passageiros() {
  const { id } = useParams();
  const [viagem, setViagem] = useState(null);
  const [lista, setLista] = useState(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    Promise.all([viagemService.buscar(id), viagemService.passageiros(id)])
      .then(([v, p]) => {
        setViagem(v);
        setLista(p);
      })
      .catch((e) => setErro(e.message));
  }, [id]);

  if (erro) return <Erro mensagem={erro} />;
  if (!lista) return <Carregando />;

  const recebido = lista.reduce((s, p) => s + p.valorPago, 0);
  const aReceber = lista.reduce((s, p) => s + (p.valorTotal - p.valorPago), 0);

  return (
    <>
      <Voltar to="/admin">← Painel</Voltar>
      <TituloPagina>Passageiros · {viagem.titulo}</TituloPagina>

      <Estatisticas>
        <Estat>
          <span>Passageiros</span>
          <strong>
            {lista.length}/{viagem.vagas}
          </strong>
        </Estat>
        <Estat>
          <span>Recebido</span>
          <strong>{moeda(recebido)}</strong>
        </Estat>
        <Estat>
          <span>A receber</span>
          <strong>{moeda(aReceber)}</strong>
        </Estat>
      </Estatisticas>

      {lista.length === 0 ? (
        <Vazio>Nenhuma passagem vendida ainda.</Vazio>
      ) : (
        <TabelaRolavel $empilhar>
          <Tabela $empilhar>
            <thead>
              <tr>
                <th>Passageiro</th>
                <th>Compra</th>
                <th>Pagamento</th>
                <th>Parcelas</th>
                <th>Pago</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {lista.map((p) => (
                <tr key={p.compraId}>
                  <td data-destaque>
                    <strong>{p.passageiro.nome}</strong>
                    <br />
                    <Email>{p.passageiro.email}</Email>
                  </td>
                  <td data-rotulo="Compra">{data(p.criadoEm)}</td>
                  <td data-rotulo="Pagamento">{NOMES_METODO[p.metodo]}</td>
                  <td data-rotulo="Parcelas">
                    {p.parcelasPagas}/{p.numParcelas}
                  </td>
                  <td data-rotulo="Pago">
                    <span>
                      {moeda(p.valorPago)}
                      <small> de {moeda(p.valorTotal)}</small>
                    </span>
                  </td>
                  <td data-rotulo="Status">
                    <Selo $tipo={p.status === 'pago' ? 'ok' : 'aviso'}>
                      {p.status === 'pago' ? 'Quitado' : 'Pagando'}
                    </Selo>
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
