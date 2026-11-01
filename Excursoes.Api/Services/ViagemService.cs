using Excursoes.Api.DTOs.Pagamentos;
using Excursoes.Api.DTOs.Viagens;
using Excursoes.Api.Excecoes;
using Excursoes.Api.Models;
using Excursoes.Api.Repositories;

namespace Excursoes.Api.Services;

public interface IViagemService
{
    Task<List<ViagemDto>> ListarAsync(bool somenteAVenda);
    Task<ViagemDto> BuscarAsync(int id);
    Task<ViagemDto> CriarAsync(ViagemFormDto dto, int adminId);
    Task<ViagemDto> AtualizarAsync(int id, ViagemFormDto dto, int adminId);
    Task ExcluirAsync(int id, int adminId);
    Task<List<PassageiroCompraDto>> PassageirosAsync(int id, int adminId);
}

// O service coordena: busca os objetos, deixa o MODEL aplicar as regras
// e pede ao REPOSITORY para gravar. Permissão (quem pode) também fica aqui.
public class ViagemService : IViagemService
{
    private readonly IViagemRepository _viagens;
    private readonly IUsuarioRepository _usuarios;
    private readonly ICompraRepository _compras;
    private readonly IImagemStorage _imagens;
    private readonly IEnderecoPublico _endereco;

    public ViagemService(IViagemRepository viagens, IUsuarioRepository usuarios, ICompraRepository compras,
                         IImagemStorage imagens, IEnderecoPublico endereco)
    {
        _viagens = viagens;
        _usuarios = usuarios;
        _compras = compras;
        _imagens = imagens;
        _endereco = endereco;
    }

    public async Task<List<ViagemDto>> ListarAsync(bool somenteAVenda)
    {
        List<Viagem> viagens = await _viagens.ListarAsync();
        DateOnly hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        List<ViagemDto> resultado = new List<ViagemDto>();
        foreach (Viagem viagem in viagens)
        {
            if (somenteAVenda && !viagem.EstaAVenda(hoje))
                continue;
            resultado.Add(ViagemDto.De(viagem, _endereco));
        }
        return resultado;
    }

    public async Task<ViagemDto> BuscarAsync(int id)
    {
        Viagem viagem = await BuscarViagem(id);
        return ViagemDto.De(viagem, _endereco);
    }

    public async Task<ViagemDto> CriarAsync(ViagemFormDto dto, int adminId)
    {
        Admin admin = await BuscarAdmin(adminId);
        if (dto.Imagens.Count > Viagem.MaxImagens)
            throw new RegraNegocioException("Máximo de " + Viagem.MaxImagens + " imagens por viagem.");

        Viagem viagem = admin.PublicarViagem(dto.ParaDados());   // regras: model

        List<string> salvas = await SalvarImagens(dto.Imagens, viagem);
        try
        {
            await _viagens.InserirAsync(viagem);                     // SQL: repository
        }
        catch
        {
            ApagarArquivos(salvas);   // se o banco falhar, não deixa arquivo órfão
            throw;
        }

        return ViagemDto.De(viagem, _endereco);
    }

    public async Task<ViagemDto> AtualizarAsync(int id, ViagemFormDto dto, int adminId)
    {
        Admin admin = await BuscarAdmin(adminId);
        Viagem viagem = await BuscarViagem(id);

        Guid versaoAntiga = viagem.Versao;              // guarda ANTES de alterar
        admin.EditarViagem(viagem, dto.ParaDados());    // confere o dono + regras da viagem

        List<string> removidas = new List<string>();
        foreach (int imagemId in dto.ImagensRemover)
        {
            string? url = viagem.RemoverImagem(imagemId);
            if (url != null)
                removidas.Add(url);
        }

        if (viagem.Imagens.Count + dto.Imagens.Count > Viagem.MaxImagens)
            throw new RegraNegocioException("Máximo de " + Viagem.MaxImagens + " imagens por viagem.");

        List<string> novas = await SalvarImagens(dto.Imagens, viagem);
        try
        {
            await _viagens.AtualizarAsync(viagem, versaoAntiga);
        }
        catch
        {
            ApagarArquivos(novas);
            throw;
        }

        ApagarArquivos(removidas);   // só apaga do disco depois que o banco confirmou
        return ViagemDto.De(viagem, _endereco);
    }

    public async Task ExcluirAsync(int id, int adminId)
    {
        Admin admin = await BuscarAdmin(adminId);
        Viagem viagem = await BuscarViagem(id);

        admin.AutorizarExclusao(viagem);   // dono + sem passagens vendidas
        await _viagens.ExcluirAsync(viagem);

        List<string> arquivos = new List<string>();
        foreach (ViagemImagem imagem in viagem.Imagens)
        {
            arquivos.Add(imagem.Url);
        }
        ApagarArquivos(arquivos);
    }

    public async Task<List<PassageiroCompraDto>> PassageirosAsync(int id, int adminId)
    {
        Admin admin = await BuscarAdmin(adminId);
        Viagem viagem = await BuscarViagem(id);
        admin.AutorizarVerPassageiros(viagem);

        List<Compra> compras = await _compras.ListarDaViagemAsync(id);
        List<PassageiroCompraDto> resultado = new List<PassageiroCompraDto>();
        foreach (Compra compra in compras)
        {
            resultado.Add(PassageiroCompraDto.De(compra));
        }
        return resultado;
    }

    // ---------- Auxiliares ----------

    private async Task<Admin> BuscarAdmin(int adminId)
    {
        Admin? admin = await _usuarios.BuscarAdminAsync(adminId);
        if (admin == null)
            throw new NaoAutorizadoException("Sessão inválida. Entre novamente.");
        return admin;
    }

    private async Task<Viagem> BuscarViagem(int id)
    {
        Viagem? viagem = await _viagens.BuscarAsync(id);
        if (viagem == null)
            throw new NaoEncontradoException("Viagem não encontrada.");
        return viagem;
    }

    private async Task<List<string>> SalvarImagens(List<IFormFile> arquivos, Viagem viagem)
    {
        List<string> salvas = new List<string>();
        try
        {
            foreach (IFormFile arquivo in arquivos)
            {
                string url = await _imagens.SalvarAsync(arquivo);
                salvas.Add(url);
                viagem.AdicionarImagem(url);
            }
        }
        catch
        {
            ApagarArquivos(salvas);   // uma imagem inválida no meio: desfaz as anteriores
            throw;
        }
        return salvas;
    }

    private void ApagarArquivos(List<string> caminhos)
    {
        foreach (string caminho in caminhos)
        {
            _imagens.Remover(caminho);
        }
    }
}
