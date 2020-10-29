using ChatBotPrime.Core.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ChatBotPrime.Core.Data.Specifications
{
	public class BasicMessagePolicy : DataItemPolicy<BasicMessage>
	{
		protected BasicMessagePolicy(Expression<Func<BasicMessage, bool>> expression) : base(expression)
		{
			AddInclude(cw => cw.Aliases);
		}

		public static BasicMessagePolicy ByWord(string word)
		{
			return new BasicMessagePolicy(x => x.MessageText == word
										  || x.Aliases.Any(a => a.Word == word));
		}
	}

}
