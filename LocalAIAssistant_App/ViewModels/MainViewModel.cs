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

		[RelayCommand(CanExecute = nameof(CanSend))]
		private void Send()
		{
			ChatMessage userMsg = new ChatMessage {Role= MessageRole.User, Content=InputText};
			Messages.Add (userMsg);

			ChatMessage replyMsg = new ChatMessage { Role = MessageRole.Assistant, Content = "Receive [" + InputText + "]"};
			Messages.Add(replyMsg);

			InputText = string.Empty;
		}

		private bool CanSend() => !string.IsNullOrWhiteSpace(InputText);
	}
}
