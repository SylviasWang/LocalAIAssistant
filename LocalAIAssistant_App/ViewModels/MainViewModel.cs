using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant_App.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
		private readonly ILlmClient _llmClient;

		public MainViewModel(ILlmClient llmClient)
		{
			_llmClient = llmClient;
		}

		public ObservableCollection<ChatMessage> Messages { get; } = new();
		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(SendCommand))]
		private string _inputText = string.Empty;

		[RelayCommand(CanExecute = nameof(CanSend), IncludeCancelCommand = true)]
		private async Task SendAsync(CancellationToken cancellationToken)
		{
			ChatMessage userMsg = new ChatMessage {Role= MessageRole.User, Content=InputText};
			Messages.Add (userMsg);

			InputText = string.Empty;

			ChatMessage replyMsg = new ChatMessage { Role = MessageRole.Assistant, Content = "Thinking..." };
			Messages.Add(replyMsg);

			bool hasReceivedText = false;
			try
			{
				//string getReplyMsg = await _llmClient.GetReplyAsync(userMsg.Content, cancellationToken);
				await foreach (string item in _llmClient.StreamReplyAsync(userMsg.Content, cancellationToken))
				{
					if (!hasReceivedText && item != string.Empty)
					{
						replyMsg.Content = string.Empty;
						hasReceivedText = true;
					}

					replyMsg.Content += item;
				}
			}
			catch (OperationCanceledException)
			{
				string errorMsg = "Canceled.";
				if (hasReceivedText)
					replyMsg.Content += $"\r\n{errorMsg}";
				else
					replyMsg.Content = errorMsg;
			}
			catch (HttpRequestException)
			{
				string errorMsg = "Cannot connect to Ollama. Please make sure Ollama is running.";
				if (hasReceivedText)
					replyMsg.Content += $"\r\n{errorMsg}";
				else
					replyMsg.Content = errorMsg;
			}

		}

		private bool CanSend() => !string.IsNullOrWhiteSpace(InputText);
	}
}
