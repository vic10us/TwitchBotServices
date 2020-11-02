using Microsoft.AspNetCore.SignalR;

namespace TwitchBot.Service.Hubs
{
    public class TwitchHub : Hub
    {
        //private readonly TwitchAPI _twitchApi;

        //public async Task SendMessage(string user, string message)
        //{
        //    var uo = _twitchApi.Helix.Users.GetUsersAsync(logins: new List<string> {user}).Result.Users.FirstOrDefault();
        //    await Clients.All.SendAsync("ReceiveMessage", user, message, uo);
        //}

        //public async Task SendChatMessage(ChatMessageData data)
        //{
        //    var uo = _twitchApi.Helix.Users.GetUsersAsync(ids: new List<string> { data.UserId }).Result.Users.FirstOrDefault();
        //    data.LogoUrl = uo?.ProfileImageUrl;
        //    await Clients.All.SendAsync("ReceiveChatMessage", data, uo);
        //}
    }
}
