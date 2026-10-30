using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Models;

public class Admin : Usuario
{
    // Lista privada; de fora só dá para LER (ninguém faz admin.Viagens.Clear())
    private readonly List<Viagem> _viagens = new();
    public IReadOnlyCollection<Viagem> Viagens => _viagens;

    public override string Papel => "Admin";

    private Admin() { } // EF Core

    public Admin(string nome, string email, string senhaHash)
        : base(nome, email, senhaHash) { }

    // Só um admin cria viagem, e ela já nasce ligada a ele
    public Viagem PublicarViagem(DadosViagem dados)
    {
        var viagem = new Viagem(this, dados);
        _viagens.Add(viagem);
        return viagem;
    }

    public void EditarViagem(Viagem viagem, DadosViagem dados)
    {
        GarantirQueEhDono(viagem);
        viagem.Atualizar(dados);
    }

    // Valida a exclusão; quem apaga do banco é o repositório
    public void AutorizarExclusao(Viagem viagem)
    {
        GarantirQueEhDono(viagem);
        if (!viagem.PodeExcluir)
            throw new RegraNegocioException("Não é possível excluir: já existem passagens vendidas.");
    }

    public bool EhDono(Viagem viagem) => viagem.AdminId == Id;

    // Viagem de outro admin responde "não encontrada" (404), não "proibido" (403),
    // para não confirmar que ela existe
    private void GarantirQueEhDono(Viagem viagem)
    {
        if (!EhDono(viagem))
            throw new NaoEncontradoException("Viagem não encontrada.");
    }
}