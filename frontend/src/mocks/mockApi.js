/**
 * API falsa que roda no navegador (localStorage).
 * Cada função devolve exatamente o formato que a API em C# deve devolver — veja API.md.
 *
 * Usuários de teste:
 *   admin@teste.com      / 123456  (Admin)
 *   passageiro@teste.com / 123456  (Passageiro)
 */
import { lerToken } from '../utils/armazenamento';
import { obterStrategy, listarStrategies } from './pagamentoStrategies';

const CHAVE = 'excursoes_mock_db';
const espera = (ms = 250) => new Promise((r) => setTimeout(r, ms));

// ---------- dados iniciais ----------
function seed() {
  return {
    proximoId: 100,
    usuarios: [
      { id: 1, nome: 'Admin Excursões', email: 'admin@teste.com', senha: '123456', tipo: 'Admin' },
      { id: 2, nome: 'Maria Passageira', email: 'passageiro@teste.com', senha: '123456', tipo: 'Passageiro' },
    ],
    viagens: [
      {
        id: 1,
        adminId: 1,
        titulo: 'Chapada Diamantina',
        destino: 'Lençóis, BA',
        descricao:
          'Quatro dias entre cachoeiras, grutas e o pôr do sol no Morro do Pai Inácio.\n\nInclui: transporte ida e volta, pousada com café da manhã, guia credenciado e seguro viagem.',
        dataSaida: '2026-12-04',
        dataRetorno: '2026-12-08',
        valor: 1890,
        vagas: 40,
        maxParcelas: 10,
        imagens: [
          { id: 1, url: 'https://picsum.photos/seed/chapada1/1200/750' },
          { id: 2, url: 'https://picsum.photos/seed/chapada2/1200/750' },
          { id: 3, url: 'https://picsum.photos/seed/chapada3/1200/750' },
        ],
      },
      {
        id: 2,
        adminId: 1,
        titulo: 'Bonito e Rio da Prata',
        destino: 'Bonito, MS',
        descricao:
          'Flutuação em rios de água cristalina, Gruta do Lago Azul e Buraco das Araras.\n\nInclui: ônibus leito, hotel, 3 passeios e guia.',
        dataSaida: '2027-01-15',
        dataRetorno: '2027-01-20',
        valor: 2650,
        vagas: 30,
        maxParcelas: 12,
        imagens: [
          { id: 4, url: 'https://picsum.photos/seed/bonito1/1200/750' },
          { id: 5, url: 'https://picsum.photos/seed/bonito2/1200/750' },
        ],
      },
      {
        id: 3,
        adminId: 1,
        titulo: 'Jalapão 4x4',
        destino: 'Mateiros, TO',
        descricao: 'Dunas, fervedouros e a Cachoeira da Formiga em veículos 4x4.\n\nInclui: traslados, hospedagem e refeições.',
        dataSaida: '2027-02-10',
        dataRetorno: '2027-02-14',
        valor: 2200,
        vagas: 16,
        maxParcelas: 6,
        imagens: [{ id: 6, url: 'https://picsum.photos/seed/jalapao1/1200/750' }],
      },
    ],
    compras: [],
  };
}

function lerDb() {
  try {
    const bruto = localStorage.getItem(CHAVE);
    if (bruto) return JSON.parse(bruto);
  } catch {
    /* recria */
  }
  const db = seed();
  salvarDb(db);
  return db;
}

function salvarDb(db) {
  try {
    localStorage.setItem(CHAVE, JSON.stringify(db));
  } catch {
    throw new Error('Armazenamento do navegador cheio (imagens grandes demais no modo simulado).');
  }
}

const novoId = (db) => ++db.proximoId;

// ---------- "JWT" falso ----------
function gerarToken(usuario) {
  return 'mock.' + btoa(JSON.stringify({ id: usuario.id, tipo: usuario.tipo }));
}

function usuarioLogado(db, ...tiposPermitidos) {
  const token = lerToken();
  if (!token?.startsWith('mock.')) throw new Error('Faça login para continuar.');
  const { id } = JSON.parse(atob(token.slice(5)));
  const usuario = db.usuarios.find((u) => u.id === id);
  if (!usuario) throw new Error('Sessão inválida.');
  if (tiposPermitidos.length && !tiposPermitidos.includes(usuario.tipo)) {
    throw new Error('Sem permissão para esta ação.'); // equivale ao 403
  }
  return usuario;
}

const publico = ({ senha, ...resto }) => resto;

// ---------- helpers ----------
function vagasDisponiveis(db, viagem) {
  const vendidas = db.compras.filter((c) => c.viagemId === viagem.id && c.status !== 'cancelado').length;
  return viagem.vagas - vendidas;
}

const viagemDto = (db, v) => ({ ...v, vagasDisponiveis: vagasDisponiveis(db, v) });

