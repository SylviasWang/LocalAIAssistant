using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure
{
	// ILlmClient 的假實作：不連任何服務，只用 Task.Delay 模擬等待時間。
	// 用來在沒有 Ollama 的環境測試 UI 流程（Thinking 狀態、取消、串流逐字顯示）。
	public class FakeLlmClient : ILlmClient
	{
		// 等 3 秒後把使用者最後一則輸入原樣包起來回傳。
		public async Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
		{
			// ConfigureAwait(false)：這裡不需要回到 UI 執行緒，讓 await 之後在任何執行緒繼續即可
			await Task.Delay(3000, cancellationToken).ConfigureAwait(false);

			// history 最後一筆就是使用者剛送出的訊息
			string userMessage = history.Last().Content;

			return $"Receive [{userMessage}]";
		}

		// 模擬串流：每 0.5 秒吐出一個字，不看 history 內容。
		// [EnumeratorCancellation] 讓 await foreach 的 WithCancellation() 能把 token 傳進來。
		public async IAsyncEnumerable<string> StreamReplyAsync(IReadOnlyList<ChatMessage> history, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (var item in "這是假的串流回覆")
			{
				await Task.Delay(500, cancellationToken).ConfigureAwait(false);
				yield return item.ToString();
			}
		}
	}
}
