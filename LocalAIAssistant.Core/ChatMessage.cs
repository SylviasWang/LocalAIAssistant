namespace LocalAIAssistant.Core
{
	public class ChatMessage
	{
		public MessageRole Role { get; init; }
		public string Content { get; set; } = string.Empty;
		public DateTime Timestamp { get; init; } = DateTime.Now;
	}
}
