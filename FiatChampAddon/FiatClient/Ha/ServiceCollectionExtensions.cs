using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.Ha
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHa(this IServiceCollection services, IConfiguration configuration) => services
            .Configure<HaSettings>(configuration.GetSection("ha"))
            .AddSingleton<IHaRestApi, HaRestApi>();
    }
}
