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
    using UseCases.Models;
    using UseCases.Settings;

    using WebEssentials.AspNetCore.Pwa;

    public class BlogController : Controller
    {
        private readonly WebManifest manifest;

        private readonly IOptionsSnapshot<BlogSettings> settings;

        private readonly IPostService _postService;

        private readonly ICategoryService _categoryService;

        private readonly ITagService _tagService;

        public BlogController(IOptionsSnapshot<BlogSettings> settings, WebManifest manifest, IPostService postService, ICategoryService categoryService, ITagService tagService)
        {
            this.settings = settings;
            this.manifest = manifest;
            this._postService = postService;
            this._categoryService = categoryService;
            this._tagService = tagService;
        }

        [Route("/blog/comment/{postId}")]
        [HttpPost]
        public async Task<IActionResult> AddComment(string postId, CommentDto comment)
        {
            if (!this.ModelState.IsValid)
            {
                var post = await this._postService.FindByIdAsync(postId, this.HttpContext.RequestAborted);

                return this.View(nameof(Post), post);
            }

            comment.IsAdmin = this.User.Identity!.IsAuthenticated;
            comment.Content = comment.Content.Trim();
            comment.Author = comment.Author.Trim();
            comment.Email = comment.Email.Trim();

            var commentId = string.Empty;

            // the website form key should have been removed by javascript unless the comment was
            // posted by a spam robot
            if (!this.Request.Form.ContainsKey("website"))
            {
                commentId = await this._postService.AddComment(postId,
                    comment,
                    this.HttpContext.RequestAborted);
            }

            var addedPost = await this._postService.FindByIdAsync(postId, this.HttpContext.RequestAborted);

            return this.Redirect($"/blog/{System.Net.WebUtility.UrlEncode(addedPost!.Slug)}/#{commentId}");
        }

        [Route("/blog/category/{category}/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Category(string category, int page = 0)
        {
            // get posts for the selected category.
            var posts =
                await this._postService.FindAllByCategoryAsync(page, category, this.HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = posts.Items;

            // set the view option
            this.ViewData["ViewOption"] = this.settings.Value.ListView;

            this.ViewData[Constants.TotalPostCount] = posts.Total;
            this.ViewData[Constants.Title] = $"{this.manifest.Name} {category}";
            this.ViewData[Constants.Description] = $"Articles posted in the {category} category";
            this.ViewData[Constants.prev] = $"/blog/category/{category}/{page + 1}/";
            this.ViewData[Constants.next] = $"/blog/category/{category}/{(page <= 1 ? null : page - 1 + "/")}";
            return this.View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/tag/{tag}/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Tag(string tag, int page = 0)
        {
            // get posts for the selected tag.
            var posts = await this._postService.FindAllByTagAsync(page, tag, this.HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = posts.Items;

            // set the view option
            this.ViewData["ViewOption"] = this.settings.Value.ListView;

            this.ViewData[Constants.TotalPostCount] = posts.Total;
            this.ViewData[Constants.Title] = $"{this.manifest.Name} {tag}";
            this.ViewData[Constants.Description] = $"Articles posted in the {tag} tag";
            this.ViewData[Constants.prev] = $"/blog/tag/{tag}/{page + 1}/";
            this.ViewData[Constants.next] = $"/blog/tag/{tag}/{(page <= 1 ? null : page - 1 + "/")}";
            return this.View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/comment/{postId}/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string postId, string commentId)
        {
            var post =
                await this._postService.FindByIdAsync(postId, this.HttpContext.RequestAborted);

            if (post is null)
            {
                return this.NotFound();
            }

            await this._postService.DeleteCommentAsync(postId,
                commentId,
                this.HttpContext.RequestAborted);


            return this.Redirect($"/blog/{System.Net.WebUtility.UrlEncode(post.Slug)}/#comments");
        }

        [Route("/blog/deletepost/{id}")]
        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeletePost(string id)
        {
            var post =
                await this._postService.FindByIdAsync(id, this.HttpContext.RequestAborted);

            if (post is null)
            {
                return this.NotFound();
            }

            await this._postService.DeleteAsync(id, this.HttpContext.RequestAborted);
            return this.Redirect("/");
        }

        [Route("/blog/edit/{id?}")]
        [HttpGet, Authorize]
        public async Task<IActionResult> Edit(string? id)
        {
            var categories = await this._categoryService.GetAllAsync(this.HttpContext.RequestAborted);
            this.ViewData[Constants.AllCats] = categories;

            var tags = await this._tagService.GetAllAsync(this.HttpContext.RequestAborted);
            this.ViewData[Constants.AllTags] = tags;

            if (string.IsNullOrEmpty(id))
            {
                return this.View(new PostDto());
            }

            var post = await this._postService.FindByIdAsync(id, this.HttpContext.RequestAborted);

            return post is null ? this.NotFound() : this.View(post);
        }

        [Route("/{page:int?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Index([FromRoute]int page = 0)
        {
            // get published posts.
            var resultPage = await this._postService.GetAllAsync(page, this.HttpContext.RequestAborted);

            // apply paging filter.
            var filteredPosts = resultPage.Items;

            // set the view option
            this.ViewData[Constants.ViewOption] = this.settings.Value.ListView;

            this.ViewData[Constants.TotalPostCount] = resultPage.Total;
            this.ViewData[Constants.Title] = this.manifest.Name;
            this.ViewData[Constants.Description] = this.manifest.Description;
            this.ViewData[Constants.prev] = $"/{page + 1}/";
            this.ViewData[Constants.next] = $"/{(page <= 1 ? null : $"{page - 1}/")}";

            return this.View("~/Views/Blog/Index.cshtml", filteredPosts);
        }

        [Route("/blog/{slug?}")]
        [OutputCache(Profile = "default")]
        public async Task<IActionResult> Post(string slug)
        {
            var post = await this._postService.FindBySlugAsync(slug, this.HttpContext.RequestAborted);

            return post is null ? this.NotFound() : (IActionResult)this.View(post);
        }

        /// <remarks>This is for redirecting potential existing URLs from the old Miniblog URL format.</remarks>
        [Route("/post/{slug}")]
        [HttpGet]
        public IActionResult Redirects(string slug) => this.LocalRedirectPermanent($"/blog/{slug}");

        [Route("/blog/{slug?}")]
        [HttpPost, Authorize, AutoValidateAntiforgeryToken]
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Consumer preference.")]
        public async Task<IActionResult> UpdatePost(PostDto post)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(nameof(Edit), post);
            }

            if (post is null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            var existing = await this._postService.FindByIdAsync(post.Id!, this.HttpContext.RequestAborted);
            var existingPostWithSameSlug = await this._postService.FindBySlugAsync(post.Slug, this.HttpContext.RequestAborted);

            existing ??= new PostDto();

            if (existingPostWithSameSlug != null && existingPostWithSameSlug.Id != post.Id)
            {
                existing.Slug = Models.Post.CreateSlug(post.Title + DateTime.UtcNow.ToString("yyyyMMddHHmm"));
            }
            string categories = this.Request.Form[Constants.categories]!;
            string tags = this.Request.Form[Constants.tags]!;

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
            existing.Slug = !string.IsNullOrWhiteSpace(post.Slug) ? post.Slug.Trim() : Models.Post.CreateSlug(post.Title);
            existing.IsPublished = post.IsPublished;
            existing.Content = post.Content.Trim();
            existing.Excerpt = post.Excerpt.Trim();

            await this._postService.SaveAsync(existing, this.HttpContext.RequestAborted);

            return this.Redirect($"/blog/{System.Net.WebUtility.UrlEncode(post.Slug)}");
        }
    }
}
