using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant.Core
{
	// LLM 客戶端抽象介面。
	// ViewModel 只依賴這個介面，實際接 Ollama、包一層繁體轉換、或用假資料，都由實作類別決定；
	// 單元測試與離線開發時可以換成 FakeLlmClient。
	// 兩個方法都收整段對話歷史（含最新一則 user），模型才有上下文可以接著聊。
	public interface ILlmClient
	{
		// 一次性回覆：等整段回覆產生完才回傳。
		// history：依時間排序的對話紀錄，最後一筆是使用者剛送出的訊息
		// cancellationToken：使用者按 Cancel 時觸發
		public Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken);

		// 串流回覆：模型每產生一小段就 yield 一次，呼叫端用 await foreach 逐段接收。
		public IAsyncEnumerable<string> StreamReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken);
	}
}
