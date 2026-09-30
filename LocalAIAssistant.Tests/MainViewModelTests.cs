using LocalAIAssistant.Core;
using LocalAIAssistant.ViewModels;

namespace LocalAIAssistant.Tests;

// MainViewModel 的單元測試。
// ViewModel 已獨立成不依賴 WPF 的專案，所以可以直接 new 出來、直接執行 Command，不需要開視窗。
// LLM 用 ScriptedLlmClient 代替，回覆內容與是否丟例外都由測試指定。
public class MainViewModelTests
{
	// 正常串流：使用者訊息與 AI 回覆各一則，三段串流內容要接成完整字串，Thinking... 佔位要被清掉，輸入框要清空。
	[Fact]
	public async Task SendAsync_StreamsReply_AddsUserAndAssistantMessages()
	{
		// Arrange：假的 LLM 會分三段回覆
		var llm = new ScriptedLlmClient(new[] { "你", "好", "！" });
		var viewModel = new MainViewModel(llm);
		viewModel.InputText = "嗨";

		// Act：直接執行 Command，不用開視窗、不用點按鈕
		await viewModel.SendCommand.ExecuteAsync(null);

		// Assert
		Assert.Equal(2, viewModel.Messages.Count);

		Assert.Equal(MessageRole.User, viewModel.Messages[0].Role);
		Assert.Equal("嗨", viewModel.Messages[0].Content);

		Assert.Equal(MessageRole.Assistant, viewModel.Messages[1].Role);
		Assert.Equal("你好！", viewModel.Messages[1].Content);   // 三段接起來，Thinking... 被清掉

		Assert.Equal(string.Empty, viewModel.InputText);           // 輸入框清空了
	}

	// 送給 LLM 的 history 必須在加入 Thinking... 之前複製，否則佔位訊息會被當成對話的一部分送出去。
	[Fact]
	public async Task SendAsync_PassesHistoryWithoutPlaceholder()
	{
		// Arrange
		var llm = new ScriptedLlmClient(new[] { "回覆" });
		var viewModel = new MainViewModel(llm);
		viewModel.InputText = "嗨";

		// Act
		await viewModel.SendCommand.ExecuteAsync(null);

		// Assert：LLM 收到的對話記錄只有使用者的問題，沒有 Thinking... 那一則
		Assert.NotNull(llm.LastHistory);
		Assert.Single(llm.LastHistory);                                   // 剛好 1 筆
		Assert.Equal(MessageRole.User, llm.LastHistory[0].Role);
		Assert.Equal("嗨", llm.LastHistory[0].Content);
	}

	// 回覆到一半斷線：已收到的文字要保留，錯誤訊息接在後面，而不是整則被取代。
	[Fact]
	public async Task SendAsync_ConnectionLostMidway_KeepsTextAndAppendsError()
	{
		// Arrange：假的 LLM 先送出「部分回覆」，然後丟出 HttpRequestException
		var llm = new ScriptedLlmClient(new[] { "部分回覆" }, new HttpRequestException());
		var viewModel = new MainViewModel(llm);
		viewModel.InputText = "嗨";

		// Act：直接執行 Command，不用開視窗、不用點按鈕
		await viewModel.SendCommand.ExecuteAsync(null);

		// Assert
		Assert.Equal(2, viewModel.Messages.Count);

		Assert.Equal(MessageRole.User, viewModel.Messages[0].Role);
		Assert.Equal("嗨", viewModel.Messages[0].Content);

		Assert.StartsWith("部分回覆", viewModel.Messages[1].Content);

		Assert.Contains("Cannot connect to Ollama", viewModel.Messages[1].Content);
	}
}
