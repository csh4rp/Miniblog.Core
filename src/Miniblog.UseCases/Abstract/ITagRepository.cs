using Miniblog.Domain;

namespace Miniblog.UseCases.Abstract;

public interface ITagRepository
{
    Task SaveAsync(Tag tag, CancellationToken cancellationToken);

    Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken);
}
