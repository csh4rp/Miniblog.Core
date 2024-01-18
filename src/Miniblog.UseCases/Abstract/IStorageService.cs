namespace Miniblog.UseCases.Abstract;

public interface IStorageService
{
    Task<string> SaveFileAsync(byte[] bytes, string fileName, CancellationToken cancellationToken);
}
