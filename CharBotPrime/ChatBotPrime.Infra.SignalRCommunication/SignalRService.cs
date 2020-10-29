using ChatBotPrime.Core.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ChatBotPrime.Infra.SignalRCommunication
{
	public class SignalRService
	{
		private HubConnection _hubConnection;
		private SignalRSettings _settings;

		public SignalRService(IOptions<ApplicationSettings> applicationSettingsAccessor)
		{
			_settings = applicationSettingsAccessor.Value.SignalRSettings;
			_hubConnection = new HubConnectionBuilder()
				.WithUrl(_settings.Endpoint)
				.WithAutomaticReconnect()
				.Build();

			Connect();
		}

		private void Connect()
		{
			_hubConnection.Closed += async (error) =>
			{
				await Task.Delay(new Random().Next(0, 5) * 1000);
				await _hubConnection.StartAsync();
			};
		}


	}
}
