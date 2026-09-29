using LocalAIAssistant.Infrastructure.Ollama;
using System.Text.Json;

namespace LocalAIAssistant.Tests
{
	public class OllamaLlmClientTests
	{
		[Fact]
		public async Task GetReplyAsync_ValidResponse_ReturnsMessageContent()
		{
			// 假的 Ollama 回應。三個引號 """ 是 C# 11 的原始字串，裡面的 " 不用跳脫
			string json = """{"message":{"role":"assistant","content":"你好"},"done":true}""";

			FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler(json);

			// 把假 handler 交給 HttpClient
			var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost:11434") };

			var client = new OllamaLlmClient(httpClient, "test-model");

			// Act
			string replyMsg = await client.GetReplyAsync ("Hi", CancellationToken.None);

			// Assert
			// 驗證兩個值相等：第一個參數是「預期值」，第二個是「實際值」
			Assert.Equal("你好", replyMsg);
		}

		[Fact]
		public async Task GetReplyAsync_SendsCorrectRequest()
		{
			// 假的 Ollama 回應。三個引號 """ 是 C# 11 的原始字串，裡面的 " 不用跳脫
			string json = """{"message":{"role":"assistant","content":"你好"},"done":true}""";

			FakeHttpMessageHandler fakeHttpMessageHandler = new FakeHttpMessageHandler(json);

			// 把假 handler 交給 HttpClient
			var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://localhost:11434") };

			var client = new OllamaLlmClient(httpClient, "test-model");

			// Act
			await client.GetReplyAsync("你好", CancellationToken.None);

			// Assert
			// 驗證兩個值相等：第一個參數是「預期值」，第二個是「實際值」
			Assert.Equal("/api/chat", fakeHttpMessageHandler.LastRequestPath);

			using JsonDocument doc = JsonDocument.Parse(fakeHttpMessageHandler.LastRequestBody!);   // 解析，用完要釋放，所以前面加 using
			JsonElement root = doc.RootElement;                        // 最外層的 { }

			string? model = root.GetProperty("model").GetString();    // 讀字串欄位
			Assert.Equal("test-model", model);

			bool stream = root.GetProperty("stream").GetBoolean();     // 讀布林欄位
			Assert.False(stream);

			JsonElement messages = root.GetProperty("messages");      // 讀陣列欄位
			int count = messages.GetArrayLength();                     // 陣列有幾筆
			Assert.Equal(1, count);

			JsonElement first = messages[0];                           // 陣列的第一筆
			Assert.Equal("user", first.GetProperty("role").GetString());
			Assert.Equal("你好", first.GetProperty("content").GetString());
		}
	}
}