using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Data.Model
{
	public class CommandAlias	: DataEntity
	{
		public virtual BasicCommand command { get; set; }
		public string Word { get; set; }
	}
}
