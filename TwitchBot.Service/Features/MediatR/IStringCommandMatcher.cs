namespace TwitchBot.Service.Features.MediatR
{
    public interface IStringCommandMatcher
    {
        bool Match(string commandName);
    }
}