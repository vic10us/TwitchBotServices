﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TwitchBot.Service.Extensions
{
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