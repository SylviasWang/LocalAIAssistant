using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	// Ollama /api/chat 的單則訊息，請求與回應共用同一格式。
	// 用 record 表示純資料；[JsonPropertyName] 讓 C# 大寫屬性對應 JSON 小寫 key。
	// internal：只是 Ollama 的傳輸格式，不該外洩到 Core 或 UI。
	internal record OllamaMessage
	{
		// "system"、"user" 或 "assistant"
		[JsonPropertyName("role")]
		public string Role { get; init; } = string.Empty;

		// 訊息內容；串流模式下每個 chunk 只含一小段
		[JsonPropertyName("content")]
		public string Content { get; init; } = string.Empty;
	}
}
