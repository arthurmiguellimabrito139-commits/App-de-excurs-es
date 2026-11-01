using Excursoes.Api.DTOs.Auth;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Repositories;

namespace Excursoes.Api.Services;

public interface IAuthService
{
    Task<AuthRespostaDto> LoginAsync(LoginDto dto);
    Task<AuthRespostaDto> CadastrarAsync(CadastroDto dto);
    Task CriarAdminInicialAsync(string nome, string email, string senha);
}

public class AuthService : IAuthService
{
    // Mesma mensagem para "e-mail não existe" e "senha errada":
    // assim ninguém descobre quais e-mails estão cadastrados.
    private const string MensagemLoginInvalido = "E-mail ou senha incorretos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly ITokenService _tokens;
    private readonly int _custoBcrypt;
    private readonly string _hashFalso;

    public AuthService(IUsuarioRepository usuarios, ITokenService tokens, IConfiguration configuracao)
    {
        _usuarios = usuarios;
        _tokens = tokens;
        _custoBcrypt = configuracao.GetValue<int>("Seguranca:BcryptCusto", 12);
        _hashFalso = ObterHashFalso(_custoBcrypt);
    }

    public async Task<AuthRespostaDto> LoginAsync(LoginDto dto)
    {
        Usuario? usuario = await _usuarios.BuscarPorEmailAsync(dto.Email);

        // Mesmo sem usuário, roda o BCrypt com um hash falso: a resposta demora o mesmo tempo
        // nos dois casos, então não dá para descobrir e-mails cadastrados pelo tempo de resposta.
        string hash = _hashFalso;
        if (usuario != null)
            hash = usuario.SenhaHash;

        bool senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, hash);

        if (usuario == null || !senhaCorreta)
            throw new NaoAutorizadoException(MensagemLoginInvalido);

        return CriarResposta(usuario);
    }

    // Cadastro público: SEMPRE cria Passageiro
    public async Task<AuthRespostaDto> CadastrarAsync(CadastroDto dto)
    {
        if (await _usuarios.EmailExisteAsync(dto.Email))
            throw new RegraNegocioException("E-mail já cadastrado.");

        string hash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, _custoBcrypt);
        Passageiro passageiro = new Passageiro(dto.Nome, dto.Email, hash);
        await _usuarios.InserirAsync(passageiro);

        return CriarResposta(passageiro);
    }

    // Usado só na inicialização (SeedAdmin): o primeiro admin vem da configuração
    public async Task CriarAdminInicialAsync(string nome, string email, string senha)
    {
        if (senha.Length < 12)
            throw new InvalidOperationException("Admin:Senha precisa ter pelo menos 12 caracteres.");

        string hash = BCrypt.Net.BCrypt.HashPassword(senha, _custoBcrypt);
        Admin admin = new Admin(nome, email, hash);
        await _usuarios.InserirAsync(admin);
    }

    private AuthRespostaDto CriarResposta(Usuario usuario)
    {
        AuthRespostaDto resposta = new AuthRespostaDto();
        resposta.Token = _tokens.Gerar(usuario);
        resposta.Usuario = UsuarioDto.De(usuario);
        return resposta;
    }

    // Calculado uma vez só (BCrypt é lento de propósito)
    private static readonly object _trava = new object();
    private static string? _hashFalsoGuardado;
    private static int _custoGuardado;

    private static string ObterHashFalso(int custo)
    {
        lock (_trava)
        {
            if (_hashFalsoGuardado == null || _custoGuardado != custo)
            {
                _hashFalsoGuardado = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), custo);
                _custoGuardado = custo;
            }
            return _hashFalsoGuardado;
        }
    }
}
