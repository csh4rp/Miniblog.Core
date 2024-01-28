namespace Miniblog.Infrastructure;

using DataAccess;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Storage;

using UseCases.Abstract;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<BlogDbContext>(c =>
        {
            c.UseSqlite("Data Source=blog.db;", b =>
            {
                b.MigrationsAssembly("Miniblog.Web");
            });
        });

        serviceCollection.AddScoped<IPostRepository, PostRepository>();
        serviceCollection.AddScoped<ITagRepository, TagRepository>();
        serviceCollection.AddScoped<ICategoryRepository, CategoryRepository>();

        return serviceCollection;
    }

    public static IServiceCollection AddStorage(this IServiceCollection serviceCollection,
        Action<StorageOptions> configure)
    {
        serviceCollection.AddOptions<StorageOptions>()
            .Configure(configure);

        serviceCollection.AddScoped<IStorageService, StorageService>();

        return serviceCollection;
    }
}
