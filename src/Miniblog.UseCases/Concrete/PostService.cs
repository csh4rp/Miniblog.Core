using Miniblog.Domain;
using Miniblog.UseCases.Dtos;

namespace Miniblog.UseCases.Concrete;

using Abstract;
using Microsoft.Extensions.Options;
using Settings;

using System.Text.RegularExpressions;
using System.Xml;

public class PostService : IPostService
{
    private readonly int _pageSize;

    private readonly IPostRepository _postRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IStorageService _storageService;
    private readonly ITagRepository _tagRepository;
    private readonly ICategoryRepository _categoryRepository;

    public PostService(IPostRepository postRepository,
        TimeProvider timeProvider,
        IStorageService storageService,
        ITagRepository tagRepository,
        ICategoryRepository categoryRepository,
        IOptions<BlogSettings> options)
    {
        _postRepository = postRepository;
        _timeProvider = timeProvider;
        _storageService = storageService;
        _tagRepository = tagRepository;
        _categoryRepository = categoryRepository;

        _pageSize = options.Value.PostsPerPage;
    }

    public async Task SaveAsync(PostDto postDto, CancellationToken cancellationToken)
    {
        Post post;

        var allTags = await _tagRepository.GetAllAsync(cancellationToken);
        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);

        foreach (var tag in postDto.Tags)
        {
            var existingTag = allTags.FirstOrDefault(t => t.Name == tag);
            if (existingTag is null)
            {
                existingTag = new Tag { Name = tag };
                await _tagRepository.SaveAsync(existingTag, cancellationToken);
                allTags.Add(existingTag);
            }
        }

        foreach (var category in postDto.Categories)
        {
            var existingCategory = allCategories.FirstOrDefault(t => t.Name == category);
            if (existingCategory is null)
            {
                existingCategory = new Category { Name = category };
                await _categoryRepository.SaveAsync(existingCategory, cancellationToken);
                allCategories.Add(existingCategory);
            }
        }

        if (!string.IsNullOrEmpty(postDto.Id))
        {
            post = await _postRepository.FindByIdAsync(postDto.Id, cancellationToken)
                   ?? throw new InvalidOperationException("Post does not exist");
        }
        else
        {
            post = new Post();
        }

        post = Map(post, postDto);

        foreach (var tag in postDto.Tags)
        {
            var existingTag = allTags.Single(t => t.Name == tag);
            if (post.Tags.All(t => t.Name != tag))
            {
                post.Tags.Add(existingTag);
            }
        }

        var tagsToRemove = new List<Tag>();
        foreach (var tag in post.Tags)
        {
            if (postDto.Tags.All(t => t != tag.Name))
            {
                tagsToRemove.Add(tag);
            }
        }

        tagsToRemove.ForEach(t => post.Tags.Remove(t));

        foreach (var category in postDto.Categories)
        {
            var existingCategory = allCategories.Single(t => t.Name == category);
            if (post.Categories.All(t => t.Name != category))
            {
                post.Categories.Add(existingCategory);
            }
        }

        var categoriesToRemove = new List<Category>();
        foreach (var category in post.Categories)
        {
            if (postDto.Categories.All(t => t != category.Name))
            {
                categoriesToRemove.Add(category);
            }
        }

        categoriesToRemove.ForEach(c => post.Categories.Remove(c));

        await SaveFilesToDiskAsync(post, cancellationToken);

