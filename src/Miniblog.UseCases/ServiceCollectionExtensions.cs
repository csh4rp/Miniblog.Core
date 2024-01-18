namespace Miniblog.UseCases;

using Abstract;

using Concrete;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPostService, PostService>()
            .AddScoped<ITagService, TagService>()
            .AddScoped<ICategoryService, CategoryService>();

        return serviceCollection;
    }
}
