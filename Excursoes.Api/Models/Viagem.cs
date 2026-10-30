
using Excursoes.Api.Excecoes;
using Excursoes.Api.Strategies.Pagamento;

namespace Excursoes.Api.Models;

public class Viagem
{
    public const int MaxImagens = 10;

    private readonly List<ViagemImagem> _imagens = new();
    private readonly List<Compra> _compras = new();

    public int Id { get; private set; }
    public int AdminId { get; private set; }
    public Admin Admin { get; private set; } = null!;

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

    public IReadOnlyCollection<ViagemImagem> Imagens => _imagens;
    public IReadOnlyCollection<Compra> Compras => _compras;

    private Viagem() { } // EF Core

    // internal: só o Admin.PublicarViagem cria viagem
    internal Viagem(Admin admin, DadosViagem dados)
    {
        Admin = admin;
        AdminId = admin.Id;
        Atualizar(dados);
    }

    public void Atualizar(DadosViagem dados)
    {
        var titulo = dados.Titulo?.Trim() ?? "";
        var destino = dados.Destino?.Trim() ?? "";

        if (titulo.Length is < 3 or > 150) throw new RegraNegocioException("O título deve ter entre 3 e 150 caracteres.");
        if (destino.Length is < 2 or > 150) throw new RegraNegocioException("Informe o destino.");
        if ((dados.Descricao?.Length ?? 0) > 4000) throw new RegraNegocioException("Descrição muito longa.");
        if (dados.DataRetorno < dados.DataSaida) throw new RegraNegocioException("A volta não pode ser antes da ida.");
        if (dados.Valor <= 0) throw new RegraNegocioException("O valor deve ser maior que zero.");
        if (dados.MaxParcelas is < 1 or > 12) throw new RegraNegocioException("Parcelas máximas entre 1 e 12.");
        if (dados.Vagas < 1) throw new RegraNegocioException("Informe pelo menos 1 vaga.");
        if (dados.Vagas < VagasVendidas)
            throw new RegraNegocioException($"Já existem {VagasVendidas} passagens vendidas.");

        Titulo = titulo;
        Destino = destino;
        Descricao = dados.Descricao?.Trim() ?? "";
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
            throw new RegraNegocioException($"Máximo de {MaxImagens} imagens por viagem.");
        _imagens.Add(new ViagemImagem(url));
    }

    public void RemoverImagem(int imagemId) =>
        _imagens.RemoveAll(i => i.Id == imagemId);

    // ---------- Vagas e venda ----------
    public int VagasVendidas => _compras.Count(c => c.Ativa);
    public int VagasDisponiveis => Vagas - VagasVendidas;
    public bool PodeExcluir => VagasVendidas == 0;
    public bool EstaAVenda(DateOnly hoje) => VagasDisponiveis > 0 && DataSaida >= hoje;

    // Quem decide se dá para vender é a viagem; quem calcula as parcelas é a strategy
    internal Compra Vender(Passageiro passageiro, IPagamentoStrategy strategy, int numParcelas)
    {
        if (!EstaAVenda(DateOnly.FromDateTime(DateTime.Today)))
            throw new RegraNegocioException("Esta viagem não está mais à venda.");

        var parcelas = strategy.Calcular(this, numParcelas);
        var compra = new Compra(passageiro, this, strategy.Metodo, parcelas);
        _compras.Add(compra);
        Versao = Guid.NewGuid();
        return compra;
    }
}