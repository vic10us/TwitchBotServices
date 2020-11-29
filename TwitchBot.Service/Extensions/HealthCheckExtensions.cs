using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TwitchBot.Service.Extensions
{
    public class HealthStatus
    {
        public DateTimeOffset StartedAt { get; set; }
        public TimeSpan UpTime => DateTimeOffset.UtcNow - StartedAt;
    }

    public static class HealthMetrics
    {
        private static DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

        public static void Initialize() { }

        public static HealthStatus Status()
        {
            return new HealthStatus
            {
                StartedAt = _startedAt
            };
        }

        public static void AddHealthMetrics(this IApplicationBuilder app)
        {
            _startedAt = DateTimeOffset.UtcNow;
            Initialize();
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = WriteResponse
            });
        }

        public static Task WriteResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                status = result.Status,
                results = result.Entries,
                metrics = HealthMetrics.Status()
            };

            var d = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var body = JsonConvert.SerializeObject(response, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = d,
                Converters = new JsonConverter[] { new StringEnumConverter() }
            });

            return httpContext.Response.WriteAsync(body);
        }
    }
}
