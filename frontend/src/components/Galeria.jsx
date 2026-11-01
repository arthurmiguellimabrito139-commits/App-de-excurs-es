import { useRef, useState } from 'react';
import styled from 'styled-components';

const Principal = styled.div`
  position: relative;
  aspect-ratio: 16 / 9;
  border-radius: ${({ theme }) => theme.raio};
  overflow: hidden;
  background: ${({ theme }) => theme.cores.primariaClara};
  display: grid;
  place-items: center;
  color: ${({ theme }) => theme.cores.textoSuave};
  touch-action: pan-y; /* deixa rolar a página na vertical; o deslize lateral troca a foto */
  user-select: none;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    pointer-events: none;
  }

  /* no celular a foto vai de ponta a ponta e fica um pouco mais alta */
  ${({ theme }) => theme.midia.mobile} {
    aspect-ratio: 4 / 3;
    border-radius: 0;
    margin: 0 -16px;
  }
`;

const Seta = styled.button`
  position: absolute;
  top: 50%;
  ${({ $lado }) => $lado}: 12px;
  transform: translateY(-50%);
  width: 40px;
  height: 40px;
  border: 0;
  border-radius: 50%;
  background: rgba(255, 253, 249, 0.9);
  color: ${({ theme }) => theme.cores.texto};
  font-size: 20px;
  line-height: 1;
  cursor: pointer;
  box-shadow: ${({ theme }) => theme.sombra};
  opacity: 0;
  transition: opacity 0.15s;

  ${Principal}:hover &,
  &:focus-visible { opacity: 1; }

  /* no toque usa o deslize, as setas atrapalham */
  ${({ theme }) => theme.midia.toque} { display: none; }
`;

const Contador = styled.span`
  position: absolute;
  right: 12px;
  bottom: 12px;
  background: rgba(31, 42, 43, 0.7);
  color: #fff;
  font-size: 12px;
  font-weight: 600;
  padding: 3px 10px;
  border-radius: 999px;
`;

const Miniaturas = styled.div`
  display: flex;
  gap: 8px;
  margin-top: 10px;
  overflow-x: auto;
  scrollbar-width: none;
  &::-webkit-scrollbar { display: none; }

  ${({ theme }) => theme.midia.mobile} { display: none; }
`;

const Miniatura = styled.button`
  padding: 0;
  border: 2px solid ${({ theme, $ativa }) => ($ativa ? theme.cores.destaque : 'transparent')};
  opacity: ${({ $ativa }) => ($ativa ? 1 : 0.7)};
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  flex: 0 0 88px;
  height: 58px;
  background: none;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }
`;

// Bolinhas no lugar das miniaturas, só no celular
const Pontos = styled.div`
  display: none;
  justify-content: center;
  gap: 6px;
  margin-top: 10px;

  ${({ theme }) => theme.midia.mobile} { display: flex; }
`;

const Ponto = styled.button`
  width: ${({ $ativo }) => ($ativo ? '18px' : '8px')};
  height: 8px;
  padding: 0;
  border: 0;
  border-radius: 999px;
  background: ${({ theme, $ativo }) => ($ativo ? theme.cores.destaque : theme.cores.borda)};
  transition: width 0.2s;
  cursor: pointer;
`;

export default function Galeria({ imagens = [], titulo }) {
  const [atual, setAtual] = useState(0);
  const toqueInicio = useRef(null);

  if (!imagens.length) return <Principal>Sem imagens</Principal>;

  const total = imagens.length;
  const indice = Math.min(atual, total - 1);
  const ir = (n) => setAtual((n + total) % total);

  // Deslizar o dedo para os lados troca a foto
  const aoTocar = (e) => {
    toqueInicio.current = e.touches[0].clientX;
  };
  const aoSoltar = (e) => {
    if (toqueInicio.current == null) return;
    const dx = e.changedTouches[0].clientX - toqueInicio.current;
    if (Math.abs(dx) > 40) ir(indice + (dx < 0 ? 1 : -1));
    toqueInicio.current = null;
  };

  return (
    <div>
      <Principal onTouchStart={aoTocar} onTouchEnd={aoSoltar}>
        <img src={imagens[indice].url} alt={`${titulo} — foto ${indice + 1} de ${total}`} />
        {total > 1 && (
          <>
            <Seta $lado="left" onClick={() => ir(indice - 1)} aria-label="Foto anterior">
              ‹
            </Seta>
            <Seta $lado="right" onClick={() => ir(indice + 1)} aria-label="Próxima foto">
              ›
            </Seta>
            <Contador>
              {indice + 1}/{total}
            </Contador>
          </>
        )}
      </Principal>

      {total > 1 && (
        <>
          <Miniaturas>
            {imagens.map((img, i) => (
              <Miniatura key={img.id} $ativa={i === indice} onClick={() => setAtual(i)} aria-label={`Ver foto ${i + 1}`}>
                <img src={img.url} alt="" />
              </Miniatura>
            ))}
          </Miniaturas>
          <Pontos>
            {imagens.map((img, i) => (
              <Ponto key={img.id} $ativo={i === indice} onClick={() => setAtual(i)} aria-label={`Ver foto ${i + 1}`} />
            ))}
          </Pontos>
        </>
      )}
    </div>
  );
}
