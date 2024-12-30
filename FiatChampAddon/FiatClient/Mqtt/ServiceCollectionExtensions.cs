using FiatChamp.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqtt(this IServiceCollection services, IConfiguration configuration) => services
            .AddHttpClient()
            .AddSingleton<PollyRequestHandler>()
            .AddSingleton<IFlurlClientCache>(sp => new FlurlClientCache().WithDefaults(builder => builder.AddMiddleware(sp.GetRequiredService<PollyRequestHandler>)));
    }
}
