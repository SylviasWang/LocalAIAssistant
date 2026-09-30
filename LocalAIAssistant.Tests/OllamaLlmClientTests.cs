using LocalAIAssistant.Core;
using LocalAIAssistant.Infrastructure.Ollama;
using System.Text.Json;

namespace LocalAIAssistant.Tests
{
	// OllamaLlmClient 的單元測試。
	// 透過 FakeHttpMessageHandler 攔截 HTTP，不需要真的啟動 Ollama。
	public class OllamaLlmClientTests
	{
		// 兩個測試共用的 system prompt，斷言時用同一個變數比對
		string systemPrompt = "test-system-prompt";

		// 回應 JSON 正確時，GetReplyAsync 應回傳 message.content。
		[Fact]
		public async Task GetReplyAsync_ValidResponse_ReturnsMessageContent()
		{
			// 假的 Ollama 回應。三個引號 """ 是 C# 11 的原始字串，裡面的 " 不用跳脫
			string json = """{"message":{"role":"assistant","content":"你好"},"done":true}""";

			FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler(json);

			// 把假 handler 交給 HttpClient
			var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost:11434") };

			var client = new OllamaLlmClient(httpClient, "test-model", systemPrompt);

			// Act
			var history = new List<ChatMessage>
			{
				new ChatMessage { Role = MessageRole.User, Content = "Hi" }
			};

			string replyMsg = await client.GetReplyAsync (history, CancellationToken.None);

			// Assert
			// 驗證兩個值相等：第一個參數是「預期值」，第二個是「實際值」
			Assert.Equal("你好", replyMsg);
		}

		// 檢查送出去的請求格式：路徑是 /api/chat、model 與 stream 正確、
		// messages 剛好兩則且第一則是 system、第二則是 user。
		[Fact]
		public async Task GetReplyAsync_SendsCorrectRequest()
		{
			// 假的 Ollama 回應。三個引號 """ 是 C# 11 的原始字串，裡面的 " 不用跳脫
			string json = """{"message":{"role":"assistant","content":"你好"},"done":true}""";

			FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler(json);

			// 把假 handler 交給 HttpClient
			var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost:11434") };

			var client = new OllamaLlmClient(httpClient, "test-model", systemPrompt);

			// Act
			var history = new List<ChatMessage>
			{
				new ChatMessage { Role = MessageRole.User,      Content = "Test1, hi, my name is Apple" },
				new ChatMessage { Role = MessageRole.Assistant, Content = "Test2, hi, Apple！" },
				new ChatMessage { Role = MessageRole.User,      Content = "Tes3, what is my name?" }
			};
			await client.GetReplyAsync(history, CancellationToken.None);

			// Assert
			// 驗證兩個值相等：第一個參數是「預期值」，第二個是「實際值」
			Assert.Equal("/api/chat", fakeHttpMessageHandler.LastRequestPath);

			// 不反序列化成 OllamaChatRequest（它是 internal），改用 JsonDocument 直接讀 JSON 節點
			using JsonDocument doc = JsonDocument.Parse(fakeHttpMessageHandler.LastRequestBody!);   // 解析，用完要釋放，所以前面加 using
			JsonElement root = doc.RootElement;                        // 最外層的 { }

			string? model = root.GetProperty("model").GetString();    // 讀字串欄位
			Assert.Equal("test-model", model);

			bool stream = root.GetProperty("stream").GetBoolean();     // 讀布林欄位
			Assert.False(stream);

			JsonElement messages = root.GetProperty("messages");      // 讀陣列欄位
			int count = messages.GetArrayLength();                     // 陣列有幾筆
			Assert.Equal(4, count);

			JsonElement first = messages[0];                           // 陣列的第一筆
			Assert.Equal("system", first.GetProperty("role").GetString());
			Assert.Equal(systemPrompt, first.GetProperty("content").GetString());

			JsonElement second = messages[1];                           // 陣列的第二筆
			Assert.Equal("user", second.GetProperty("role").GetString());
			Assert.Equal("Test1, hi, my name is Apple", second.GetProperty("content").GetString());

			JsonElement third = messages[2];                           // 陣列的第三筆
			Assert.Equal("assistant", third.GetProperty("role").GetString());
			Assert.Equal("Test2, hi, Apple！", third.GetProperty("content").GetString());

			JsonElement fourth = messages[3];                           // 陣列的第四筆
			Assert.Equal("user", fourth.GetProperty("role").GetString());
			Assert.Equal("Tes3, what is my name?", fourth.GetProperty("content").GetString());
		}
	}
}
