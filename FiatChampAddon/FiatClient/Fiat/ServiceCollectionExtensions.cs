using Microsoft.Extensions.DependencyInjection;

namespace FiatChamp.Fiat
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFiat(this IServiceCollection services)
        {
            return services;
        }
    }
}
