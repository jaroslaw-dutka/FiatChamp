using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog;
using FiatChamp.App;

namespace FiatChamp.Logging
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
    }
}
