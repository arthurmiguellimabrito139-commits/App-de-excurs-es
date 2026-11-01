import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import styled, { css } from 'styled-components';
import * as viagemService from '../../services/viagemService';
import { Carregando, Erro } from '../../components/Estado';
import {
  Acoes, AreaTexto, Botao, Campo, Cartao, Entrada, GradeCampos, Selecao, TituloPagina, Voltar,
} from '../../components/ui.js';

const VAZIO = {
  titulo: '',
  destino: '',
  descricao: '',
  dataSaida: '',
  dataRetorno: '',
  valor: '',
  vagas: '',
  maxParcelas: 1,
};

const Imagens = styled.fieldset`
  border: 0;
  padding: 0;
  margin: 8px 0 0;

  legend {
    font-size: 13px;
    font-weight: 600;
    color: ${({ theme }) => theme.cores.textoSuave};
    margin-bottom: 8px;
  }
`;

const GradeImagens = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 130px), 1fr));

  ${({ theme }) => theme.midia.celular} {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px;
  }
  gap: 12px;
`;

// $estado: 'salva' | 'removida' | 'nova'
const ItemImagem = styled.div`
  position: relative;
  aspect-ratio: 4 / 3;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid ${({ theme }) => theme.cores.borda};

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  button {
    position: absolute;
    bottom: 6px;
    right: 6px;
    border: 0;
    border-radius: 999px;
    padding: 4px 10px;
    font-size: 12px;
    font-weight: 600;
    background: rgba(0, 0, 0, 0.65);
    color: #fff;
    cursor: pointer;

    ${({ theme }) => theme.midia.toque} { padding: 8px 12px; }
  }

  ${({ $estado }) => $estado === 'removida' && css`
    img {
      opacity: 0.3;
      filter: grayscale(1);
    }
  `}

  ${({ $estado, theme }) => $estado === 'nova' && css`
    border: 2px solid ${theme.cores.primaria};
  `}
`;

const Adicionar = styled.label`
  display: grid;
  place-items: center;
  aspect-ratio: 4 / 3;
  border: 2px dashed ${({ theme }) => theme.cores.borda};
  border-radius: 10px;
  cursor: pointer;
  color: ${({ theme }) => theme.cores.primaria};
  font-weight: 600;

  &:hover { border-color: ${({ theme }) => theme.cores.primaria}; }
  input { display: none; }
`;

const RodapeForm = styled(Acoes)`
  margin-top: 16px;
