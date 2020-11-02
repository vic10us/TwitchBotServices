using System;
using Microsoft.Extensions.Options;
using OBS.WebSockets.Core;
using TwitchBot.Service.Features.MediatR;

namespace TwitchBot.Service.Services
{
    public class OBSServices
    {
        private readonly OBSConfig _obsConfig;
        private readonly INotifierMediatorService _notifierMediatorService;
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public OBSWebsocket OBSClient { get; }

        public OBSServices(OBSWebsocket obs, IOptions<OBSConfig> obsConfig, INotifierMediatorService notifierMediatorService)
        {
            OBSClient = obs;
            _obsConfig = obsConfig.Value;
            _notifierMediatorService = notifierMediatorService;
            SetupOBS();
        }

        private void SetupOBS()
        {
            _notifierMediatorService.Notify("OBS is being setup");
            OBSClient.Connected += OnObsConnected;
            OBSClient.Disconnected += OnObsDisconnected;
            ConnectOBS();
        }

        private void ConnectOBS()
        {
            if (!OBSClient.IsConnected) OBSClient.Connect(_obsConfig.Connection.Url, _obsConfig.Connection.Password);
        }

        private void OnObsConnected(object sender, EventArgs e)
        {
            var connected = this.Connected;
            connected?.Invoke(this, e);
        }

        private void OnObsDisconnected(object sender, EventArgs e)
        {
            var disconnected = this.Disconnected;
            disconnected?.Invoke(this, e);
        }
    }
}