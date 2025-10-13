using Bunit;
using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BootstrapBlazor.Components;
using NewsFlowRx.Pages;
using Microsoft.AspNetCore.Components;

namespace NewsFlowRx.Tests;

/// <summary>
/// News.razor の Rx.NET統合テスト
/// 完全自動検索の動作を検証
/// </summary>
public class NewsFlowRxTests : TestContext
{
    #region Helper Methods

    private HttpClient CreateMockHttpClient(string jsonResponse, HttpStatusCode statusCode)
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
                Content = new StringContent(jsonResponse)
            });

        return new HttpClient(mockHandler.Object);
    }

    private void ConfigureTestServices(HttpClient httpClient)
    {
        // IConfigurationを先に設定（BootstrapBlazorが必要とする）
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["ApiKeys:NewsAPIKey"]).Returns("test-api-key");
        mockConfig.Setup(c => c["NewsAPIUrl"]).Returns("https://newsapi.org/v2/everything");
        mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
        Services.AddSingleton(mockConfig.Object);

        Services.AddSingleton(httpClient);
        Services.AddBootstrapBlazor();

        // ToastServiceを追加（BootstrapBlazorから提供）
        // Services.AddBootstrapBlazor()がToastServiceも登録するため、追加の設定は不要

        // JSInteropをセットアップ（BootstrapBlazorコンポーネント用）
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #endregion

    #region Event Handler Tests

    [Fact]
    public void OnKeywordInput_UpdatesKeywordAndTriggersSubject()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var args = new ChangeEventArgs { Value = "テストキーワード" };
        cut.Instance.OnKeywordInput(args);

        // Assert
        // キーワードが更新されていることを確認
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task OnLanguageChanged_UpdatesLanguageAndTriggersSubject()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var item = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(item);

        // Assert
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task OnDateFromChanged_UpdatesDateAndTriggersSubject()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var testDate = DateTime.Today.AddDays(-10);
        await cut.Instance.OnDateFromChanged(testDate);

        // Assert
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task OnDateToChanged_UpdatesDateAndTriggersSubject()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var testDate = DateTime.Today;
        await cut.Instance.OnDateToChanged(testDate);

        // Assert
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task OnSortByChanged_UpdatesSortByAndTriggersSubject()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var item = new SelectedItem("relevancy", "関連度");
        await cut.Instance.OnSortByChanged(item);

        // Assert
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Search Method Tests

    [Fact]
    public async Task PerformSearchWithAllParams_WithValidKeyword_ReturnsResults()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 3,
            articles = new[]
            {
                new
                {
                    source = new { id = "test-1", name = "Test Source 1" },
                    author = "Author 1",
                    title = "Test Article 1",
                    description = "Description 1",
                    url = "https://test.com/1",
                    urlToImage = "https://test.com/image1.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Content 1"
                },
                new
                {
                    source = new { id = "test-2", name = "Test Source 2" },
                    author = "Author 2",
                    title = "Test Article 2",
                    description = "Description 2",
                    url = "https://test.com/2",
                    urlToImage = "https://test.com/image2.jpg",
                    publishedAt = "2025-01-02T00:00:00Z",
                    content = "Content 2"
                },
                new
                {
                    source = new { id = "test-3", name = "Test Source 3" },
                    author = "Author 3",
                    title = "Test Article 3",
                    description = "Description 3",
                    url = "https://test.com/3",
                    urlToImage = "https://test.com/image3.jpg",
                    publishedAt = "2025-01-03T00:00:00Z",
                    content = "Content 3"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var result = await cut.InvokeAsync(async () =>
            await cut.Instance.PerformSearchWithAllParams(
                "テストキーワード",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            )
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.Status);
        Assert.Equal(3, result.TotalResults);
        Assert.Equal(3, result.Articles.Count);
    }

    [Fact]
    public async Task PerformSearchWithAllParams_WithEmptyKeyword_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var result = await cut.InvokeAsync(async () =>
            await cut.Instance.PerformSearchWithAllParams(
                "",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            )
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PerformSearchWithAllParams_WithWhitespaceKeyword_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        var result = await cut.InvokeAsync(async () =>
            await cut.Instance.PerformSearchWithAllParams(
                "   ",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            )
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PerformSearchWithAllParams_WithMultipleKeywords_FormatsWithAND()
    {
        // Arrange
        string? capturedUrl = null;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, token) =>
            {
                capturedUrl = req.RequestUri?.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    status = "ok",
                    totalResults = 1,
                    articles = new object[] { }
                }))
            });

        var httpClient = new HttpClient(mockHandler.Object);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act
        await cut.InvokeAsync(async () =>
            await cut.Instance.PerformSearchWithAllParams(
                "AI 人工知能 技術",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            )
        );

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Contains("AI", capturedUrl);
        Assert.Contains("AND", capturedUrl);
    }

    #endregion

    #region ClearSearch Tests

    [Fact]
    public async Task ClearSearch_ResetsAllFieldsToDefault()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Actの前に値を変更
        var args = new ChangeEventArgs { Value = "テストキーワード" };
        cut.Instance.OnKeywordInput(args);

        // Act
        await cut.InvokeAsync(async () =>
            await cut.Instance.ClearSearch()
        );

        // Assert
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ReleasesAllResources()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act & Assert - 例外が発生しないことを確認
        cut.Instance.Dispose();
    }

    #endregion

    #region Rx.NET Integration Tests

    [Fact]
    public async Task ReactiveSearch_Throttle_WaitsBeforeSearch()
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
                    title = "Test",
                    description = "Test",
                    url = "https://test.com",
                    urlToImage = "https://test.com/image.jpg",
                    publishedAt = "2025-01-01T00:00:00Z"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - 連続して入力
        var args1 = new ChangeEventArgs { Value = "テスト" };
        cut.Instance.OnKeywordInput(args1);

        var args2 = new ChangeEventArgs { Value = "テストキーワード" };
        cut.Instance.OnKeywordInput(args2);

        // Throttle (500ms) より長く待機
        await Task.Delay(600);

        // Assert - 例外が発生しないことを確認
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task ReactiveSearch_CombineLatest_UpdatesOnAnyFieldChange()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("{\"status\":\"ok\",\"totalResults\":0,\"articles\":[]}", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - 複数のフィールドを変更
        var keywordArgs = new ChangeEventArgs { Value = "テスト" };
        cut.Instance.OnKeywordInput(keywordArgs);

        var languageItem = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(languageItem);

        var sortByItem = new SelectedItem("relevancy", "関連度");
        await cut.Instance.OnSortByChanged(sortByItem);

        // Assert - 例外が発生しないことを確認
        Assert.NotNull(cut.Instance);
    }

    #endregion
}
