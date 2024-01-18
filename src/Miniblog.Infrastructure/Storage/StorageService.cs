namespace Miniblog.Infrastructure.Storage;

using Microsoft.Extensions.Options;

using System.Globalization;
using System.Text.RegularExpressions;

using UseCases.Abstract;

public class StorageService : IStorageService
{
    private const string FILES = "files";

    private const string POSTS = "Posts";

    private readonly string folder;

    public StorageService(IOptions<StorageOptions> options) => this.folder = Path.Combine(options.Value.RootPath, POSTS);

    public async Task<string> SaveFileAsync(byte[] bytes,
        string fileName,
        CancellationToken cancellationToken)
    {
        var suffix = CleanFromInvalidChars(DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

        var ext = Path.GetExtension(fileName);
        var name = CleanFromInvalidChars(Path.GetFileNameWithoutExtension(fileName));

        var fileNameWithSuffix = $"{name}_{suffix}{ext}";

        var absolute = Path.Combine(this.folder, FILES, fileNameWithSuffix);
        var dir = Path.GetDirectoryName(absolute)!;

        Directory.CreateDirectory(dir);
        await using (var writer = new FileStream(absolute, FileMode.CreateNew))
        {
            await writer.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        return $"/{POSTS}/{FILES}/{fileNameWithSuffix}";
    }

    private static string CleanFromInvalidChars(string input)
    {
        // ToDo: what we are doing here if we switch the blog from windows to unix system or
        // vice versa? we should remove all invalid chars for both systems

        var regexSearch = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
        var r = new Regex($"[{regexSearch}]");
        return r.Replace(input, string.Empty);
    }
}
