using Excursoes.Api.Excecoes;

namespace Excursoes.Api.Services;

public interface IImagemStorage
{
    Task<string> SalvarAsync(IFormFile arquivo);   // devolve o caminho, ex.: /uploads/abc.webp
    void Remover(string caminho);
}

// Salva as imagens na pasta "uploads" do servidor, com segurança:
//  - confere o TAMANHO (limite configurável, padrão 5 MB)
//  - confere o CONTEÚDO pelos primeiros bytes (não confia na extensão nem no tipo que o navegador diz)
//  - salva com NOME ALEATÓRIO (o nome original do usuário nunca é usado)
//  - a extensão final vem do conteúdo detectado, então um ".exe" disfarçado de ".jpg" é recusado
public class ImagemLocalStorage : IImagemStorage
{
    public const string PrefixoUrl = "/uploads/";

    private readonly string _pasta;
    private readonly long _tamanhoMaximo;

    public ImagemLocalStorage(IWebHostEnvironment ambiente, IConfiguration configuracao)
    {
        _pasta = PastaUploads(ambiente);
        Directory.CreateDirectory(_pasta);

        int megas = configuracao.GetValue<int>("Uploads:TamanhoMaximoMb", 5);
        _tamanhoMaximo = megas * 1024L * 1024L;
    }

    public static string PastaUploads(IWebHostEnvironment ambiente)
    {
        return Path.Combine(ambiente.ContentRootPath, "wwwroot", "uploads");
    }

    public async Task<string> SalvarAsync(IFormFile arquivo)
    {
        if (arquivo.Length == 0)
            throw new RegraNegocioException("Imagem vazia.");
        if (arquivo.Length > _tamanhoMaximo)
            throw new RegraNegocioException("Cada imagem pode ter no máximo " + (_tamanhoMaximo / 1024 / 1024) + " MB.");

        byte[] inicio = new byte[12];
        int lidos;
        using (Stream leitura = arquivo.OpenReadStream())
        {
            lidos = await leitura.ReadAtLeastAsync(inicio, inicio.Length, false);
        }

        string? extensao = DetectarExtensao(inicio, lidos);
        if (extensao == null)
            throw new RegraNegocioException("Envie apenas imagens JPG, PNG ou WEBP.");

        string nome = Guid.NewGuid().ToString("N") + extensao;
        string destino = Path.Combine(_pasta, nome);

        using (FileStream escrita = new FileStream(destino, FileMode.CreateNew, FileAccess.Write))
        {
            await arquivo.CopyToAsync(escrita);
        }

        return PrefixoUrl + nome;
    }

    public void Remover(string caminho)
    {
        if (!caminho.StartsWith(PrefixoUrl, StringComparison.Ordinal))
            return;

        // Só o nome do arquivo, sem pastas: impede "../" (path traversal)
        string nome = caminho.Substring(PrefixoUrl.Length);
        if (nome.Length == 0 || nome != Path.GetFileName(nome))
            return;

        string completo = Path.Combine(_pasta, nome);
        if (File.Exists(completo))
            File.Delete(completo);
    }

    // "Assinatura" de cada formato nos primeiros bytes do arquivo
    public static string? DetectarExtensao(byte[] b, int lidos)
    {
        // JPEG: FF D8 FF
        if (lidos >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (lidos >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
            && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";

        // WEBP: "RIFF" ???? "WEBP"
        if (lidos >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
            && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
            return ".webp";

        return null;
    }
}
