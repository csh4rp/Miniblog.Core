using Miniblog.Domain;

namespace Miniblog.UseCases.Abstract;

public interface ICategoryRepository
{
    Task SaveAsync(Category category, CancellationToken cancellationToken);

    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
}
