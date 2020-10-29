using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Data.Model
{
	public class MessageAlias  : DataEntity
	{
		public MessageAlias()
		{}
		public MessageAlias(BasicMessage message, string word)
		{
			Message = message;
			Word = word;
		}
		public virtual BasicMessage Message { get; set; }
		public string Word { get; set; }
	}
}
