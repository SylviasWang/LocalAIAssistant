using LocalAIAssistant.Infrastructure.Ollama;

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

			string replyMsg = await client.GetReplyAsync ("Hi", CancellationToken.None);

			// 驗證兩個值相等：第一個參數是「預期值」，第二個是「實際值」
			Assert.Equal("你好", replyMsg);
		}
	}
}