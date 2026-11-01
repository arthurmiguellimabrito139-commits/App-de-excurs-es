namespace Excursoes.Api.Models;

public class ViagemImagem : Entidade
{
    public int ViagemId { get; private set; }

    // Caminho relativo, ex.: /uploads/3f2a...c1.webp
    public string Url { get; private set; } = "";

    private ViagemImagem()
    {
    }

    internal ViagemImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Endereço da imagem vazio.");
        Url = url;
    }
}
