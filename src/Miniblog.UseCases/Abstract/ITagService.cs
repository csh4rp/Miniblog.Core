namespace Miniblog.UseCases.Abstract;

public interface ITagService
{
    Task<List<string>> GetAllAsync(CancellationToken cancellationToken);
}
