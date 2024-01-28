using Miniblog.UseCases.Dtos;

namespace Miniblog.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using UseCases.Abstract;
    using UseCases.Settings;

    using WebEssentials.AspNetCore.Pwa;

    public class BlogController : Controller
    {
        private readonly WebManifest _manifest;
        private readonly IOptionsSnapshot<BlogSettings> _settings;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;

        public BlogController(IOptionsSnapshot<BlogSettings> settings,
            WebManifest manifest,
            IPostService postService,
            ICategoryService categoryService,
            ITagService tagService)
        {
            _settings = settings;
            _manifest = manifest;
            _postService = postService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        [Route("/blog/comment/{postId}")]
        [HttpPost]
        public async Task<IActionResult> AddComment(string postId, CommentDto comment)
        {
            if (!ModelState.IsValid)
            {
                var post = await _postService.FindByIdAsync(postId, HttpContext.RequestAborted);

                return View(nameof(Post), post);
            }

            comment.IsAdmin = User.Identity!.IsAuthenticated;
            comment.Content = comment.Content.Trim();
            comment.Author = comment.Author.Trim();
            comment.Email = comment.Email.Trim();

            var commentId = string.Empty;

            // the website form key should have been removed by javascript unless the comment was
            // posted by a spam robot
            if (!Request.Form.ContainsKey("website"))
            {
                commentId = await _postService.AddComment(postId,
                    comment,
                    HttpContext.RequestAborted);
            }

            var addedPost = await _postService.FindByIdAsync(postId, HttpContext.RequestAborted);

            return Redirect($"/blog/{System.Net.WebUtility.UrlEncode(addedPost!.Slug)}/#{commentId}");
        }

        [Route("/blog/category/{category}/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Category(string category, int page = 0)
        {
            // get posts for the selected category.
            var posts =
                await _postService.FindAllByCategoryAsync(page, category, HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = posts.Items;

            // set the view option
            ViewData["ViewOption"] = _settings.Value.ListView;

            ViewData[Constants.TotalPostCount] = posts.Total;
            ViewData[Constants.Title] = $"{_manifest.Name} {category}";
            ViewData[Constants.Description] = $"Articles posted in the {category} category";
            ViewData[Constants.Prev] = $"/blog/category/{category}/{page + 1}/";
            ViewData[Constants.Next] = $"/blog/category/{category}/{(page <= 1 ? null : page - 1 + "/")}";
            return View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/tag/{tag}/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Tag(string tag, int page = 0)
        {
            // get posts for the selected tag.
            var posts = await _postService.FindAllByTagAsync(page, tag, HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = posts.Items;

            // set the view option
            ViewData["ViewOption"] = _settings.Value.ListView;

            ViewData[Constants.TotalPostCount] = posts.Total;
            ViewData[Constants.Title] = $"{_manifest.Name} {tag}";
            ViewData[Constants.Description] = $"Articles posted in the {tag} tag";
            ViewData[Constants.Prev] = $"/blog/tag/{tag}/{page + 1}/";
            ViewData[Constants.Next] = $"/blog/tag/{tag}/{(page <= 1 ? null : page - 1 + "/")}";
            return View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/comment/{postId}/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string postId, string commentId)
        {
            var post =
                await _postService.FindByIdAsync(postId, HttpContext.RequestAborted);

            if (post is null)
            {
                return NotFound();
            }

            await _postService.DeleteCommentAsync(postId,
                commentId,
                HttpContext.RequestAborted);


            return Redirect($"/blog/{System.Net.WebUtility.UrlEncode(post.Slug)}/#comments");
        }

        [Route("/blog/deletepost/{id}")]
        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeletePost(string id)
        {
            var post =
                await _postService.FindByIdAsync(id, HttpContext.RequestAborted);

            if (post is null)
            {
                return NotFound();
            }

            await _postService.DeleteAsync(id, HttpContext.RequestAborted);
            return Redirect("/");
        }

        [Route("/blog/edit/{id?}")]
        [HttpGet, Authorize]
        public async Task<IActionResult> Edit(string? id)
        {
            var categories = await _categoryService.GetAllAsync(HttpContext.RequestAborted);
            ViewData[Constants.AllCats] = categories;

            var tags = await _tagService.GetAllAsync(HttpContext.RequestAborted);
            ViewData[Constants.AllTags] = tags;

            if (string.IsNullOrEmpty(id))
            {
                return View(new PostDto());
            }

            var post = await _postService.FindByIdAsync(id, HttpContext.RequestAborted);

            return post is null ? NotFound() : View(post);
        }

        [Route("/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Index([FromRoute]int page = 0)
        {
            // get published posts.
            var resultPage = await _postService.GetAllAsync(page, null, HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = resultPage.Items;

            // set the view option
            ViewData[Constants.ViewOption] = _settings.Value.ListView;

            ViewData[Constants.TotalPostCount] = resultPage.Total;
            ViewData[Constants.Title] = _manifest.Name;
            ViewData[Constants.Description] = _manifest.Description;
            ViewData[Constants.Prev] = $"/{page + 1}/";
            ViewData[Constants.Next] = $"/{(page <= 1 ? null : $"{page - 1}/")}";

            return View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/{slug?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Post(string slug)
        {
            var post = await _postService.FindBySlugAsync(slug, HttpContext.RequestAborted);

            return post is null ? NotFound() : (IActionResult)View(post);
        }

        /// <remarks>This is for redirecting potential existing URLs from the old Miniblog URL format.</remarks>
        [Route("/post/{slug}")]
        [HttpGet]
        public IActionResult Redirects(string slug) => LocalRedirectPermanent($"/blog/{slug}");

        [Route("/blog/{slug?}")]
        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Consumer preference.")]
        public async Task<IActionResult> UpdatePost(PostDto post)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), post);
            }

            if (post is null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            var existing = await _postService.FindByIdAsync(post.Id!, HttpContext.RequestAborted);
            var existingPostWithSameSlug = await _postService.FindBySlugAsync(post.Slug, HttpContext.RequestAborted);

            existing ??= new PostDto();

            if (existingPostWithSameSlug != null && existingPostWithSameSlug.Id != post.Id)
            {
                existing.Slug = PostDto.CreateSlug(post.Title + DateTime.UtcNow.ToString("yyyyMMddHHmm"));
            }
            string categories = Request.Form[Constants.Categories]!;
            string tags = Request.Form[Constants.Tags]!;

            existing.Categories.Clear();
            categories.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToLowerInvariant())
                .ToList()
                .ForEach(existing.Categories.Add);
            existing.Tags.Clear();
            tags.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .ToList()
                .ForEach(existing.Tags.Add);
            existing.Title = post.Title.Trim();
            existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : PostDto.CreateSlug(post.Title);
            existing.IsPublished = post.IsPublished;
            existing.Content = post.Content.Trim();
            existing.Excerpt = post.Excerpt.Trim();

            await _postService.SaveAsync(existing, HttpContext.RequestAborted);

            return Redirect($"/blog/{System.Net.WebUtility.UrlEncode(post.Slug)}");
        }
    }
}
