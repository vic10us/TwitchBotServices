using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ChatBotPrime.Core.Interfaces.Chat;
using ChatBotPrime.Core.Interfaces.Stream;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TwitchBot.Service.ChatCommands;

namespace TwitchBot.Service.Extensions
{
    public static class TwitchExtensions
    {
        public static IApplicationBuilder UseTwitchBot(this IApplicationBuilder builder)
        {
            var bot = builder.ApplicationServices.GetService<Services.TwitchBot>();
            return builder;
        }

        public static IServiceCollection AddTwitchCommands(this IServiceCollection services)
        {
            // TODO: Scrutor Pattern later?
            //var commands = Assembly.GetAssembly(typeof(IChatCommand))
            //    ?.GetTypes()
            //    .Where(x => x.Namespace != null && x.Namespace.StartsWith("TwitchBot"))
            //    .Where(x => x.IsClass)
            //    .Select(x => x);

            //if (commands == null) return services;

            //foreach (var cmd in commands)
            //{
            //    services.AddSingleton(sp => (IChatCommand) ActivatorUtilities.CreateInstance(sp, cmd));
            //}

            services.AddSingleton<IChatCommand, TwitchYeetCommand>();

            return services;
        }
    }

    public static class LogExtensions
    {
        public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            var serilogConfig = new LoggerConfiguration().ReadFrom.Configuration(configuration);

            Log.Logger = serilogConfig.CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            services.AddSingleton(Log.Logger);
            return services;
        }
    }
}
