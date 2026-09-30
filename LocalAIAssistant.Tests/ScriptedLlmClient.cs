using System.Runtime.CompilerServices;
using LocalAIAssistant.Core;

namespace LocalAIAssistant.Tests;

// 測試用的 ILlmClient：依照指定的內容回覆，不用等待，也可以指定丟出例外。
// 和 FakeLlmClient 的差別：這個沒有 Task.Delay，且回覆內容與例外都由測試決定，適合驗證 ViewModel 的流程。
public class ScriptedLlmClient : ILlmClient
{
	// 串流時要逐段吐出的內容；非串流時會全部接起來一次回傳
	private readonly IReadOnlyList<string> _chunks;

	// 不為 null 時，串流送完所有 chunks 後丟出這個例外
	private readonly Exception? _exception;

	// 最後一次收到的對話記錄，給測試檢查 ViewModel 傳了什麼
	public IReadOnlyList<ChatMessage>? LastHistory { get; private set; }

	// chunks：要回覆的片段
	// exception：串流結束後要丟的例外；預設 null 表示正常結束
	public ScriptedLlmClient(IReadOnlyList<string> chunks, Exception? exception = null)
	{
		_chunks = chunks;
		_exception = exception;
	}

	// 非串流：記下 history，把所有片段接成一個字串同步回傳。
	public Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
	{
		LastHistory = history;
		return Task.FromResult(string.Concat(_chunks));
	}

	// 串流：記下 history，逐段 yield；全部送完後若有指定例外才丟出，用來模擬「回覆到一半斷線」。
	public async IAsyncEnumerable<string> StreamReplyAsync(
		IReadOnlyList<ChatMessage> history,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		LastHistory = history;

		foreach (string chunk in _chunks)
		{
			await Task.Yield();          // 讓它真的是非同步，但不用等待
			yield return chunk;
		}

		if (_exception != null)
			throw _exception;            // 全部送完之後才丟例外，模擬「回覆到一半斷線」
	}
}
