using Miniblog.UseCases.Abstract;
using Miniblog.UseCases.Dtos;

namespace Miniblog.Web.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using WilderMinds.MetaWeblog;

public class MetaWeblogService : IMetaWeblogProvider
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _context;
    private readonly IPostService _postService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;
    private readonly IStorageService _storageService;

    public MetaWeblogService(IConfiguration config,
        IHttpContextAccessor context,
        IPostService postService,
        ICategoryService categoryService,
        ITagService tagService,
        IStorageService storageService)
    {
        _config = config;
        _context = context;
        _postService = postService;
        _categoryService = categoryService;
        _tagService = tagService;
        _storageService = storageService;
    }

    public Task<int> AddCategoryAsync(string key, string username, string password, NewCategory category)
    {
        ValidateCredentials(username, password);

        throw new NotImplementedException();
    }

    public Task<string> AddPageAsync(string blogid, string username, string password, Page page, bool publish)
    {
        ValidateCredentials(username, password);

        throw new NotImplementedException();
    }

    public async Task<string> AddPostAsync(string blogid, string username, string password, Post post, bool publish)
    {
        ValidateCredentials(username, password);

        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        var newPost = new PostDto
        {
            Title = post.title,
            Slug = !string.IsNullOrWhiteSpace(post.wp_slug) ? post.wp_slug : PostDto.CreateSlug(post.title),
            Excerpt = post.mt_excerpt,
            Content = post.description,
            IsPublished = publish
        };

        post.categories.ToList().ForEach(newPost.Categories.Add);
        post.mt_keywords.Split(',').ToList().ForEach(newPost.Tags.Add);

        if (post.dateCreated != DateTime.MinValue)
        {
            newPost.PubDate = post.dateCreated;
        }

        await _postService.SaveAsync(newPost, default);

        return newPost.Id!;
    }

    public Task<bool> DeletePageAsync(string blogid, string username, string password, string pageid)
    {
        ValidateCredentials(username, password);

        throw new NotImplementedException();
    }

    public async Task<bool> DeletePostAsync(string key, string postid, string username, string password, bool publish)
    {
        ValidateCredentials(username, password);

        var post = await _postService.FindByIdAsync(postid, default);
        if (post is null)
        {
            return false;
        }

        await _postService.DeleteAsync(postid, default);
        return true;
    }

    public Task<bool> EditPageAsync(string blogid, string pageid, string username, string password, Page page, bool publish)
    {
        ValidateCredentials(username, password);

        throw new NotImplementedException();
    }

    public async Task<bool> EditPostAsync(string postid, string username, string password, Post? post, bool publish)
    {
        ValidateCredentials(username, password);

        var existing = await _postService.FindByIdAsync(postid, default);

        if (existing is null || post is null)
        {
            return false;
        }

        existing.Title = post.title;
        existing.Slug = post.wp_slug;
        existing.Excerpt = post.mt_excerpt;
        existing.Content = post.description;
        existing.IsPublished = publish;
        existing.Categories.Clear();
        post.categories.ToList().ForEach(existing.Categories.Add);
        existing.Tags.Clear();
        post.mt_keywords.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(existing.Tags.Add);

        if (post.dateCreated != DateTime.MinValue)
        {
            existing.PubDate = post.dateCreated;
        }

        await _postService.SaveAsync(existing, default);

        return true;
    }

    public Task<Author[]> GetAuthorsAsync(string blogid, string username, string password) =>
        throw new NotImplementedException();

    public async Task<CategoryInfo[]> GetCategoriesAsync(string blogid, string username, string password)
    {
        ValidateCredentials(username, password);

        var categories = await _categoryService.GetAllAsync(default);

            return categories.Select(
                cat =>
                    new CategoryInfo
                    {
                        categoryid = cat,
                        title = cat
                    })
            .ToArray();
    }

    public Task<Page> GetPageAsync(string blogid, string pageid, string username, string password) =>
        throw new NotImplementedException();

    public Task<Page[]> GetPagesAsync(string blogid, string username, string password, int numPages) =>
        throw new NotImplementedException();

    public async Task<Post?> GetPostAsync(string postid, string username, string password)
    {
        ValidateCredentials(username, password);

        var post = await _postService.FindByIdAsync(postid, default);

        return post is null ? null : ToMetaWebLogPost(post);
    }

    public async Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int numberOfPosts)
    {
        ValidateCredentials(username, password);

        var posts = await _postService.GetAllAsync(0, numberOfPosts, default);

        return posts.Items.Select(ToMetaWebLogPost).ToArray();
    }

    public async Task<Tag[]> GetTagsAsync(string blogid, string username, string password)
    {
        ValidateCredentials(username, password);

        var tags = await _tagService.GetAllAsync(default);

        return tags.Select(
                tag =>
                    new Tag
                    {
                        name = tag
                    })
            .ToArray();
    }

    public Task<UserInfo> GetUserInfoAsync(string key, string username, string password)
    {
        ValidateCredentials(username, password);

        throw new NotImplementedException();
    }

    public Task<BlogInfo[]> GetUsersBlogsAsync(string key, string username, string password)
    {
        ValidateCredentials(username, password);

        var request = _context.HttpContext!.Request;
        var url = $"{request.Scheme}://{request.Host}";

        return Task.FromResult(
            new[]
            {
                new BlogInfo
                {
                    blogid ="1",
                    blogName = _config[Constants.Config.Blog.Name] ?? nameof(MetaWeblogService),
                    url = url
                }
            });
    }

    public async Task<MediaObjectInfo> NewMediaObjectAsync(string blogid, string username, string password, MediaObject mediaObject)
    {
        ValidateCredentials(username, password);

        if (mediaObject is null)
        {
            throw new ArgumentNullException(nameof(mediaObject));
        }

        var bytes = Convert.FromBase64String(mediaObject.bits);
        var path = await _storageService.SaveFileAsync(bytes, mediaObject.name, default);

        return new MediaObjectInfo { url = path };
    }

    private Post ToMetaWebLogPost(PostDto post)
    {
        var request = _context.HttpContext!.Request;
        var url = $"{request.Scheme}://{request.Host}";

        return new Post
        {
            postid = post.Id,
            title = post.Title,
            wp_slug = post.Slug,
            permalink = url + post.GetLink(),
            dateCreated = post.PubDate,
            mt_excerpt = post.Excerpt,
            description = post.Content,
            categories = post.Categories.ToArray(),
            mt_keywords = string.Join(',', post.Tags)
        };
    }

    private void ValidateCredentials(string username, string password)
    {
        if (ValidateUser(username, password) == false)
        {
            throw new MetaWeblogException(Properties.Resources.Unauthorized);
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, username));

        _context.HttpContext!.User = new ClaimsPrincipal(identity);
    }

    private bool ValidateUser(string username, string password) =>
        username == _config[Constants.Config.User.UserName] && VerifyHashedPassword(password);

    private bool VerifyHashedPassword(string password)
    {
        var saltBytes = Encoding.UTF8.GetBytes(_config[Constants.Config.User.Salt]!);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8);

        var hashText = BitConverter.ToString(hashBytes).Replace(Constants.Dash, string.Empty, StringComparison.OrdinalIgnoreCase);
        return hashText == _config[Constants.Config.User.Password];
    }
}
