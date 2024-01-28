using Miniblog.UseCases.Dtos;

namespace Miniblog.UseCases.Abstract;

public interface IPostService
{
    Task SaveAsync(PostDto postDto, CancellationToken cancellationToken);

    Task DeleteAsync(string postId, CancellationToken cancellationToken);

    Task<ResultPage<PostDto>> GetAllAsync(int page, int? pageSize, CancellationToken cancellationToken);

    Task<PostDto?> FindByIdAsync(string postId, CancellationToken cancellationToken);

    Task<PostDto?> FindBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ResultPage<PostDto>> FindAllByCategoryAsync(int page, string category, CancellationToken cancellationToken);

    Task<ResultPage<PostDto>> FindAllByTagAsync(int page, string tag, CancellationToken cancellationToken);

    Task<string> AddComment(string postId, CommentDto commentDto, CancellationToken cancellationToken);

    Task DeleteCommentAsync(string postId, string commentId, CancellationToken cancellationToken);
}
