namespace TwitchBot.Service.Models
{
    public class EmoteDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string ImageUrl { get; set; }

        public EmoteDto() { }

        public EmoteDto(string emoteId, string name, int emoteStartIndex, int emoteEndIndex)
        {
            Id = emoteId;
            Name = name;
            StartIndex = emoteStartIndex;
            EndIndex = emoteEndIndex;
            ImageUrl = "https://static-cdn.jtvnw.net/emoticons/v1/" + emoteId + "/1.0";
        }
    }
}