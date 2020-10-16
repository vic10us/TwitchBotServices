namespace TwitchBot.Service.Services
{
    public class TwitchChatConfig
    {
        public string PaswordGeneratorToken { get; set; }
        public string BotName { get; set; }
        public string Channel { get; set; }
        public bool RespondToUnknownCommand { get; set; }
    }
}