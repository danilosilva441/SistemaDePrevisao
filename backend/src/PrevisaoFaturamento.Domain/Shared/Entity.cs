namespace PrevisaoFaturamento.Domain.Shared;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;

    // Construtor protegido para uso do EF Core
    protected Entity() { }
}