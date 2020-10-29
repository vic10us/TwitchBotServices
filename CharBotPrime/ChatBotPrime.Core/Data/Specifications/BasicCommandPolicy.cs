using ChatBotPrime.Core.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ChatBotPrime.Core.Data.Specifications
{
	public class BasicCommandPolicy : DataItemPolicy<BasicCommand>
	{
		protected BasicCommandPolicy(Expression<Func<BasicCommand, bool>> expression) : base(expression)
		{
			AddInclude(cw => cw.Aliases);
		}
	}
}
