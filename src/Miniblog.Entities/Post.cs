using System.Globalization;

namespace Miniblog.Domain;

public class Post
{
    public string Id { get; set; } = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);

    public string Content { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public DateTime PubDate { get; set; } = DateTime.UtcNow;

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public IList<Category> Categories { get; } = new List<Category>();

    public IList<Tag> Tags { get; } = new List<Tag>();

    public IList<Comment> Comments { get; } = new List<Comment>();
}
