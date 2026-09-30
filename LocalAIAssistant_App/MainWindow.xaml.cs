using LocalAIAssistant.Infrastructure;
using LocalAIAssistant.Infrastructure.Ollama;
using LocalAIAssistant_App.ViewModels;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalAIAssistant_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			//DataContext = new MainViewModel(new FakeLlmClient ());
			var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:11434") };

			DataContext = new MainViewModel(new OllamaLlmClient(httpClient, "qwen2.5:3b"
				, "You are a desktop AI assistant. Reply in the same language as the user's latest message.\r\n" +
				"If the user writes in English, reply in English.\r\n" +
				"If the user writes in Chinese, reply in Traditional Chinese (繁體中文，台灣用語), for example: 「這是一個設計模式，用來分離畫面與邏輯。」 " +
				"Never use Simplified Chinese characters."));
		}
    }
}