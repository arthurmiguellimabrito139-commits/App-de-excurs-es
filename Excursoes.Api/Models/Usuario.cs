using System.Net.Mail;
using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Models;

public abstract class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string SenhaHash { get; private set; } = "";
    public DateTime CriadoEm { get; private set; }

    // Cada filho diz qual é o seu papel. Vai para a claim "role" do JWT
    // e é o texto usado em [Authorize(Roles = "...")].
    public abstract string Papel { get; }

    // Construtor vazio só para o EF Core montar o objeto vindo do banco
    protected Usuario() { }

    // Só os filhos chamam (protected): ninguém faz "new Usuario(...)"
    protected Usuario(string nome, string email, string senhaHash)
    {
        DefinirNome(nome);
        DefinirEmail(email);
        AlterarSenha(senhaHash);
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarPerfil(string nome, string email)
    {
        DefinirNome(nome);
        DefinirEmail(email);
    }

    // Recebe a senha JÁ criptografada (o BCrypt roda no AuthService)
    public void AlterarSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new RegraNegocioException("Senha inválida.");
        SenhaHash = novoHash;
    }

    private void DefinirNome(string nome)
    {
        nome = nome?.Trim() ?? "";
        if (nome.Length < 2 || nome.Length > 100)
            throw new RegraNegocioException("O nome deve ter entre 2 e 100 caracteres.");
        Nome = nome;
    }

    private void DefinirEmail(string email)
    {
        email = email?.Trim().ToLowerInvariant() ?? "";
        if (email.Length > 150 || !MailAddress.TryCreate(email, out _))
            throw new RegraNegocioException("E-mail inválido.");
        Email = email;
    }
}