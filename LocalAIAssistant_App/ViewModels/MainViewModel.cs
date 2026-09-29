using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalAIAssistant.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalAIAssistant_App.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
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

			try
			{
				await Task.Delay(3000, cancellationToken);

				replyMsg.Content = "Receive [" + userMsg.Content + "]";
			}
			catch (OperationCanceledException)
			{
				replyMsg.Content = "Canceled.";
			}
			
		}

		private bool CanSend() => !string.IsNullOrWhiteSpace(InputText);
	}
}
