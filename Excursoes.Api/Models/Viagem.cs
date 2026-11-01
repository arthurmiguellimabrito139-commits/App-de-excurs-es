using Excursoes.Api.Excecoes;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Models;

public class Viagem : Entidade
{
    public const int MaxImagens = 10;

    private readonly List<ViagemImagem> _imagens = new List<ViagemImagem>();
    private readonly List<Compra> _compras = new List<Compra>();

    public int AdminId { get; private set; }
    public string Titulo { get; private set; } = "";
    public string Destino { get; private set; } = "";
    public string Descricao { get; private set; } = "";
    public DateOnly DataSaida { get; private set; }
    public DateOnly DataRetorno { get; private set; }
    public decimal Valor { get; private set; }
    public int Vagas { get; private set; }
    public int MaxParcelas { get; private set; }

    // Muda a cada alteração; se duas compras salvarem ao mesmo tempo, uma é recusada
    public Guid Versao { get; private set; }

    public IReadOnlyCollection<ViagemImagem> Imagens
    {
        get { return _imagens; }
    }

    public IReadOnlyCollection<Compra> Compras
    {
        get { return _compras; }
    }

    private Viagem()
    {
    }

    // internal: só o Admin.PublicarViagem cria viagem
    internal Viagem(Admin admin, DadosViagem dados)
    {
        AdminId = admin.Id;
        Atualizar(dados);
    }

    public void Atualizar(DadosViagem dados)
    {
        string titulo = Limpar(dados.Titulo);
        string destino = Limpar(dados.Destino);
        string descricao = Limpar(dados.Descricao);

        if (titulo.Length < 3 || titulo.Length > 150)
            throw new RegraNegocioException("O título deve ter entre 3 e 150 caracteres.");
        if (destino.Length < 2 || destino.Length > 150)
            throw new RegraNegocioException("Informe o destino.");
        if (descricao.Length > 4000)
            throw new RegraNegocioException("Descrição muito longa (máximo 4000 caracteres).");
        if (dados.DataRetorno < dados.DataSaida)
            throw new RegraNegocioException("A volta não pode ser antes da ida.");
        if (dados.Valor <= 0)
            throw new RegraNegocioException("O valor deve ser maior que zero.");
        if (dados.MaxParcelas < 1 || dados.MaxParcelas > 12)
            throw new RegraNegocioException("Parcelas máximas entre 1 e 12.");
        if (dados.Vagas < 1)
            throw new RegraNegocioException("Informe pelo menos 1 vaga.");
        if (dados.Vagas < VagasVendidas)
            throw new RegraNegocioException("Já existem " + VagasVendidas + " passagens vendidas; as vagas não podem ser menores que isso.");

        Titulo = titulo;
        Destino = destino;
        Descricao = descricao;
        DataSaida = dados.DataSaida;
        DataRetorno = dados.DataRetorno;
        Valor = Math.Round(dados.Valor, 2);
        Vagas = dados.Vagas;
        MaxParcelas = dados.MaxParcelas;
        Versao = Guid.NewGuid();
    }

    // ---------- Imagens ----------
    public void AdicionarImagem(string url)
    {
        if (_imagens.Count >= MaxImagens)
            throw new RegraNegocioException("Máximo de " + MaxImagens + " imagens por viagem.");
        _imagens.Add(new ViagemImagem(url));
    }

    // Devolve o endereço da imagem removida (para apagar o arquivo) ou null se não existia
    public string? RemoverImagem(int imagemId)
    {
        for (int i = 0; i < _imagens.Count; i++)
        {
            if (_imagens[i].Id == imagemId)
            {
                string url = _imagens[i].Url;
                _imagens.RemoveAt(i);
                return url;
            }
        }
        return null;
    }

    // ---------- Vagas e venda ----------
    public int VagasVendidas
    {
        get
        {
            int quantidade = 0;
            foreach (Compra compra in _compras)
            {
                if (compra.Ativa)
                    quantidade++;
            }
            return quantidade;
        }
    }

    public int VagasDisponiveis
    {
        get { return Vagas - VagasVendidas; }
    }

    public bool PodeExcluir
    {
        get { return VagasVendidas == 0; }
    }

    public bool EstaAVenda(DateOnly hoje)
    {
        return VagasDisponiveis > 0 && DataSaida >= hoje;
    }

    // Quem decide se dá para vender é a viagem; quem calcula as parcelas é a strategy
    internal Compra Vender(Passageiro passageiro, IPagamentoStrategy strategy, int numParcelas)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        if (VagasDisponiveis <= 0)
            throw new RegraNegocioException("Viagem esgotada.");
        if (DataSaida < hoje)
            throw new RegraNegocioException("Esta viagem já saiu.");

        IReadOnlyList<ParcelaCalculada> parcelas = strategy.Calcular(this, numParcelas);
        Compra compra = new Compra(passageiro, this, strategy.Metodo, parcelas);
        _compras.Add(compra);
        Versao = Guid.NewGuid();
        return compra;
    }

    // ---------- Carregamento vindo do banco ----------
    internal void CarregarImagens(IEnumerable<ViagemImagem> imagens)
    {
        _imagens.Clear();
        _imagens.AddRange(imagens);
    }

    internal void CarregarCompras(IEnumerable<Compra> compras)
    {
        _compras.Clear();
        _compras.AddRange(compras);
    }

    private static string Limpar(string? texto)
    {
        if (texto == null)
            return "";
        return texto.Trim();
    }
}
