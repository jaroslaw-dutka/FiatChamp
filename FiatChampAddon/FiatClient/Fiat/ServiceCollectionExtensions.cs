using FiatChamp.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FiatChamp.Fiat
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFiat(this IServiceCollection services, IConfiguration configuration) => services
            .Configure<FiatSettings>(configuration.GetSection("fiat"))
            .AddSingleton<FiatClient>()
            .AddSingleton<FiatClientFake>()
            .AddSingleton<IFiatClient>(s => s.GetService<IOptions<AppSettings>>().Value.FakeApi
                ? s.GetService<FiatClientFake>()
                : s.GetService<FiatClient>());
    }
}
