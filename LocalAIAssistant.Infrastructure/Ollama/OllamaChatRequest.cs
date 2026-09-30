using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	// POST /api/chat 的請求本體。
	// 序列化後範例：{"model":"qwen2.5:3b","messages":[{"role":"system",...},{"role":"user",...}],"stream":false}
	internal record OllamaChatRequest
	{
		// 模型名稱，例如 "qwen2.5:3b"
		[JsonPropertyName("model")]
		public string Model { get; init; } = string.Empty;

		// 對話歷史；目前每次只送 system + 最新一則 user
		[JsonPropertyName("messages")]
		public List<OllamaMessage> Messages { get; init; } = new List<OllamaMessage>();

		// true：以 NDJSON 逐段回傳；false：等整段完成後一次回傳
		[JsonPropertyName("stream")]
		public bool Stream { get; init; }
	}
}
