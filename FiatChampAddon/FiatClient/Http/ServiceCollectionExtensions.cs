using FiatChamp.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.Http
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttp(this IServiceCollection services, IConfiguration configuration) => services
            .Configure<MqttSettings>(configuration.GetSection("mqtt"))
            .AddSingleton<IMqttClient, MqttClient>();
    }
}
