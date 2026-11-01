import { useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';
import * as viagemService from '../services/viagemService';
import ViagemCard from '../components/ViagemCard';
import { Carregando, Erro, Vazio } from '../components/Estado';
import { Entrada, Grade } from '../components/ui.js';

const Hero = styled.section`
  padding: 24px 0 32px;

  ${({ theme }) => theme.midia.mobile} { padding: 8px 0 20px; }

  h1 {
    font-size: clamp(30px, 5vw, 48px);
    max-width: 14ch;
    margin-bottom: 20px;
  }
`;

const Sobre = styled.p`
  color: ${({ theme }) => theme.cores.destaque};
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-size: 12px;
  margin: 0 0 8px;
`;

const Busca = styled(Entrada)`
  display: block;
  max-width: 460px;
  padding: 12px 16px;
  border-radius: 999px;
`;

export default function Viagens() {
  const [viagens, setViagens] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [busca, setBusca] = useState('');

  useEffect(() => {
    viagemService
      .listar()
      .then(setViagens)
      .catch((e) => setErro(e.message))
      .finally(() => setCarregando(false));
  }, []);

  const filtradas = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    if (!termo) return viagens;
    return viagens.filter((v) => `${v.titulo} ${v.destino}`.toLowerCase().includes(termo));
  }, [viagens, busca]);

  return (
    <>
      <Hero>
        <Sobre>Excursões com parcelamento facilitado</Sobre>
        <h1>Para onde vamos na próxima?</h1>
        <Busca
          type="search"
          placeholder="Buscar por destino ou nome da viagem"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
        />
      </Hero>

      <Erro mensagem={erro} />
      {carregando ? (
        <Carregando texto="Buscando viagens…" />
      ) : filtradas.length === 0 ? (
        <Vazio>Nenhuma viagem encontrada.</Vazio>
      ) : (
        <Grade>
          {filtradas.map((v) => (
            <ViagemCard key={v.id} viagem={v} />
          ))}
        </Grade>
      )}
    </>
  );
}
