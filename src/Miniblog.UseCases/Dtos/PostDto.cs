using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Miniblog.UseCases.Dtos;

public class PostDto
{
    public string? Id { get; set; }

    [Required]
    public string Content { get; set; } = default!;

    [Required]
    public string Excerpt { get; set; } = default!;

    public bool IsPublished { get; set; }

    public DateTime LastModified { get; set; }

    public DateTime PubDate { get; set; }

    [Required]
    public string Slug { get; set; } = default!;

    [Required]
    public string Title { get; set; } = default!;

    public IList<string> Categories { get; set; } = new List<string>();

    public IList<string> Tags { get; set; } = new List<string>();

    public IList<CommentDto> Comments { get; set; } = new List<CommentDto>();

    public string GetLink() => $"/blog/{this.Slug}/";

    public string RenderContent()
    {
        var result = this.Content;

        // Set up lazy loading of images/iframes
        if (!string.IsNullOrEmpty(result))
        {
            // Set up lazy loading of images/iframes
            var replacement = " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"";
            var pattern = "(<img.*?)(src=[\\\"|'])(?<src>.*?)([\\\"|'].*?[/]?>)";
            result = Regex.Replace(result, pattern, m => m.Groups[1].Value + replacement + m.Groups[4].Value + m.Groups[3].Value);

            // Youtube content embedded using this syntax: [youtube:xyzAbc123]
            var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
            result = Regex.Replace(
                result,
                @"\[youtube:(.*?)\]",
                m => string.Format(CultureInfo.InvariantCulture, video, m.Groups[1].Value));
        }

        return result;
    }

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "The slug should be lower case.")]
    public static string CreateSlug(string title)
    {
        title = title?.ToLowerInvariant().Replace(
            " ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
        title = RemoveDiacritics(title);
        title = RemoveReservedUrlCharacters(title);

        return title.ToLowerInvariant();
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string RemoveReservedUrlCharacters(string text)
    {
        var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

        foreach (var chr in reservedCharacters)
        {
            text = text.Replace(chr, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    public bool AreCommentsOpen(int commentsCloseAfterDays) =>
        this.PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;
}
