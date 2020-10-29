using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBotPrime.Core.Events.EventArguments
{
	public class ChatMessage
	{
        public ChatMessage() { }

		public ChatMessage(string message, bool isVip, bool isSubscriber, bool isModerator, bool isMe, bool isBroadcaster, int subscribedMonthCount, string id, string channel, int bits, bool isHighlighted, string userId, string username)
		{
			Message = message;
			IsVip = isVip;
			IsSubscriber = isSubscriber;
			IsModerator = isModerator;
			IsMe = isMe;
			IsBroadcaster = isBroadcaster;
			SubscribedMonthCount = subscribedMonthCount;
			Id = id;
			Channel = channel;
			Bits = bits;
			IsHighlighted = isHighlighted;
			UserId = userId;
			Username = username;
		}

		public string Message { get; }
		public bool IsVip { get; }
		public bool IsSubscriber { get; }
		public bool IsModerator { get; }
		public bool IsMe { get; }
		public bool IsBroadcaster { get; }
		public int SubscribedMonthCount { get; }
		public string Id { get; }
		public string Channel { get; }
		public double BitsInDollars { get; }
		public int Bits { get; }
		public bool IsHighlighted { get; }
		public string UserId { get; }
		public string Username { get; }
	}
}
