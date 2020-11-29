using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBS.WebSockets.Core;
using TwitchBot.Service.Features.MediatR;
using TwitchBot.Service.Models;

namespace TwitchBot.Service.Services
{
    public class OBSServices
    {
        private readonly OBSConfig _obsConfig;
        private readonly IServiceProvider _services;
        private readonly INotifierMediatorService _notifierMediatorService;
        private readonly ILogger<OBSServices> _logger;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnReady;
        public bool IsReady { get; private set; } = false;
        public uint ConnectionAttempts { get; private set; } = 0;
        public uint ConnectionFailures { get; private set; } = 0;
        public DateTimeOffset? LastConnectionFailure { get; private set; } = null;
        public DateTimeOffset? LastSuccessfulConnection { get; private set; } = null;
        public DateTimeOffset? LastConnectionAttempt { get; private set; } = null;

        public OBSWebsocket OBSClient { get; private set; }

        public OBSServices(
            IServiceProvider services,
            IOptions<OBSConfig> obsConfig, 
            INotifierMediatorService notifierMediatorService, 
            ILogger<OBSServices> logger)
        {
            _obsConfig = obsConfig.Value;
            _services = services;
            _notifierMediatorService = notifierMediatorService;
            _logger = logger;
            var task = Task.Run(SetupOBS);
        }

        private void SetupOBS()
        {
            OBSClient = _services.GetRequiredService<OBSWebsocket>();
            _notifierMediatorService.Notify("OBS is being setup");
            OBSClient.Connected += OnObsConnected;
            OBSClient.Disconnected += OnObsDisconnected;
            ConnectOBS();
            OnReady?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectOBS()
        {
            try
            {
                if (!OBSClient.IsConnected)
                {
                    ConnectionAttempts++;
                    LastConnectionAttempt = DateTimeOffset.UtcNow;
                    OBSClient.Connect(_obsConfig.Connection.Url, _obsConfig.Connection.Password);
                }

                IsReady = true;
            }
            catch
            {
                // do nothing for now...
                _logger.LogWarning("Failed to connect to OBS Websockets");
            }
        }

        private void OnObsConnected(object sender, EventArgs e)
        {
            LastSuccessfulConnection = DateTimeOffset.UtcNow;
            var connected = this.OnConnected;
            connected?.Invoke(this, e);
        }

        private void OnObsDisconnected(object sender, EventArgs e)
        {
            ConnectionFailures++;
            LastConnectionFailure = DateTimeOffset.UtcNow;
            OnDisconnected?.Invoke(this, e);
            ConnectOBS();
        }

        public void TriggerHotKeyByName(string hotKeyName)
        {
            var request = new JObject { { "hotkeyName", hotKeyName } };

            OBSClient.SendRequest("TriggerHotkeyByName", request);
        }

        public AppSettings GetAppSettings()
        {
            var y = OBSClient.GetSourceSettings("settings.json");
            var appSettings = JsonConvert.DeserializeObject<AppSettings>(y.sourceSettings["text"]?.ToString() ?? "");
            return appSettings;
        }

        public void SaveAppSettings(AppSettings settings)
        {
            var y = OBSClient.GetSourceSettings("settings.json");
            y.sourceSettings["text"] = JsonConvert.SerializeObject(settings);
            OBSClient.SetSourceSettings("settings.json", y.sourceSettings);
        }
    }
}