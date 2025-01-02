using FiatChamp.App;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace FiatChamp.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogger(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = new AppSettings();
            configuration.GetSection("app").Bind(appSettings);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(appSettings.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();

            return services.AddLogging(i => i.AddSerilog(logger));
        }

        public static IServiceCollection AddHttp(this IServiceCollection services, IConfiguration configuration) => services
            .AddHttpClient()
            .AddSingleton<PollyRequestHandler>()
            .AddSingleton(sp => new FlurlClientCache().WithDefaults(builder => builder.AddMiddleware(sp.GetRequiredService<PollyRequestHandler>)));
    }
}
