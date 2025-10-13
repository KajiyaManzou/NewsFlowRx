using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BootstrapBlazor.Components;
using Bunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NewsFlow.Pages;
using Xunit;

namespace NewsFlow.Tests;

/// <summary>
/// News.razor の UI レンダリングを検証する bUnit テスト
/// </summary>
public class NewsFlowUITests : TestContext
{
    [Fact]
    public void InitialRender_ShowsSearchFormElements()
    {
        // 初期表示で検索フォームに必要な入力・ボタンが揃っていることを確認
        var newsComponent = RenderNewsComponent();

        var header = newsComponent.Find("h4");
        Assert.Contains("ニュース検索", header.TextContent, StringComparison.Ordinal);

        Assert.NotNull(newsComponent.FindComponent<BootstrapInput<string>>());
        Assert.Equal(2, newsComponent.FindComponents<DateTimePicker<DateTime?>>().Count);
        Assert.NotNull(newsComponent.FindComponent<Select<string>>());

        var buttons = newsComponent.FindAll("button");
        Assert.Contains(buttons, b => b.TextContent.Contains("検索", StringComparison.Ordinal));
        Assert.Contains(buttons, b => b.TextContent.Contains("クリア", StringComparison.Ordinal));
    }

    [Fact]
    public void WhenSearchResultsPresent_RendersArticleCards()
    {
        // 成功レスポンスを注入し、検索結果カードが意図通りに並ぶことを検証
        var newsComponent = RenderNewsComponent();

        var response = new News.NewsApiResponse
        {
            Status = "ok",
            TotalResults = 2,
            Articles = new List<News.Article>
            {
                new()
                {
                    Title = "検索結果のタイトル1",
                    Description = "説明1",
                    Url = "https://example.com/article-1",
                    Source = new News.Source { Name = "Source1" }
                },
                new()
                {
                    Title = "検索結果のタイトル2",
                    Description = "説明2",
                    Url = "https://example.com/article-2",
                    Source = new News.Source { Name = "Source2" }
                }
            }
        };

        SetPrivateField(newsComponent.Instance, "searchResults", response);
        newsComponent.Render();

        var headings = newsComponent.FindAll("h6.card-title");
        Assert.True(headings.Count >= 2, "検索結果カードのタイトルが描画されていることを期待しています。");

        var resultHeading = headings[^1];
        Assert.Contains("検索結果", resultHeading.TextContent, StringComparison.Ordinal);

        var compactHeading = NormalizeText(resultHeading.TextContent);
        Assert.Contains("検索結果:2件", compactHeading, StringComparison.Ordinal);

        var articleCards = newsComponent.FindAll(".article-card");
        Assert.Equal(2, articleCards.Count);
        Assert.Contains("検索結果のタイトル1", articleCards[0].TextContent, StringComparison.Ordinal);
        Assert.Contains("検索結果のタイトル2", articleCards[1].TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void WhenSearchResultsEmpty_ShowsEmptyMessage()
    {
        // ヒット件数が 0 の場合に情報メッセージが表示されることを確認
        var newsComponent = RenderNewsComponent();

        var response = new News.NewsApiResponse
        {
            Status = "ok",
            TotalResults = 0,
            Articles = new List<News.Article>()
        };

        SetPrivateField(newsComponent.Instance, "searchResults", response);
        newsComponent.Render();

        var alert = newsComponent.Find(".alert.alert-info");
        Assert.Contains("該当する記事が見つかりませんでした。", alert.TextContent, StringComparison.Ordinal);
    }

    private IRenderedComponent<News> RenderNewsComponent()
    {
        ConfigureTestServices(CreateMockHttpClient("{}", HttpStatusCode.OK));

        var root = RenderComponent<BootstrapBlazorRoot>(parameters => parameters.AddChildContent<News>());
        return root.FindComponent<News>();
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

    private void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(instance, value);
    }

    private static string NormalizeText(string value) => string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
}
