using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatBotPrime.Core.Extensions
{

	public static class StringExtensions
	{
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source?.IndexOf(toCheck, comp) >= 0;
		}

		private static readonly Regex TokenFindingRegex = new Regex(@"\[\w+]");

		public static IEnumerable<string> FindTokens(this string src)
		{
			MatchCollection matches = TokenFindingRegex.Matches(src);
			IEnumerable<Match> matchesEnumerable = matches.OfType<Match>();
			return matchesEnumerable.Select(m => m.Value);
		}
	}

}
