using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant.Infrastructure
{
	public class FakeLlmClient : ILlmClient
	{
		public async Task<string> GetReplyAsync(string userMessage, CancellationToken cancellationToken)
		{
			await Task.Delay(3000, cancellationToken).ConfigureAwait(false);

			return $"Receive [{userMessage}]";
		}
	}
}
