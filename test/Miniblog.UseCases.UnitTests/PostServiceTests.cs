using FluentAssertions;
using Microsoft.Extensions.Options;
using Miniblog.Domain;
using Miniblog.UseCases.Abstract;
using Miniblog.UseCases.Concrete;
using Miniblog.UseCases.Dtos;
using Miniblog.UseCases.Settings;
using NSubstitute;

namespace Miniblog.UseCases.UnitTests;

public class PostServiceTests
{
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IOptions<BlogSettings> _options = new OptionsWrapper<BlogSettings>(new BlogSettings
    {
        PostsPerPage = 1,
    });

    [Fact]
    public async Task given_valid_dto_when_saving_changes_entity_is_added()
    {
        // Given
        var dto = new PostDto
        {
            Title = "New Post",
            Content = "Post",
            IsPublished = true,
            Slug = PostDto.CreateSlug("New Post"),
            Excerpt = "Test"
        };

        _categoryRepository.GetAllAsync(default)
            .ReturnsForAnyArgs(new List<Category>());

        _tagRepository.GetAllAsync(default)
            .ReturnsForAnyArgs(new List<Tag>());

        Post? post = null;

        await _postRepository.SaveAsync(Arg.Do<Post>(p => post = p), Arg.Any<CancellationToken>());

        var sut = new PostService(_postRepository, _tagRepository, _categoryRepository, TimeProvider.System, _storageService, _options);

        // When
        await sut.SaveAsync(dto, default);

        // Then
        await _postRepository.Received(1)
            .SaveAsync(Arg.Is<Post>(p => p == post), Arg.Any<CancellationToken>());

        post.Should().NotBeNull();
        post!.Id.Should().NotBeEmpty();
        post.Title.Should().Be(dto.Title);
        post.Content.Should().Be(dto.Content);
        post.Excerpt.Should().Be(dto.Excerpt);
        post.Slug.Should().Be(dto.Slug);
    }
}