namespace Miniblog.UseCases.Concrete;

using Abstract;

using Miniblog.Domain.Abstract;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        this._categoryRepository = categoryRepository;
    }

    public async Task<List<string>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await this._categoryRepository.GetAllAsync(cancellationToken);

        return categories.Select(t => t.Name).Order().ToList();
    }
}
