using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalAIAssistant.Core
{
	public partial class ChatMessage : ObservableObject
	{
		public MessageRole Role { get; init; }
		public DateTime Timestamp { get; init; } = DateTime.Now;
		[ObservableProperty]
		private string _content = string.Empty;
	}
}
