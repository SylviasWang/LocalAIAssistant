using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	// /api/chat 的回應。
	// 非串流：只有一個物件，done = true。
	// 串流：每行一個物件（NDJSON），最後一行 done = true 且 message 可能為空。
	// Ollama 還會回其他欄位（created_at、eval_count…），這裡只取需要的兩個，其餘反序列化時自動忽略。
	internal record OllamaChatResponse
	{
		// 模型回覆；可能為 null（例如串流最後一個 done chunk）
		[JsonPropertyName("message")]
		public OllamaMessage? Message { get; init; }

		// 是否已產生完畢
		[JsonPropertyName("done")]
		public bool Done { get; init; }
	}
}
