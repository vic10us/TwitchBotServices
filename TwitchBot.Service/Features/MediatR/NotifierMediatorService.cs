#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace TwitchBot.Service.Features.MediatR
{
    public class CommandFactory : ICommandFactory
    {

    }

    public interface ICommandFactory
    {
    }

    public class NotifierMediatorService : INotifierMediatorService
    {
        private readonly IMediator _mediator;
        private readonly IEnumerable<INotification> _notifications;
        private readonly IEnumerable<Type> _types;

        public NotifierMediatorService(IMediator mediator)
        {
            _mediator = mediator;
            var type = typeof(INotification);
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            var notifications = new List<INotification>();
            foreach (var type1 in _types)
            {
                try
                {
                    var o = Activator.CreateInstance(type1);
                    notifications.Add(o as INotification);
                }
                catch(Exception ex)
                {
                    // do nothing for now... maybe log it later.
                }
            }
            _notifications = notifications;
        }

        public IEnumerable<INotification> GetCommands(string commandName)
        {
            var matches = _notifications.Where(n => n is IStringCommandMatcher matcher && matcher.Match(commandName));
            return matches;
        }

        public void NotifyPattern(string pattern, params object?[]? args)
        {
            var commands = GetCommands(pattern);
            foreach (var command in commands)
            {
                if (command is INullCommand || args == null || args.Length == 0) 
                    _mediator.Publish(command);
                else if (command is IChatCommand || command is IChatMessageCommand)
                {
                    var type = command.GetType();
                    var param = Activator.CreateInstance(type, args);
                    _mediator.Publish(param!);
                }
            }
        }

        public void Notify(string notifyText)
        {
            _mediator.Publish(new NotificationMessage { NotifyText = notifyText });
        }

        public void Notify(INotification obj)
        {
            _mediator.Publish(obj);
        }
    }
}