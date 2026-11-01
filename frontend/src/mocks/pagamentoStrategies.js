/**
 * Espelho em JS do Strategy que vai existir no back em C#.
 * Só é usado no modo simulado — com a API ligada, quem calcula é o C#.
 * Mantenha as regras iguais às de Strategies/Pagamento/*.cs
 */

const arred = (v) => Math.round(v * 100) / 100;

class PagamentoStrategy {
  constructor(metodo, nome, descricao) {
    this.metodo = metodo;
    this.nome = nome;
    this.descricao = descricao;
  }

  maxParcelas(viagem) {
    return viagem.maxParcelas;
  }

  calcularTotal() {
    throw new Error('calcularTotal não implementado');
  }

  validar(numParcelas, viagem) {
    const max = this.maxParcelas(viagem);
    if (!Number.isInteger(numParcelas) || numParcelas < 1 || numParcelas > max) {
      throw new Error(max === 1 ? `${this.nome} só aceita pagamento à vista.` : `${this.nome}: escolha entre 1 e ${max} parcelas.`);
    }
  }

  // A última parcela absorve a diferença de centavos
  gerarParcelas(valor, numParcelas) {
    const total = arred(this.calcularTotal(valor, numParcelas));
    const base = Math.floor((total / numParcelas) * 100) / 100;
    const hoje = new Date();

    const parcelas = Array.from({ length: numParcelas }, (_, i) => {
      // Mesmo dia nos meses seguintes; se o mês for mais curto (ex.: 31 → fev), usa o último dia
      const ano = hoje.getFullYear();
      const mes = hoje.getMonth() + i;
      const ultimoDia = new Date(ano, mes + 1, 0).getDate();
      const venc = new Date(Date.UTC(ano, mes, Math.min(hoje.getDate(), ultimoDia)));
      return {
        numero: i + 1,
        valor: i === numParcelas - 1 ? arred(total - base * (numParcelas - 1)) : base,
        vencimento: venc.toISOString().slice(0, 10),
      };
    });

    return { total, parcelas };
  }
}

class PixStrategy extends PagamentoStrategy {
  constructor() {
    super('pix', 'Pix', 'À vista com 5% de desconto');
  }
  maxParcelas() {
    return 1;
  }
  calcularTotal(valor) {
    return valor * 0.95;
  }
}

class CartaoStrategy extends PagamentoStrategy {
  constructor() {
    super('cartao', 'Cartão de crédito', 'Até 3x sem juros; acima disso, 2,5% ao mês');
  }
  maxParcelas(viagem) {
    return Math.min(12, viagem.maxParcelas);
  }
  calcularTotal(valor, numParcelas) {
    return numParcelas <= 3 ? valor : valor * Math.pow(1.025, numParcelas);
  }
}

class BoletoStrategy extends PagamentoStrategy {
  constructor() {
    super('boleto', 'Boleto', 'Parcelado sem juros, um boleto por mês');
  }
  calcularTotal(valor) {
    return valor;
  }
}

// Equivale ao "registro" das strategies no Program.cs
const estrategias = [new PixStrategy(), new CartaoStrategy(), new BoletoStrategy()];

export function obterStrategy(metodo) {
  const s = estrategias.find((e) => e.metodo === metodo);
  if (!s) throw new Error('Forma de pagamento inválida');
  return s;
}

export function listarStrategies() {
  return estrategias.map(({ metodo, nome, descricao }) => ({ metodo, nome, descricao }));
}
