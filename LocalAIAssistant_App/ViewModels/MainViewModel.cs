using CommunityToolkit.Mvvm.ComponentModel;
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
		private string _inputText = string.Empty;
	}
}