function arquivoParaDataUrl(arquivo) {
  return new Promise((resolve, reject) => {
    const leitor = new FileReader();
    leitor.onload = () => resolve(leitor.result);
    leitor.onerror = reject;
    leitor.readAsDataURL(arquivo);
  });
}

function normalizarViagem(dados) {
  const v = {
    titulo: String(dados.titulo || '').trim(),
    destino: String(dados.destino || '').trim(),
    descricao: String(dados.descricao || ''),
    dataSaida: dados.dataSaida,
    dataRetorno: dados.dataRetorno,
    valor: Number(dados.valor),
    vagas: Number(dados.vagas),
    maxParcelas: Number(dados.maxParcelas),
  };
  if (!v.titulo) throw new Error('Informe o título.');
  if (!(v.valor > 0)) throw new Error('Valor deve ser maior que zero.');
  if (!(v.vagas > 0)) throw new Error('Informe a quantidade de vagas.');
  if (!(v.maxParcelas >= 1 && v.maxParcelas <= 12)) throw new Error('Parcelas máximas entre 1 e 12.');
  if (v.dataSaida && v.dataRetorno && v.dataRetorno < v.dataSaida)
    throw new Error('A volta não pode ser antes da ida.');
  return v;
}

// ================= AUTH =================
export async function login(email, senha) {
  await espera();
  const db = lerDb();
  const usuario = db.usuarios.find((u) => u.email === email.trim().toLowerCase() && u.senha === senha);
  if (!usuario) throw new Error('E-mail ou senha incorretos.');
  return { token: gerarToken(usuario), usuario: publico(usuario) };
}

export async function cadastrar({ nome, email, senha }) {
  await espera();
  const db = lerDb();
  const emailNorm = email.trim().toLowerCase();
  if (db.usuarios.some((u) => u.email === emailNorm)) throw new Error('E-mail já cadastrado.');
  const usuario = { id: novoId(db), nome: nome.trim(), email: emailNorm, senha, tipo: 'Passageiro' };
  db.usuarios.push(usuario);
  salvarDb(db);
  return { token: gerarToken(usuario), usuario: publico(usuario) };
}

// ================= VIAGENS =================
export async function listarViagens() {
  await espera();
  const db = lerDb();
  return db.viagens.map((v) => viagemDto(db, v));
}

export async function buscarViagem(id) {
  await espera();
  const db = lerDb();
  const v = db.viagens.find((x) => x.id === Number(id));
  if (!v) throw new Error('Viagem não encontrada.');
  return viagemDto(db, v);
}

export async function criarViagem(dados, novasImagens = []) {
  await espera();
  const db = lerDb();
  const admin = usuarioLogado(db, 'Admin');
  const viagem = { id: novoId(db), adminId: admin.id, ...normalizarViagem(dados), imagens: [] };
  for (const arq of novasImagens) {
    viagem.imagens.push({ id: novoId(db), url: await arquivoParaDataUrl(arq) });
  }
  db.viagens.push(viagem);
  salvarDb(db);
  return viagemDto(db, viagem);
}

export async function atualizarViagem(id, dados, novasImagens = [], imagensRemover = []) {
  await espera();
  const db = lerDb();
  const admin = usuarioLogado(db, 'Admin');
  const viagem = db.viagens.find((v) => v.id === Number(id));
  if (!viagem) throw new Error('Viagem não encontrada.');
  if (viagem.adminId !== admin.id) throw new Error('Você só pode editar suas viagens.');

  Object.assign(viagem, normalizarViagem(dados));
  viagem.imagens = viagem.imagens.filter((img) => !imagensRemover.includes(img.id));
  for (const arq of novasImagens) {
    viagem.imagens.push({ id: novoId(db), url: await arquivoParaDataUrl(arq) });
  }
  salvarDb(db);
  return viagemDto(db, viagem);
}

export async function excluirViagem(id) {
  await espera();
  const db = lerDb();
  const admin = usuarioLogado(db, 'Admin');
  const viagem = db.viagens.find((v) => v.id === Number(id));
  if (!viagem) throw new Error('Viagem não encontrada.');
  if (viagem.adminId !== admin.id) throw new Error('Você só pode excluir suas viagens.');
  if (db.compras.some((c) => c.viagemId === viagem.id && c.status !== 'cancelado'))
    throw new Error('Não é possível excluir: já existem passagens vendidas.');
  db.viagens = db.viagens.filter((v) => v.id !== viagem.id);
  salvarDb(db);
  return null;
}

