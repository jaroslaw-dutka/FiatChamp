using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.Http
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttp(this IServiceCollection services, IConfiguration configuration) => services
            .AddHttpClient()
            .AddSingleton<PollyRequestHandler>()
            .AddSingleton(sp => new FlurlClientCache().WithDefaults(builder => builder.AddMiddleware(sp.GetRequiredService<PollyRequestHandler>)));
    }
}
