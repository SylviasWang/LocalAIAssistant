using LocalAIAssistant.Infrastructure;
using LocalAIAssistant.Infrastructure.Ollama;
using LocalAIAssistant.ViewModels;
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
    // MVVM 架構下 View 的 code-behind 只負責組裝依賴並設定 DataContext，
    // 所有互動邏輯都在 MainViewModel。
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			// 沒有 Ollama 時可改用下面這行，用假資料測 UI
			//DataContext = new MainViewModel(new FakeLlmClient ());

			// Ollama 預設監聽 11434 port；BaseAddress 設好後 client 內只需寫相對路徑 /api/chat
			var httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:11434") };

			// 手動注入：HttpClient → OllamaLlmClient → TraditionalChineseLlmClient（輸出端強制繁體）→ MainViewModel
			// system prompt 要求模型跟隨使用者語言，中文一律回繁體（台灣用語）
			//DataContext = new MainViewModel(new OllamaLlmClient(httpClient, "qwen2.5:3b"
			//	, "You are a desktop AI assistant. Reply in the same language as the user's latest message.\r\n" +
			//	"If the user writes in English, reply in English.\r\n" +
			//	"If the user writes in Chinese, reply in Traditional Chinese (繁體中文，台灣用語), for example: 「這是一個設計模式，用來分離畫面與邏輯。」 " +
			//	"Never use Simplified Chinese characters."));

			DataContext = new MainViewModel(
				new TraditionalChineseLlmClient(new OllamaLlmClient(httpClient, "qwen2.5:3b"
				, "You are a desktop AI assistant. Reply in the same language as the user's latest message.\r\n" +
				"If the user writes in English, reply in English.\r\n" +
				"If the user writes in Chinese, reply in Traditional Chinese (繁體中文，台灣用語), for example: 「這是一個設計模式，用來分離畫面與邏輯。」 " +
				"Never use Simplified Chinese characters.")));
		}
    }
}
