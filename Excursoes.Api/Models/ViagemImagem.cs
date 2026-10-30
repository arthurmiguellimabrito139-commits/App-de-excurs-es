namespace Excursoes.Api.Models;

public class ViagemImagem
{
    public int Id { get; private set; }
    public int ViagemId { get; private set; }
    public string Url { get; private set; } = "";

    private ViagemImagem() { } // EF Core

    internal ViagemImagem(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL da imagem vazia.");
        Url = url;
    }
}