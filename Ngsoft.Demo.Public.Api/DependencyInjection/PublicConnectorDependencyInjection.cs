using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngsoft.Demo.Public.Api.Auth;
using Ngsoft.Demo.Public.Api.Connectors;
using System;

namespace Ngsoft.Demo.Public.Api.DependencyInjection
{
    public static class PublicConnectorDependencyInjection
    {
        public static IServiceCollection AddPublicConnector(this IServiceCollection services, string url, string username, string password)
        {
            services.AddHttpClient<IPublicConnector, PublicConnector>((client, sp) =>
            {
                client.BaseAddress = new Uri(url);
                var storage = new TokenMemoryStorage();
                var logger = sp.GetService<ILoggerFactory>().CreateLogger<PublicConnector>();
                return new PublicConnector(client, storage, username, password, logger);
            });
            services.AddSingleton<PublicConnector>();
            return services;
        }
    }
}
