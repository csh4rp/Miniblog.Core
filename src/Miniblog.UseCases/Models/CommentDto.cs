namespace Miniblog.UseCases.Models;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

public class CommentDto
{
    public string? Id { get; set; }

    public string Author { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public DateTime PubDate { get; set; }

    public string GetGravatar()
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(this.Email.Trim().ToLowerInvariant());
        var hashBytes = md5.ComputeHash(inputBytes);

        // Convert the byte array to hexadecimal string
        var sb = new StringBuilder();
        for (var i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2", CultureInfo.InvariantCulture));
        }

        return $"https://www.gravatar.com/avatar/{sb.ToString().ToLowerInvariant()}?s=60&d=blank";
    }
}
