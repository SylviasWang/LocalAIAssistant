using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	public class OllamaLlmClient : ILlmClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _model;
		private readonly string _systemPrompt;

		public OllamaLlmClient (HttpClient httpClient, string model, string systemPrompt)
		{
			_httpClient = httpClient;
			_model = model;
			_systemPrompt = systemPrompt;
		}

		public async Task<string> GetReplyAsync(string userMessage, CancellationToken cancellationToken)
		{
			var request = new OllamaChatRequest
			{
				Model = _model,
				Messages = new List<OllamaMessage>
				{
					new OllamaMessage { Role = "system", Content = _systemPrompt }, 
					new OllamaMessage { Role = "user", Content = userMessage }
				},
				Stream = false
			};

			// 把 C# 物件轉成 JSON，用 POST 送出
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken).ConfigureAwait(false);

			// 確認 HTTP 狀態碼是成功的（200 這類），失敗就丟出 HttpRequestException
			response.EnsureSuccessStatusCode();

			// 把回應的 JSON 轉回 C# 物件
			OllamaChatResponse? result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken).ConfigureAwait(false);

			if (result?.Message == null)
				throw new InvalidOperationException ("Ollama 回應中沒有 message 欄位。");

			return result.Message.Content;
		}

		public async IAsyncEnumerable<string> StreamReplyAsync(string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var request = new OllamaChatRequest
			{
				Model = _model,
				Messages = new List<OllamaMessage>
				{
					new OllamaMessage { Role = "system", Content = _systemPrompt },
					new OllamaMessage { Role = "user", Content = userMessage }
				},
				Stream = true
			};

			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
			{
				Content = JsonContent.Create(request)          // 把 C# 物件轉成 JSON 內容
			};

			using HttpResponseMessage response = await _httpClient.SendAsync(
				httpRequest,
				HttpCompletionOption.ResponseHeadersRead,      // ← 關鍵：收到標頭就返回
				cancellationToken).ConfigureAwait(false);

			// 確認 HTTP 狀態碼是成功的（200 這類），失敗就丟出 HttpRequestException
			response.EnsureSuccessStatusCode();

			using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			using var reader = new StreamReader(stream);

			string? line;
			while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
			{
				if (string.IsNullOrWhiteSpace (line))
					continue;

				OllamaChatResponse? chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);

				if (chunk == null)
					continue;

				if (chunk.Message != null)
					yield return chunk.Message.Content;

				if (chunk.Done)
					break;
			}
		}
	}
}
