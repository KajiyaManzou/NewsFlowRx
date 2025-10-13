using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BootstrapBlazor.Components;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NewsFlow.Pages;
using Xunit;

namespace NewsFlow.Tests;

/// <summary>
/// News.razor のユーザーインタラクションを検証するテスト
/// </summary>
public class NewsFlowInteractionTests : TestContext
{
    [Fact]
    public void SearchButtonClick_WithKeyword_ShowsArticles()
    {
        // 検索ボタンを押した場合に記事カードが描画されることを確認
        var jsonResponse = JsonSerializer.Serialize(new
        {
            status = "ok",
            totalResults = 1,
            articles = new[]
            {
                new
                {
                    source = new { name = "Test Source" },
                    author = "Author",
                    title = "Article",
                    description = "Description",
                    url = "https://example.com/article-1",
                    urlToImage = "https://example.com/image.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Content"
                }
            }
        });

        var root = RenderNewsRoot(CreateMockHttpClient(jsonResponse, HttpStatusCode.OK));

        root.Find("input[placeholder=\"例: AI 人工知能 技術\"]").Change("AI");
        root.FindAll("button").First(b => b.TextContent.Trim() == "検索").Click();

        root.WaitForAssertion(() =>
        {
            Assert.Single(root.FindAll(".article-card"));
            Assert.Contains("検索結果", root.Markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void KeywordInputEnterKey_TriggersSearch()
    {
        // Enter キーによる検索ショートカットが機能することを検証
        var jsonResponse = JsonSerializer.Serialize(new
        {
            status = "ok",
            totalResults = 2,
            articles = new[]
            {
                new
                {
                    source = new { name = "Source1" },
                    author = "Author1",
                    title = "Article1",
                    description = "Description1",
                    url = "https://example.com/article-1",
                    urlToImage = "https://example.com/image1.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Content1"
                },
                new
                {
                    source = new { name = "Source2" },
                    author = "Author2",
                    title = "Article2",
                    description = "Description2",
                    url = "https://example.com/article-2",
                    urlToImage = "https://example.com/image2.jpg",
                    publishedAt = "2025-01-02T00:00:00Z",
                    content = "Content2"
                }
            }
        });

        var root = RenderNewsRoot(CreateMockHttpClient(jsonResponse, HttpStatusCode.OK));

        root.Find("input[placeholder=\"例: AI 人工知能 技術\"]").Change("AI 人工知能");
        root.Find("input[placeholder=\"例: AI 人工知能 技術\"]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        root.WaitForAssertion(() => Assert.Equal(2, root.FindAll(".article-card").Count));
    }

    [Fact]
    public void ClearButtonClick_ResetsFormAndResults()
    {
        // クリア操作でフォームと内部状態が初期化されることを確認
        var jsonResponse = JsonSerializer.Serialize(new
        {
            status = "ok",
            totalResults = 1,
            articles = new[]
            {
                new
                {
                    source = new { name = "Source" },
                    author = "Author",
                    title = "Article",
                    description = "Description",
                    url = "https://example.com/article",
                    urlToImage = "https://example.com/image.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "Content"
                }
            }
        });

        var root = RenderNewsRoot(CreateMockHttpClient(jsonResponse, HttpStatusCode.OK));

        root.Find("input[placeholder=\"例: AI 人工知能 技術\"]").Change("AI");
        root.FindAll("button").First(b => b.TextContent.Trim() == "検索").Click();

        root.WaitForAssertion(() => Assert.NotEmpty(root.FindAll(".article-card")));

        root.FindAll("button").First(b => b.TextContent.Contains("クリア", StringComparison.Ordinal)).Click();

        root.WaitForAssertion(() =>
        {
            Assert.Empty(root.FindAll(".article-card"));
            var input = root.FindAll("input[placeholder=\"例: AI 人工知能 技術\"]").FirstOrDefault();
            Assert.NotNull(input);
            Assert.True(string.IsNullOrEmpty(input!.GetAttribute("value")));
        });

        var news = root.FindComponent<News>();
        Assert.Equal(string.Empty, GetPrivateField<string>(news.Instance, "searchKeywords"));
        Assert.Null(GetPrivateField<News.NewsApiResponse>(news.Instance, "searchResults"));
        Assert.Equal("jp", GetPrivateField<string>(news.Instance, "selectedLanguage"));
        Assert.Equal("publishedAt", GetPrivateField<string>(news.Instance, "selectedSortBy"));
    }

    [Fact]
    public async Task ArticleCardClick_NavigatesToUrl()
    {
        // 記事カードのクリックで外部リンクに遷移することを確認
        var root = RenderNewsRoot();
        var news = root.FindComponent<News>();

        var response = new News.NewsApiResponse
        {
            Status = "ok",
            TotalResults = 1,
            Articles = new List<News.Article>
            {
                new()
                {
                    Title = "タイトル",
                    Description = "説明",
                    Url = "https://example.com/article",
                    Source = new News.Source { Name = "Source" }
                }
            }
        };

        await news.InvokeAsync(() => SetPrivateField(news.Instance, "searchResults", response));
        news.Render();

        root.Find(".article-card").Click();

        var navigationManager = Services.GetRequiredService<FakeNavigationManager>();
        Assert.Equal("https://example.com/article", navigationManager.Uri);
    }

    private IRenderedComponent<BootstrapBlazorRoot> RenderNewsRoot(HttpClient? httpClient = null)
    {
        ConfigureTestServices(httpClient ?? CreateMockHttpClient("{}", HttpStatusCode.OK));
        return RenderComponent<BootstrapBlazorRoot>(parameters => parameters.AddChildContent<News>());
    }

    private void ConfigureTestServices(HttpClient httpClient)
    {
        Services.AddSingleton(httpClient);

        var configData = new Dictionary<string, string>
        {
            { "ApiKeys:NewsAPIKey", "test-api-key" },
            { "NewsAPIUrl", "https://newsapi.org/v2/everything" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        Services.AddSingleton<IConfiguration>(configuration);
        Services.AddBootstrapBlazor();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        return new HttpClient(mockHandler.Object);
    }

    private T? GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T?)field?.GetValue(instance);
    }

    private void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(instance, value);
    }
}
