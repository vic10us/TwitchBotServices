using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace TwitchBot.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "config");
                    config.AddKeyPerFile(directoryPath: path, optional: true);
                    if (string.IsNullOrEmpty(hostingContext.HostingEnvironment.ApplicationName)) return;
                    var appAssembly = Assembly.GetExecutingAssembly();
                    config.AddUserSecrets(appAssembly, optional: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
