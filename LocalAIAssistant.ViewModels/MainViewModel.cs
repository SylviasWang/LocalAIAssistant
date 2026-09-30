using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant.ViewModels
{
	// 主視窗的 ViewModel（CommunityToolkit.Mvvm）。
	// 負責：維護訊息清單、接收輸入、把整段對話歷史交給 ILlmClient 串流取得回覆並逐段更新畫面、處理取消與連線失敗。
	// partial 是因為 [ObservableProperty] 與 [RelayCommand] 會由 Source Generator 產生另一半程式碼。
	public partial class MainViewModel : ObservableObject
	{
		// 只依賴介面，實作由 MainWindow 注入
		private readonly ILlmClient _llmClient;

		public MainViewModel(ILlmClient llmClient)
		{
			_llmClient = llmClient;
		}

		// 聊天紀錄；ListBox 綁定這個集合，Add 之後畫面自動出現
		public ObservableCollection<ChatMessage> Messages { get; } = new();

		// 產生 public string InputText 屬性；每次變更會通知 SendCommand 重新評估 CanExecute，
		// 所以輸入框有字時 Send 才會亮起
		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(SendCommand))]
		private string _inputText = string.Empty;

		// Send 按鈕的命令本體。
		// IncludeCancelCommand = true 會額外產生 SendCancelCommand，按下時取消這裡收到的 cancellationToken。
		// 執行中 SendCommand 會自動停用，避免重複送出。
		[RelayCommand(CanExecute = nameof(CanSend), IncludeCancelCommand = true)]
		private async Task SendAsync(CancellationToken cancellationToken)
		{
			// 1. 先把使用者的訊息加進清單
			ChatMessage userMsg = new ChatMessage {Role= MessageRole.User, Content=InputText};
			Messages.Add (userMsg);

			// 2. 複製一份目前的對話當作 history 送給模型。
			//    要在加入 Thinking... 佔位訊息之前複製，否則會把空的 assistant 訊息一起送出去
			List<ChatMessage> history = Messages.ToList();

			// 3. 清空輸入框（同時讓 Send 變成停用）
			InputText = string.Empty;

			// 4. 先放一則佔位的 AI 訊息顯示 Thinking...，之後串流內容會覆蓋它
			ChatMessage replyMsg = new ChatMessage { Role = MessageRole.Assistant, Content = "Thinking..." };
			Messages.Add(replyMsg);

			// 記錄是否已收到任何文字，用來決定「Thinking...」何時要清掉，以及錯誤訊息要接在後面還是取代
			bool hasReceivedText = false;
			try
			{
				// 舊的非串流做法，保留參考
				//string getReplyMsg = await _llmClient.GetReplyAsync(history, cancellationToken);

				// 5. 逐段接收：每收到一段就 append 到 replyMsg.Content，因為 Content 是 ObservableProperty，畫面會即時更新
				await foreach (string item in _llmClient.StreamReplyAsync(history, cancellationToken))
				{
					// 第一段真正的文字到達時，先把 Thinking... 清掉
					if (!hasReceivedText && item != string.Empty)
					{
						replyMsg.Content = string.Empty;
						hasReceivedText = true;
					}

					replyMsg.Content += item;
				}
			}
			catch (HttpRequestException)
			{
				// 連不上 Ollama（沒啟動、port 不對、非 2xx 狀態碼）
				string errorMsg = "Cannot connect to Ollama. Please make sure Ollama is running.";
				if (hasReceivedText)
					replyMsg.Content += $"\r\n{errorMsg}";
				else
					replyMsg.Content = errorMsg;
			}
			catch (OperationCanceledException)
			{
				// 使用者按了 Cancel：已有部分回覆就在後面補一行，否則直接取代 Thinking...
				string errorMsg = "Canceled.";
				if (hasReceivedText)
					replyMsg.Content += $"\r\n{errorMsg}";
				else
					replyMsg.Content = errorMsg;
			}
		}

		// 輸入框有非空白內容時才允許送出。
		private bool CanSend() => !string.IsNullOrWhiteSpace(InputText);
	}
}
