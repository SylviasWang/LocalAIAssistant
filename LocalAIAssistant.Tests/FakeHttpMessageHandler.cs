using System.Net;
using System.Text;

namespace LocalAIAssistant.Tests;

// 測試用的假 HttpMessageHandler：攔截 HttpClient 送出的請求，不真的連網路，
// 一律回傳建構時給的 JSON，並記錄最後一次請求的路徑與本體供測試斷言。
// 用法：new HttpClient(new FakeHttpMessageHandler(json)) { BaseAddress = ... }
public class FakeHttpMessageHandler : HttpMessageHandler
{
	// 要回給呼叫端的固定回應
	private readonly string _responseJson;

	// 最後一次請求的路徑，例如 "/api/chat"
	public string? LastRequestPath { get; private set; }

	// 最後一次請求的 body（JSON 字串）
	public string? LastRequestBody { get; private set; }

	public FakeHttpMessageHandler(string responseJson)
	{
		_responseJson = responseJson;
	}

	// HttpClient 每次送請求都會進到這裡。
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// 記下你的程式送出了什麼，給測試檢查
		LastRequestPath = request.RequestUri?.AbsolutePath;
		if (request.Content != null)
		{
			LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
		}

		// 不連網路，直接回傳寫好的 JSON
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
		};
	}
}
