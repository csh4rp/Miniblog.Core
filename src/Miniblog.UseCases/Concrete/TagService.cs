namespace Miniblog.UseCases.Concrete;

using Abstract;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<string>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);

        return tags.Select(t => t.Name).Order().ToList();
    }
}
