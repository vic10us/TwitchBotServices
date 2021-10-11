#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        private readonly ILogger _logger;
        private readonly IEnumerable<INotification> _notifications;

        public NotifierMediatorService(IMediator mediator,
            IServiceProvider sp
            // , ILogger logger
            )
        {
            _mediator = mediator;
            
            var logger = sp.GetService<ILogger<NotifierMediatorService>>();
            _logger = logger;
            var type = typeof(INotification);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            var notifications = new List<INotification>();
            foreach (var type1 in types)
            {
                try
                {
                    var o = Activator.CreateInstance(type1);
                    notifications.Add((o as INotification)!);
                }
                catch
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
                if (args == null || args.Length == 0)
                {
                    _mediator.Publish(command);
                    continue;
                }

                if (command is INullCommand _)
                    _mediator.Publish(command);
                else if (command is IRedemptionCommand _ || command is IChatCommand _ ||
                         command is IChatMessageCommand _)
                    try
                    {
                        var type = command.GetType();
                        var param = Activator.CreateInstance(type, args);
                        _mediator.Publish(param!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to publish message....", ex);
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