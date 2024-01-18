namespace Miniblog.UseCases.Abstract;

public interface ICategoryService
{
    Task<List<string>> GetAllAsync(CancellationToken cancellationToken);
}
