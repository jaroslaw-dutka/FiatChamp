using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.App
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration configuration) => services
            .Configure<AppSettings>(configuration.GetSection("app"))
            .AddSingleton<IAppService, AppService>();
    }
}
