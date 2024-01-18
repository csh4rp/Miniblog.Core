namespace Miniblog.UseCases.Models;

public record ResultPage<T>(List<T> Items, int Total);
