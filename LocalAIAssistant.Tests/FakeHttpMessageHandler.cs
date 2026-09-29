using System.Net;
using System.Text;

namespace LocalAIAssistant.Tests;

public class FakeHttpMessageHandler : HttpMessageHandler
{
	private readonly string _responseJson;

	public string? LastRequestPath { get; private set; }
	public string? LastRequestBody { get; private set; }

	public FakeHttpMessageHandler(string responseJson)
	{
		_responseJson = responseJson;
	}

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
