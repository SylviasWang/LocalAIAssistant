using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalAIAssistant.Core
{
	// 一則聊天訊息。
	// 繼承 ObservableObject 並把 Content 標成 [ObservableProperty]，
	// 串流回覆時每次 += 一段文字，畫面上的 TextBlock 就會即時更新。
	public partial class ChatMessage : ObservableObject
	{
		// 發送者；建立後不可變（init）
		public MessageRole Role { get; init; }

		// 建立時間，預設為當下
		public DateTime Timestamp { get; init; } = DateTime.Now;

		// Source Generator 會依這個欄位自動產生 public string Content 屬性，
		// setter 內含 PropertyChanged 通知
		[ObservableProperty]
		private string _content = string.Empty;
	}
}