export async function passageirosDaViagem(id) {
  await espera();
  const db = lerDb();
  const admin = usuarioLogado(db, 'Admin');
  const viagem = db.viagens.find((v) => v.id === Number(id));
  if (!viagem || viagem.adminId !== admin.id) throw new Error('Viagem não encontrada.');

  return db.compras
    .filter((c) => c.viagemId === viagem.id)
    .map((c) => {
      const p = db.usuarios.find((u) => u.id === c.usuarioId);
      const pagas = c.parcelas.filter((x) => x.status === 'paga');
      return {
        compraId: c.id,
        passageiro: { id: p.id, nome: p.nome, email: p.email },
        metodo: c.metodo,
        numParcelas: c.numParcelas,
        parcelasPagas: pagas.length,
        valorTotal: c.valorTotal,
        valorPago: Math.round(pagas.reduce((s, x) => s + x.valor, 0) * 100) / 100,
        status: c.status,
        criadoEm: c.criadoEm,
      };
    });
}

// ================= PAGAMENTOS =================
export async function metodosPagamento() {
  await espera(100);
  return listarStrategies();
}

export async function simular(viagemId, metodo, numParcelas) {
  await espera(150);
  const db = lerDb();
  const viagem = db.viagens.find((v) => v.id === Number(viagemId));
  if (!viagem) throw new Error('Viagem não encontrada.');

  const strategy = obterStrategy(metodo);
  const n = Number(numParcelas);
  strategy.validar(n, viagem);
  const { total, parcelas } = strategy.gerarParcelas(viagem.valor, n);
  return { metodo, numParcelas: n, maxParcelas: strategy.maxParcelas(viagem), total, parcelas };
}

export async function comprar(viagemId, metodo, numParcelas) {
  await espera();
  const db = lerDb();
  const passageiro = usuarioLogado(db, 'Passageiro');
  const viagem = db.viagens.find((v) => v.id === Number(viagemId));
  if (!viagem) throw new Error('Viagem não encontrada.');
  if (vagasDisponiveis(db, viagem) <= 0) throw new Error('Viagem esgotada.');
  if (db.compras.some((c) => c.viagemId === viagem.id && c.usuarioId === passageiro.id && c.status !== 'cancelado'))
    throw new Error('Você já comprou esta viagem.');

  const strategy = obterStrategy(metodo);
  const n = Number(numParcelas);
  strategy.validar(n, viagem);
  const { total, parcelas } = strategy.gerarParcelas(viagem.valor, n);

  const compra = {
    id: novoId(db),
    usuarioId: passageiro.id,
    viagemId: viagem.id,
    metodo,
    numParcelas: n,
    valorTotal: total,
    status: 'pendente',
    criadoEm: new Date().toISOString(),
    parcelas: parcelas.map((p) => ({ id: novoId(db), ...p, status: 'aberta' })),
  };
  db.compras.push(compra);
  salvarDb(db);
  return compraDto(db, compra);
}

function compraDto(db, c) {
  const v = db.viagens.find((x) => x.id === c.viagemId);
  return {
    id: c.id,
    metodo: c.metodo,
    numParcelas: c.numParcelas,
    valorTotal: c.valorTotal,
    status: c.status,
    criadoEm: c.criadoEm,
    viagem: v
      ? { id: v.id, titulo: v.titulo, destino: v.destino, dataSaida: v.dataSaida, imagem: v.imagens[0]?.url ?? null }
      : null,
    parcelas: c.parcelas,
  };
}

export async function minhasCompras() {
  await espera();
  const db = lerDb();
  const passageiro = usuarioLogado(db, 'Passageiro');
  return db.compras
    .filter((c) => c.usuarioId === passageiro.id)
    .sort((a, b) => b.criadoEm.localeCompare(a.criadoEm))
    .map((c) => compraDto(db, c));
}

export async function pagarParcela(parcelaId) {
  await espera();
  const db = lerDb();
  const passageiro = usuarioLogado(db, 'Passageiro');
  const compra = db.compras.find(
    (c) => c.usuarioId === passageiro.id && c.parcelas.some((p) => p.id === Number(parcelaId)),
  );
  if (!compra) throw new Error('Parcela não encontrada.');

  const parcela = compra.parcelas.find((p) => p.id === Number(parcelaId));
  const anteriorAberta = compra.parcelas.find((p) => p.numero < parcela.numero && p.status === 'aberta');
  if (anteriorAberta) throw new Error(`Pague antes a parcela ${anteriorAberta.numero}.`);
  if (parcela.status === 'paga') throw new Error('Parcela já paga.');

  parcela.status = 'paga';
  parcela.pagoEm = new Date().toISOString();
  if (compra.parcelas.every((p) => p.status === 'paga')) compra.status = 'pago';
  salvarDb(db);
  return parcela;
}

// Útil para testes: apaga tudo e volta ao seed
export function resetarMock() {
  try {
    localStorage.removeItem(CHAVE);
  } catch {
    /* nada */
  }
}
