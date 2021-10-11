using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.HealthChecks
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TwitchClientHealthCheck : IHealthCheck
    {
        private readonly TwitchClientServices _service;

        public TwitchClientHealthCheck(TwitchClientServices service)
        {
            _service = service;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(_service.Client.IsConnected ?
                HealthCheckResult.Healthy("Twitch Client is UP!")
                : HealthCheckResult.Unhealthy("Twitch Client is DOWN!"));
        }
    }
}