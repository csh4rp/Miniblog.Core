namespace Miniblog.Infrastructure.Storage;

using Microsoft.Extensions.Options;

using System.Globalization;
using System.Text.RegularExpressions;

using UseCases.Abstract;

public class StorageService : IStorageService
{
    private const string Files = "files";
    private const string Posts = "Posts";
    private readonly string _folder;

    public StorageService(IOptions<StorageOptions> options) => _folder = Path.Combine(options.Value.RootPath, Posts);

    public async Task<string> SaveFileAsync(byte[] bytes,
        string fileName,
        CancellationToken cancellationToken)
    {
        var suffix = CleanFromInvalidChars(DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));

        var ext = Path.GetExtension(fileName);
        var name = CleanFromInvalidChars(Path.GetFileNameWithoutExtension(fileName));

        var fileNameWithSuffix = $"{name}_{suffix}{ext}";

        var absolute = Path.Combine(_folder, Files, fileNameWithSuffix);
        var dir = Path.GetDirectoryName(absolute)!;

        Directory.CreateDirectory(dir);
        await using (var writer = new FileStream(absolute, FileMode.CreateNew))
        {
            await writer.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        return $"/{Posts}/{Files}/{fileNameWithSuffix}";
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
