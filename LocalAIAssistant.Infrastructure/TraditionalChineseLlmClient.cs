using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure
{
	// Decorator：包住另一個 ILlmClient，把它的回覆轉成繁體中文。
	// 被包住的實作不需要知道這件事。
	// 目的是補強 system prompt：小模型有時仍會混出簡體字，這裡在輸出端再保險一次。
	public class TraditionalChineseLlmClient : ILlmClient
	{
		private readonly ILlmClient _inner;   // 被包住的那一個（例如 OllamaLlmClient）

		public TraditionalChineseLlmClient(ILlmClient inner)
		{
			_inner = inner;
		}

		// 非串流：整段拿回來後轉一次。
		public async Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
		{
			// 轉交給 inner，拿到結果後轉換
			string reply = await _inner.GetReplyAsync(history, cancellationToken).ConfigureAwait(false);
			return ChineseConverter.ToTraditional(reply);
		}

		// 串流：逐段轉換，維持即時顯示的效果。
		// LCMapStringEx 是字對字轉換，所以分段轉與整段轉的結果相同，不會因為切在詞中間而出錯。
		public async IAsyncEnumerable<string> StreamReplyAsync(
			IReadOnlyList<ChatMessage> history,
			[EnumeratorCancellation] CancellationToken cancellationToken)
		{
			// inner 每產生一段，就轉換一段再交出去
			await foreach (string chunk in _inner.StreamReplyAsync(history, cancellationToken).ConfigureAwait(false))
			{
				yield return ChineseConverter.ToTraditional(chunk);
			}
		}
	}
}
