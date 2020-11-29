using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TwitchBot.Service.Services;

namespace TwitchBot.Service.Features.HealthChecks
{
    public class OBSWebSocketsHealthCheck : IHealthCheck
    {
        private readonly OBSServices _service;

        public OBSWebSocketsHealthCheck(OBSServices service)
        {
            _service = service;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var data = new Dictionary<string, object> { };

            var isOk = CheckService(data);
            
            return Task.FromResult(isOk ? 
                  HealthCheckResult.Healthy("OBS WebSockets are Healthy", data) 
                : HealthCheckResult.Degraded("OBS WebSockets are not Healthy", null, data));
        }

        private bool CheckService(IDictionary<string, object> dictionary)
        {
            dictionary.Add("client_created", _service.OBSClient != null);
            dictionary.Add("client_ready", _service.IsReady);
            dictionary.Add("client_connected", _service.OBSClient?.IsConnected ?? false);
            dictionary.Add("client_connection_attempts", _service.ConnectionAttempts);
            dictionary.Add("client_connection_failures", _service.ConnectionFailures);
            dictionary.Add("client_last_connection_failure", _service.LastConnectionFailure);
            dictionary.Add("client_last_connection_attempt", _service.LastConnectionAttempt);
            dictionary.Add("client_last_successful_connection", _service.LastSuccessfulConnection);

            return _service.OBSClient != null && _service.IsReady && _service.OBSClient.IsConnected;
        }
    }
}
