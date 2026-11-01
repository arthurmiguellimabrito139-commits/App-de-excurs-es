using System.Security.Claims;
using System.Text;
using Excursoes.Api.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Excursoes.Api.Services;

// Configurações do JWT (seção "Jwt" do appsettings + a chave nos secrets)
public class JwtOpcoes
{
    public const int TamanhoMinimoChave = 32;

    public string Chave { get; set; } = "";
    public string Emissor { get; set; } = "";
    public string Audiencia { get; set; } = "";
    public int ExpiracaoMinutos { get; set; } = 60;

    public static JwtOpcoes Ler(IConfiguration configuracao)
    {
        JwtOpcoes opcoes = new JwtOpcoes();
        configuracao.GetSection("Jwt").Bind(opcoes);

        // Falha logo ao iniciar se a chave for fraca ou estiver faltando
        if (opcoes.Chave.Length < TamanhoMinimoChave)
            throw new InvalidOperationException(
                "Jwt:Chave ausente ou com menos de " + TamanhoMinimoChave + " caracteres. " +
                "Use: dotnet user-secrets set \"Jwt:Chave\" \"<texto aleatório longo>\"");
        if (string.IsNullOrWhiteSpace(opcoes.Emissor) || string.IsNullOrWhiteSpace(opcoes.Audiencia))
            throw new InvalidOperationException("Jwt:Emissor e Jwt:Audiencia precisam estar configurados.");
        if (opcoes.ExpiracaoMinutos < 1)
            throw new InvalidOperationException("Jwt:ExpiracaoMinutos inválido.");

        return opcoes;
    }

    public SymmetricSecurityKey ChaveDeAssinatura()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Chave));
    }
}

public interface ITokenService
{
    string Gerar(Usuario usuario);
}

public class TokenService : ITokenService
{
    public const string ClaimId = "id";
    public const string ClaimPapel = "role";
    public const string ClaimNome = "nome";

    private readonly JwtOpcoes _opcoes;

    public TokenService(JwtOpcoes opcoes)
    {
        _opcoes = opcoes;
    }

    public string Gerar(Usuario usuario)
    {
        Claim[] claims = new Claim[]
        {
            new Claim(ClaimId, usuario.Id.ToString()),
            new Claim(ClaimPapel, usuario.Papel),
            new Claim(ClaimNome, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        SecurityTokenDescriptor descricao = new SecurityTokenDescriptor();
        descricao.Subject = new ClaimsIdentity(claims);
        descricao.Issuer = _opcoes.Emissor;
        descricao.Audience = _opcoes.Audiencia;
        descricao.Expires = DateTime.UtcNow.AddMinutes(_opcoes.ExpiracaoMinutos);
        descricao.SigningCredentials = new SigningCredentials(_opcoes.ChaveDeAssinatura(), SecurityAlgorithms.HmacSha256);

        JsonWebTokenHandler gerador = new JsonWebTokenHandler();
        return gerador.CreateToken(descricao);
    }
}
