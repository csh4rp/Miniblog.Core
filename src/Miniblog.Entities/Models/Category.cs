namespace Miniblog.Domain.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = default!;
}
