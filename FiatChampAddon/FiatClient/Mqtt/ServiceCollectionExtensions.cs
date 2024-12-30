using FiatChamp.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FiatChamp.Mqtt
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqtt(this IServiceCollection services, IConfiguration configuration) => services
            .AddHttpClient()
            .AddSingleton(i => new FlurlClientCache().WithDefaults(builder => builder.ConfigureInnerHandler(handler => new PollyRequestHandler(i.GetRequiredService<ILogger<PollyRequestHandler>>(), handler))));
    }
}
