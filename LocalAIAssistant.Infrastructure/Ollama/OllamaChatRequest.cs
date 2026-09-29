using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	internal record OllamaChatRequest
	{
		[JsonPropertyName("model")]
		public string Model { get; init; } = string.Empty;

		[JsonPropertyName("messages")]
		public List<OllamaMessage> Messages { get; init; } = new List<OllamaMessage>();

		[JsonPropertyName("stream")]
		public bool Stream { get; init; }
	}
}
