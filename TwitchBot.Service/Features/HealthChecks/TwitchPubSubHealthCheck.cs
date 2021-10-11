using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.HealthChecks
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TwitchPubSubHealthCheck : IHealthCheck
    {
        private readonly TwitchPubSubService _service;

        public TwitchPubSubHealthCheck(TwitchPubSubService service)
        {
            _service = service;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(_service.PubSubClient != null
                ? HealthCheckResult.Healthy("Twitch PubSub is UP!")
                : throw new Exception("Twitch PubSub Service is down!"));
        }
    }
}