namespace Miniblog.Domain.Models;

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string PostId { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsAdmin { get; set; } = false;

    public DateTime PubDate { get; set; } = DateTime.UtcNow;
}
