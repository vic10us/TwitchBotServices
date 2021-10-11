using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TwitchBot.Service.Extensions
{
    public static class TwitchExtensions
    {
        public static IApplicationBuilder UseTwitchBot(this IApplicationBuilder builder)
        {
            _ = builder.ApplicationServices.GetService<Services.TwitchBot>();
            return builder;
        }
    }
}
