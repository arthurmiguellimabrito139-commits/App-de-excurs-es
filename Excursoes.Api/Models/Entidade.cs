namespace Excursoes.Api.Models;

// Base de todas as classes que viram linha no banco.
public abstract class Entidade
{
    public int Id { get; private set; }

    // O repositório chama depois do INSERT, com o id gerado pelo banco
    internal void DefinirId(int id)
    {
        if (Id != 0)
            throw new InvalidOperationException("Id já definido.");
        Id = id;
    }
}
