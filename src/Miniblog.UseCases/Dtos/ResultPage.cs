namespace Miniblog.UseCases.Dtos;

public record ResultPage<T>(List<T> Items, int Total);
