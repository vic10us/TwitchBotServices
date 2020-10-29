using ChatBotPrime.Core.Data;
using ChatBotPrime.Core.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatBotPrime.Infra.Data.EF
{
	public static class SetupDatabase
	{
		public static void Configure(AppDataContext appDataContext, IRepository repository)
		{
			EnsureDatabase(appDataContext);
			EnsureInitialData(repository);
		}

		private static void EnsureDatabase(AppDataContext appDataContext)
		{
			appDataContext.Database.Migrate();
		}

		private static void EnsureInitialData(IRepository repository)
		{
			if (!repository.ListAsync<BasicCommand>().Result.Any())
			{
				var ping = new BasicCommand("Ping", "Pong");
				repository.CreateAsync(ping);
			}

			if (!repository.ListAsync<BasicMessage>().Result.Any())
			{
				var greet = new BasicMessage("Hello", "Welcome [UserDisplayName] to the chat please join us for some fun");
				var greetAliases = new List<MessageAlias>
				{
					new MessageAlias(greet,"Hi "),
					new MessageAlias(greet, "Hey")
				};

				greet.Aliases = greetAliases;

				repository.CreateAsync(greet);
			}
		}
	}
}
