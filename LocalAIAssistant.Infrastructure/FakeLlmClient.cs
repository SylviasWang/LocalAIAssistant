using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

		public async IAsyncEnumerable<string> StreamReplyAsync(string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			foreach (var item in "這是假的串流回覆")
			{
				await Task.Delay(500, cancellationToken).ConfigureAwait(false);
				yield return item.ToString();  
			}
		}
	}
}
