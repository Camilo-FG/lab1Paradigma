using LibraryService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryService.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ILibrariesService, LibrariesService>();
        services.AddScoped<IBooksService, BooksService>();

        return services;
    }
}
