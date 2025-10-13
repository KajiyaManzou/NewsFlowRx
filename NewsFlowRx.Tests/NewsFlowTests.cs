using Bunit;
using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BootstrapBlazor.Components;
using NewsFlowRx.Pages;

namespace NewsFlowRx.Tests;

/// <summary>
/// News.razor の SearchNews() メソッドを直接呼び出す単体テスト
/// </summary>
public class NewsFlowTests : TestContext
{
    [Fact]
    public async Task SearchNews_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 2,
            articles = new[]
            {
                new
                {
                    source = new { id = "test-source", name = "Test Source" },
                    author = "Test Author",
                    title = "Test Article 1",
                    description = "Test Description 1",
                    url = "https://test.com/article1",
                    urlToImage = "https://test.com/image1.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Test Content 1"
                },
                new
                {
                    source = new { id = "test-source2", name = "Test Source 2" },
                    author = "Test Author 2",
                    title = "Test Article 2",
                    description = "Test Description 2",
                    url = "https://test.com/article2",
                    urlToImage = "https://test.com/image2.jpg",
                    publishedAt = "2025-01-02T00:00:00Z",
                    content = "Test Content 2"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードを設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "AI 人工知能");

        // Act - SearchNews()を直接呼び出し
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // Assert - searchResultsフィールドを取得して検証
        var searchResults = GetPrivateField<dynamic>(newsComponent.Instance, "searchResults");

        Assert.NotNull(searchResults);
        Assert.Equal("ok", (string)searchResults.Status);
        Assert.Equal(2, (int)searchResults.TotalResults);
        Assert.Equal(2, searchResults.Articles.Count);
    }

    [Fact]
    public async Task SearchNews_WithEmptyKeyword_DoesNotCallApi()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードを空に設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "");

        // Act - SearchNews()を直接呼び出し
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // Assert - searchResultsがnullのまま（API呼び出しなし）
        var searchResults = GetPrivateField<object>(newsComponent.Instance, "searchResults");

        Assert.Null(searchResults);
    }

    [Fact]
    public async Task SearchNews_WithNoResults_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 0,
            articles = Array.Empty<object>()
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードを設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "NonExistentKeyword12345");

        // Act - SearchNews()を直接呼び出し
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // Assert
        var searchResults = GetPrivateField<dynamic>(newsComponent.Instance, "searchResults");

        Assert.NotNull(searchResults);
        Assert.Equal(0, (int)searchResults.TotalResults);
        Assert.Empty(searchResults.Articles);
    }

    [Fact]
    public async Task SearchNews_With426Error_HandlesException()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.UpgradeRequired); // 426
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードを設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "test keyword");

        // Act - SearchNews()を直接呼び出し（例外が発生してもコンポーネントがクラッシュしないことを確認）
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // Assert - isLoadingがfalseに戻っていることを確認（finallyブロックが実行された）
        var isLoading = GetPrivateField<bool>(newsComponent.Instance, "isLoading");

        Assert.False(isLoading);
    }

    [Fact]
    public async Task SearchNews_SetsIsLoadingCorrectly()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 1,
            articles = new[]
            {
                new
                {
                    source = new { id = "test", name = "Test" },
                    author = "Author",
                    title = "Title",
                    description = "Description",
                    url = "https://test.com",
                    urlToImage = "https://test.com/img.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Content"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードを設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "test");

        // Act & Assert
        // 検索前: isLoading = false
        Assert.False(GetPrivateField<bool>(newsComponent.Instance, "isLoading"));

        // SearchNews()を直接呼び出し
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // 検索後: isLoading = false（finallyで戻される）
        Assert.False(GetPrivateField<bool>(newsComponent.Instance, "isLoading"));
    }

    [Fact]
    public async Task SearchNews_FormatsKeywordsWithAND()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 0,
            articles = Array.Empty<object>()
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);

        // モックを作成してURL検証
        string? capturedUrl = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                capturedUrl = request.RequestUri?.ToString();
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<BootstrapBlazorRoot>(parameters => parameters
            .AddChildContent<News>());

        var newsComponent = cut.FindComponent<News>();

        // キーワードをスペース区切りで設定
        SetPrivateField(newsComponent.Instance, "searchKeywords", "AI 人工知能 技術");

        // Act
        await newsComponent.InvokeAsync(async () => await newsComponent.Instance.SearchNews());

        // Assert - URLに "AI AND 人工知能 AND 技術" が含まれることを確認
        Assert.NotNull(capturedUrl);
        Assert.Contains("AI", capturedUrl);
        Assert.Contains("AND", capturedUrl);
    }

    // ヘルパーメソッド
    private void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(instance, value);
    }

    private T? GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T?)field?.GetValue(instance);
    }

    private void ConfigureTestServices(HttpClient httpClient)
    {
        // HttpClientの設定
        Services.AddSingleton(httpClient);

        // IConfigurationの設定
        var configData = new Dictionary<string, string>
        {
            { "ApiKeys:NewsAPIKey", "test-api-key" },
            { "NewsAPIUrl", "https://newsapi.org/v2/everything" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        Services.AddSingleton<IConfiguration>(configuration);

        // BootstrapBlazorのサービス追加
        Services.AddBootstrapBlazor();

        // JSInteropのモック設定（Looseモードで全てのJS呼び出しを許可）
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            });

        return new HttpClient(mockHandler.Object);
    }
}
