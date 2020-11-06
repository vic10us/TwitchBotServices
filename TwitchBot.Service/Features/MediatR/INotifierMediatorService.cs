#nullable enable
using System.Collections.Generic;
using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public interface INotifierMediatorService
    {
        IEnumerable<INotification> GetCommands(string commandName);
        void NotifyPattern(string pattern, params object?[]? args);
        void Notify(string notifyText);
        void Notify(INotification obj);
    }
}
