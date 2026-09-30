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
	// 透過 HTTP 呼叫本機 Ollama（預設 http://localhost:11434）的 ILlmClient 實作。
	// HttpClient 由外部注入（含 BaseAddress），方便測試時換成假的 HttpMessageHandler。
	public class OllamaLlmClient : ILlmClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _model;
		private readonly string _systemPrompt;

		// httpClient：已設定 BaseAddress 的 HttpClient
		// model：Ollama 模型名稱，例如 "qwen2.5:3b"
		// systemPrompt：每次請求都會放在 messages 第一則的 system 指令
		public OllamaLlmClient (HttpClient httpClient, string model, string systemPrompt)
		{
			_httpClient = httpClient;
			_model = model;
			_systemPrompt = systemPrompt;
		}

		// 非串流模式：一次 POST，等完整回覆。
		public async Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken)
		{
			var request = CreateRequest(history, false);

			// 把 C# 物件轉成 JSON，用 POST 送出
			HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken).ConfigureAwait(false);

			// 確認 HTTP 狀態碼是成功的（200 這類），失敗就丟出 HttpRequestException
			response.EnsureSuccessStatusCode();

			// 把回應的 JSON 轉回 C# 物件
			OllamaChatResponse? result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken).ConfigureAwait(false);

			// 回應格式不對（沒有 message）視為錯誤，不要默默回傳空字串
			if (result?.Message == null)
				throw new InvalidOperationException ("Ollama 回應中沒有 message 欄位。");

			return result.Message.Content;
		}

		// 串流模式：Ollama 以 NDJSON（每行一個 JSON）逐段回傳，這裡逐行讀、逐行解析、逐段 yield。
		public async IAsyncEnumerable<string> StreamReplyAsync(IReadOnlyList<ChatMessage> history, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			var request = CreateRequest (history, true);

			// 串流不能用 PostAsJsonAsync（它會等整個 body 收完），要自己組 HttpRequestMessage
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

			// 直接拿回應的 Stream，包成 StreamReader 逐行讀
			using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			using var reader = new StreamReader(stream);

			string? line;
			// ReadLineAsync 回傳 null 表示串流結束（連線關閉）
			while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
			{
				// 跳過空行（NDJSON 之間可能夾雜）
				if (string.IsNullOrWhiteSpace (line))
					continue;

				OllamaChatResponse? chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);

				if (chunk == null)
					continue;

				// 有內容就吐給呼叫端；yield 之後會暫停在這裡，等呼叫端處理完再繼續讀下一行
				if (chunk.Message != null)
					yield return chunk.Message.Content;

				// done = true 代表模型已產生完畢，不用再等連線關閉
				if (chunk.Done)
					break;
			}
		}

		// 把自己的 MessageRole 轉成 Ollama API 要的字串；system 不在 enum 裡，由 CreateRequest 另外加。
		// 之後 enum 若新增成員而忘了對應，會在執行期丟 ArgumentOutOfRangeException 提醒。
		private static string ToOllamaRole(MessageRole role)
		{
			return role switch
			{
				MessageRole.User => "user",
				MessageRole.Assistant => "assistant",
				_ => throw new ArgumentOutOfRangeException(nameof(role), $"不支援的角色：{role}")
			};
		}

		// 組 /api/chat 的請求本體：第一則固定是 system prompt，後面接完整對話歷史。
		// GetReplyAsync 與 StreamReplyAsync 共用，只差 stream 旗標。
		private OllamaChatRequest CreateRequest(IReadOnlyList<ChatMessage> history, bool stream)
		{
			List<OllamaMessage> lstMessages = new List<OllamaMessage> ();
			lstMessages.Add (new OllamaMessage { Role = "system", Content = _systemPrompt });

			// 依序把 user / assistant 的歷史訊息轉成 Ollama 格式
			foreach (var item in history)
			{
				lstMessages.Add(new OllamaMessage
				{
					Role = ToOllamaRole (item.Role),
					Content = item.Content
				});
			}

			var request = new OllamaChatRequest
			{
				Model = _model,
				Messages = lstMessages,
				Stream = stream
			};

			return request;

		}
	}
}
