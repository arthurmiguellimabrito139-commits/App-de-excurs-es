using System.Text;
using Excursoes.Api.Services;

namespace Excursoes.Tests.Services;

// O upload confere o CONTEÚDO do arquivo, não a extensão
public class ImagemTests
{
    private static string? Detectar(byte[] bytes)
    {
        byte[] inicio = new byte[12];
        int lidos = Math.Min(bytes.Length, 12);
        Array.Copy(bytes, inicio, lidos);
        return ImagemLocalStorage.DetectarExtensao(inicio, lidos);
    }

    [Fact]
    public void Jpeg_EhAceito()
    {
        Assert.Equal(".jpg", Detectar(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0 }));
    }

    [Fact]
    public void Png_EhAceito()
    {
        Assert.Equal(".png", Detectar(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0 }));
    }

    [Fact]
    public void Webp_EhAceito()
    {
        Assert.Equal(".webp", Detectar(Encoding.ASCII.GetBytes("RIFF1234WEBPVP8 ")));
    }

    [Fact]
    public void ExecutavelDisfarcado_EhRecusado()
    {
        Assert.Null(Detectar(Encoding.ASCII.GetBytes("MZ\x90\0executavel")));
    }

    [Fact]
    public void HtmlComScript_EhRecusado()
    {
        Assert.Null(Detectar(Encoding.ASCII.GetBytes("<script>alert(1)</script>")));
    }
}
