namespace Miniblog.Domain.Abstract;

using Models;

public interface ITagRepository
{
    Task SaveAsync(Tag tag, CancellationToken cancellationToken);

    Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken);
}
