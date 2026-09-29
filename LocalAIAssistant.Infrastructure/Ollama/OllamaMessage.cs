using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	internal record OllamaMessage
	{
		[JsonPropertyName("role")]
		public string Role { get; init; } = string.Empty;

		[JsonPropertyName("content")]
		public string Content { get; init; } = string.Empty;
	}
}