        await _postRepository.SaveAsync(post, cancellationToken);
    }

    public async Task DeleteAsync(string postId, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(postId, cancellationToken)
                   ?? throw new InvalidOperationException("Post does not exist");

        await _postRepository.DeleteAsync(post, cancellationToken);
    }

    public async Task<ResultPage<PostDto>> GetAllAsync(int page, int? pageSize, CancellationToken cancellationToken)
    {
        var take = pageSize ?? _pageSize;
        var skip = page * take;

        var posts = await _postRepository.FindAllAsync(skip, take, cancellationToken);
        var count = await _postRepository.CountAllAsync(cancellationToken);

        var items =  posts.Select(Map).ToList();

        return new ResultPage<PostDto>(items, count);
    }

    public async Task<PostDto?> FindByIdAsync(string postId, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(postId, cancellationToken);

        return post is null ? null : Map(post);
    }

    private static PostDto Map(Post post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        LastModified = post.LastModified,
        Slug = post.Slug,
        Content = post.Content,
        Excerpt = post.Excerpt,
        IsPublished = post.IsPublished,
        PubDate = post.PubDate,
        Tags = post.Tags.Select(t => t.Name).ToList(),
        Comments = post.Comments.Select(Map).ToList(),
        Categories = post.Categories.Select(c => c.Name).ToList()
    };

    private Post Map(Post post, PostDto postDto)
    {
        post.Slug = postDto.Slug;
        post.Title = postDto.Title;
        post.Excerpt = postDto.Excerpt;
        post.Content = postDto.Content;
        post.IsPublished = postDto.IsPublished;
        post.LastModified = _timeProvider.GetUtcNow().UtcDateTime;

        return post;
    }

    private static CommentDto Map(Comment comment) => new()
    {
        Id = comment.Id,
        Content = comment.Content,
        PubDate = comment.PubDate,
        Author = comment.Author,
        Email = comment.Email,
        IsAdmin = comment.IsAdmin
    };

    public async Task<PostDto?> FindBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindBySlugAsync(slug, cancellationToken);

        return post is null ? null : Map(post);
    }

    public async Task<ResultPage<PostDto>> FindAllByCategoryAsync(int page,
        string category,
        CancellationToken cancellationToken)
    {
        var skip = page * _pageSize;
        var take = _pageSize;

        var posts =
            await _postRepository.FindAllByCategoryAsync(skip, take, category,
                cancellationToken);

        var count = await _postRepository.CountByCategoryAsync(category, cancellationToken);

        var items =  posts.Select(Map).ToList();

        return new ResultPage<PostDto>(items, count);
    }

    public async Task<ResultPage<PostDto>> FindAllByTagAsync(int page,
        string tag,
        CancellationToken cancellationToken)
    {
        var skip = page * _pageSize;
        var take = _pageSize;

        var posts =
            await _postRepository.FindAllByTagAsync(skip, take, tag, cancellationToken);

        var count = await _postRepository.CountByTagAsync(tag, cancellationToken);

        var items =  posts.Select(Map).ToList();

        return new ResultPage<PostDto>(items, count);
    }

    public async Task<string> AddComment(string postId,
        CommentDto commentDto,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(postId, cancellationToken)
                   ?? throw new InvalidOperationException("Post was not found");

        var comment = new Comment
        {
            Content = commentDto.Content,
            PubDate = _timeProvider.GetUtcNow().UtcDateTime,
            Author = commentDto.Author,
            Email = commentDto.Email
        };

        post.Comments.Add(comment);

        await _postRepository.SaveAsync(post, cancellationToken);

        return comment.Id;
    }

    public async Task DeleteCommentAsync(string postId,
        string commentId,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.FindByIdAsync(postId, cancellationToken)
                   ?? throw new InvalidOperationException("Post was not found");

        var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is not null)
        {
            post.Comments.Remove(comment);
        }

        await _postRepository.SaveAsync(post, cancellationToken);
    }

    private async Task SaveFilesToDiskAsync(Post post, CancellationToken cancellationToken)
    {
        var imgRegex = new Regex("<img[^>]+ />", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var base64Regex = new Regex("data:[^/]+/(?<ext>[a-z]+);base64,(?<base64>.+)",
            RegexOptions.IgnoreCase);
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".gif", ".png", ".webp" };

        foreach (Match? match in imgRegex.Matches(post.Content))
        {
            if (match is null)
            {
                continue;
            }

            var doc = new XmlDocument();
            doc.LoadXml($"<root>{match.Value}</root>");

            var img = doc.FirstChild!.FirstChild;
            var srcNode = img!.Attributes!["src"];
            var fileNameNode = img.Attributes["data-filename"];

            // The HTML editor creates base64 DataURIs which we'll have to convert to image
            // files on disk
            if (srcNode is null || fileNameNode is null)
            {
                continue;
            }

            var extension = System.IO.Path.GetExtension(fileNameNode.Value);

            // Only accept image files
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var base64Match = base64Regex.Match(srcNode.Value);
            if (base64Match.Success)
            {
                var bytes = Convert.FromBase64String(base64Match.Groups["base64"].Value);
                srcNode.Value =
                    await _storageService.SaveFileAsync(bytes, fileNameNode.Value,
                        cancellationToken);

                img.Attributes.Remove(fileNameNode);
                post.Content = post.Content.Replace(match.Value, img.OuterXml,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
