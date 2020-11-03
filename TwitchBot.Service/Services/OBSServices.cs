using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBS.WebSockets.Core;
using TwitchBot.Service.Features.MediatR;

namespace TwitchBot.Service.Services
{
    public class AppSettings
    {
        public int Version { get; set; }
        public string Name { get; set; }
        public string CurrentVideoSize { get; set; }
    }

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