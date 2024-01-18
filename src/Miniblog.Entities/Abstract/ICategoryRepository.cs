namespace Miniblog.Domain.Abstract;

using Models;

public interface ICategoryRepository
{
    Task SaveAsync(Category category, CancellationToken cancellationToken);

    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
}