`;

// Mesmo formulário para criar (/admin/viagens/nova) e editar (/admin/viagens/:id/editar)
export default function ViagemForm() {
  const { id } = useParams();
  const editando = Boolean(id);
  const navigate = useNavigate();

  const [form, setForm] = useState(VAZIO);
  const [imagensSalvas, setImagensSalvas] = useState([]); // [{id,url}]
  const [imagensRemover, setImagensRemover] = useState([]); // ids
  const [novasImagens, setNovasImagens] = useState([]); // File[]
  const [previas, setPrevias] = useState([]); // object URLs
  const [carregando, setCarregando] = useState(editando);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');

  useEffect(() => {
    if (!editando) return;
    viagemService
      .buscar(id)
      .then((v) => {
        setForm({
          titulo: v.titulo,
          destino: v.destino,
          descricao: v.descricao,
          dataSaida: v.dataSaida?.slice(0, 10) ?? '',
          dataRetorno: v.dataRetorno?.slice(0, 10) ?? '',
          valor: v.valor,
          vagas: v.vagas,
          maxParcelas: v.maxParcelas,
        });
        setImagensSalvas(v.imagens);
      })
      .catch((e) => setErro(e.message))
      .finally(() => setCarregando(false));
  }, [id, editando]);

  // Gera e limpa as prévias das imagens novas
  useEffect(() => {
    const urls = novasImagens.map((f) => URL.createObjectURL(f));
    setPrevias(urls);
    return () => urls.forEach(URL.revokeObjectURL);
  }, [novasImagens]);

  const mudar = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const adicionarArquivos = (e) => {
    const arquivos = Array.from(e.target.files).filter((f) => f.type.startsWith('image/'));
    const grandes = arquivos.filter((f) => f.size > 5 * 1024 * 1024);
    if (grandes.length) setErro('Cada imagem pode ter no máximo 5 MB.');
    setNovasImagens((atual) => [...atual, ...arquivos.filter((f) => f.size <= 5 * 1024 * 1024)]);
    e.target.value = '';
  };

  const removerNova = (i) => setNovasImagens((atual) => atual.filter((_, idx) => idx !== i));

  const alternarRemocao = (imgId) =>
    setImagensRemover((atual) => (atual.includes(imgId) ? atual.filter((x) => x !== imgId) : [...atual, imgId]));

  const enviar = async (e) => {
    e.preventDefault();
    setErro('');
    setSalvando(true);
    try {
      const salva = editando
        ? await viagemService.atualizar(id, form, novasImagens, imagensRemover)
        : await viagemService.criar(form, novasImagens);
      navigate(`/viagens/${salva.id}`);
    } catch (err) {
      setErro(err.message);
    } finally {
      setSalvando(false);
    }
  };

  if (carregando) return <Carregando />;

  return (
    <>
      <Voltar to="/admin">← Painel</Voltar>
      <TituloPagina>{editando ? 'Editar viagem' : 'Nova viagem'}</TituloPagina>

      <Cartao as="form" onSubmit={enviar}>
        <GradeCampos>
          <Campo $span={4}>
            <span>Título</span>
            <Entrada name="titulo" value={form.titulo} onChange={mudar} required maxLength={150} />
          </Campo>
          <Campo $span={2} $spanTablet={6}>
            <span>Destino</span>
            <Entrada name="destino" value={form.destino} onChange={mudar} placeholder="Cidade, UF" required />
          </Campo>

          <Campo $span={3}>
            <span>Data de ida</span>
            <Entrada name="dataSaida" type="date" value={form.dataSaida} onChange={mudar} required />
          </Campo>
          <Campo $span={3}>
            <span>Data de volta</span>
            <Entrada
              name="dataRetorno"
              type="date"
              value={form.dataRetorno}
              onChange={mudar}
              min={form.dataSaida}
              required
            />
          </Campo>
          <Campo $span={2}>
            <span>Valor da passagem (R$)</span>
            <Entrada name="valor" type="number" inputMode="decimal" min="1" step="0.01" value={form.valor} onChange={mudar} required />
          </Campo>
          <Campo $span={2}>
            <span>Vagas</span>
            <Entrada name="vagas" type="number" inputMode="numeric" min="1" value={form.vagas} onChange={mudar} required />
          </Campo>
          <Campo $span={2} $spanTablet={6}>
            <span>Parcelas máximas</span>
            <Selecao name="maxParcelas" value={form.maxParcelas} onChange={mudar}>
              {Array.from({ length: 12 }, (_, i) => i + 1).map((n) => (
                <option key={n} value={n}>
                  {n}x
                </option>
              ))}
            </Selecao>
          </Campo>

          <Campo>
            <span>Descrição</span>
            <AreaTexto
              name="descricao"
              rows={7}
              value={form.descricao}
              onChange={mudar}
              placeholder="Roteiro, o que está incluso, ponto de encontro…"
            />
          </Campo>
        </GradeCampos>

        <Imagens>
          <legend>Imagens</legend>
          <GradeImagens>
            {imagensSalvas.map((img) => {
              const marcada = imagensRemover.includes(img.id);
              return (
                <ItemImagem key={img.id} $estado={marcada ? 'removida' : 'salva'}>
                  <img src={img.url} alt="" />
                  <button type="button" onClick={() => alternarRemocao(img.id)}>
                    {marcada ? 'Desfazer' : 'Remover'}
                  </button>
                </ItemImagem>
              );
            })}
            {previas.map((url, i) => (
              <ItemImagem key={url} $estado="nova">
                <img src={url} alt="" />
                <button type="button" onClick={() => removerNova(i)}>
                  Remover
                </button>
              </ItemImagem>
            ))}
            <Adicionar>
              <input type="file" accept="image/*" multiple onChange={adicionarArquivos} />
              <span>+ Adicionar fotos</span>
            </Adicionar>
          </GradeImagens>
        </Imagens>

        <Erro mensagem={erro} />

        <RodapeForm $fim $empilharNoMobile>
          <Botao as={Link} to="/admin">
            Cancelar
          </Botao>
          <Botao type="submit" $variante="primario" disabled={salvando}>
            {salvando ? 'Salvando…' : editando ? 'Salvar alterações' : 'Publicar viagem'}
          </Botao>
        </RodapeForm>
      </Cartao>
    </>
  );
}
