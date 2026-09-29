using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure.Ollama
{
	internal record OllamaChatResponse
	{
		[JsonPropertyName("message")]
		public OllamaMessage? Message { get; init; }

		[JsonPropertyName("done")]
		public bool Done { get; init; }
	}
}
