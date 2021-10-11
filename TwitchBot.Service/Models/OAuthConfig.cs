namespace TwitchBot.Service.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OAuthConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Authority { get; set; }
        public string AccessToken { get; set; }
    }
}