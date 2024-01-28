using Miniblog.Domain;

namespace Miniblog.UseCases.Abstract;

public interface IPostRepository
{
    Task SaveAsync(Post post, CancellationToken cancellationToken);

    Task DeleteAsync(Post post, CancellationToken cancellationToken);

    Task<Post?> FindByIdAsync(string id, CancellationToken cancellationToken);

    Task<Post?> FindBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<List<Post>> FindAllByTagAsync(int skip, int take, string tag, CancellationToken cancellationToken);

    Task<int> CountByTagAsync(string tag, CancellationToken cancellationToken);

    Task<List<Post>> FindAllByCategoryAsync(int skip, int take, string category, CancellationToken cancellationToken);

    Task<int> CountByCategoryAsync(string category, CancellationToken cancellationToken);

    Task<List<Post>> FindAllAsync(int skip, int take, CancellationToken cancellationToken);

    Task<int> CountAllAsync(CancellationToken cancellationToken);
}
