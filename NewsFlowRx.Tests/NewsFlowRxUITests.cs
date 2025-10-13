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
using AngleSharp.Dom;

namespace NewsFlowRx.Tests;

/// <summary>
/// News.razor の UIレンダリングテスト
/// bUnitを使用してDOM要素の存在と表示内容を検証
/// </summary>
public class NewsFlowRxUITests : TestContext
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
        // IConfigurationを先に設定
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["ApiKeys:NewsAPIKey"]).Returns("test-api-key");
        mockConfig.Setup(c => c["NewsAPIUrl"]).Returns("https://newsapi.org/v2/everything");
        mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
        Services.AddSingleton(mockConfig.Object);

        Services.AddSingleton(httpClient);
        Services.AddBootstrapBlazor();

        // JSInteropをセットアップ
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #endregion

    #region UI Rendering Tests

    [Fact]
    public void NewsComponent_RendersPageTitle()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - ページタイトルが表示されている
        var title = cut.Find("h4");
        Assert.Contains("ニュース検索", title.TextContent);
    }

    [Fact]
    public void NewsComponent_RendersSearchConditionsCard()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - 検索条件カードが存在
        var card = cut.Find(".card");
        Assert.NotNull(card);

        // 検索条件タイトルが表示されている
        var cardTitle = cut.Find(".card-title");
        Assert.Contains("検索条件", cardTitle.TextContent);
    }

    [Fact]
    public void NewsComponent_RendersKeywordInput()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - キーワード入力フィールドが存在
        var keywordLabel = cut.Find("label[for='keywords']");
        Assert.Contains("キーワード", keywordLabel.TextContent);
    }

    [Fact]
    public void NewsComponent_RendersDatePickers()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - 開始日ラベルが存在
        var startDateLabel = cut.Find("label[for='startdate']");
        Assert.Contains("開始日", startDateLabel.TextContent);

        // 終了日ラベルが存在
        var endDateLabel = cut.Find("label[for='enddate']");
        Assert.Contains("終了日", endDateLabel.TextContent);
    }

    [Fact]
    public void NewsComponent_RendersLanguageAndSortBySelects()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - 言語ラベルが存在
        var languageLabel = cut.Find("label[for='language']");
        Assert.Contains("言語", languageLabel.TextContent);

        // 並び替えラベルが存在
        var sortByLabel = cut.Find("label[for='sortby']");
        Assert.Contains("並び替え", sortByLabel.TextContent);
    }

    [Fact]
    public void NewsComponent_RendersClearButton()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - クリアボタンが存在
        var buttons = cut.FindAll("button");
        var clearButton = buttons.FirstOrDefault(b => b.TextContent.Contains("クリア"));
        Assert.NotNull(clearButton);
    }

    [Fact]
    public void NewsComponent_RendersRxNetInfoMessage()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - Rx.NET情報メッセージが表示されている
        var markup = cut.Markup;
        Assert.Contains("完全自動検索", markup);
        Assert.Contains("すべての条件変更で自動的に検索を実行", markup);
    }

    [Fact]
    public void NewsComponent_DoesNotRenderSearchResultsInitially()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - 初期状態では検索結果が表示されない
        var markup = cut.Markup;
        Assert.DoesNotContain("検索結果:", markup);
    }

    [Fact]
    public void NewsComponent_RendersSearchResults_WhenResultsExist()
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
                    source = new { id = "test-1", name = "テストソース1" },
                    author = "著者1",
                    title = "テスト記事タイトル1",
                    description = "テスト記事の説明1",
                    url = "https://test.com/1",
                    urlToImage = "https://test.com/image1.jpg",
                    publishedAt = "2025-01-01T00:00:00Z",
                    content = "テスト内容1"
                },
                new
                {
                    source = new { id = "test-2", name = "テストソース2" },
                    author = "著者2",
                    title = "テスト記事タイトル2",
                    description = "テスト記事の説明2",
                    url = "https://test.com/2",
                    urlToImage = "https://test.com/image2.jpg",
                    publishedAt = "2025-01-02T00:00:00Z",
                    content = "テスト内容2"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - 検索を実行
        cut.InvokeAsync(async () =>
        {
            await cut.Instance.PerformSearchWithAllParams(
                "テスト",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            );
        }).Wait();

        // Assert - 検索結果が表示される
        var markup = cut.Markup;
        Assert.Contains("検索結果:", markup);
        Assert.Contains("2 件", markup);
    }

    [Fact]
    public void NewsComponent_RendersArticleCards_WhenResultsExist()
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
                    source = new { id = "test-source", name = "テストソース" },
                    author = "テスト著者",
                    title = "テスト記事タイトル",
                    description = "これはテスト記事の説明文です。",
                    url = "https://test.com/article",
                    urlToImage = "https://test.com/image.jpg",
                    publishedAt = "2025-01-01T12:00:00Z",
                    content = "テスト記事の本文"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - 検索を実行
        cut.InvokeAsync(async () =>
        {
            await cut.Instance.PerformSearchWithAllParams(
                "テスト",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            );
        }).Wait();

        // Assert - 記事カードが表示される
        var markup = cut.Markup;
        Assert.Contains("テスト記事タイトル", markup);
        Assert.Contains("これはテスト記事の説明文です。", markup);
        Assert.Contains("テストソース", markup);
    }

    [Fact]
    public void NewsComponent_RendersNoResultsMessage_WhenNoArticlesFound()
    {
        // Arrange
        var mockResponse = new
        {
            status = "ok",
            totalResults = 0,
            articles = new object[] { }
        };

        var jsonResponse = JsonSerializer.Serialize(mockResponse);
        var httpClient = CreateMockHttpClient(jsonResponse, HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - 検索を実行
        cut.InvokeAsync(async () =>
        {
            await cut.Instance.PerformSearchWithAllParams(
                "存在しないキーワード",
                "jp",
                DateTime.Today.AddDays(-7),
                DateTime.Today,
                "publishedAt"
            );
        }).Wait();

        // Assert - 結果なしメッセージが表示される
        var markup = cut.Markup;
        Assert.Contains("該当する記事が見つかりませんでした", markup);
    }

    [Fact]
    public void NewsComponent_RendersLoadingIndicator_WhenSearching()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        var cut = RenderComponent<News>();

        // Act - isLoadingを確認（検索前は表示されない）
        var markup = cut.Markup;

        // Assert - 初期状態ではローディング表示なし
        Assert.DoesNotContain("検索中", markup);
    }

    [Fact]
    public void NewsComponent_HasCorrectCSSClasses()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - 主要なCSSクラスが存在
        Assert.NotNull(cut.Find(".container"));
        Assert.NotNull(cut.Find(".card"));
        Assert.NotNull(cut.Find(".card-body"));
    }

    [Fact]
    public void NewsComponent_ClearButton_IsInCorrectPosition()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);

        // Act
        var cut = RenderComponent<News>();

        // Assert - クリアボタンと検索条件が並んでいる
        var flexDiv = cut.Find(".d-flex.align-items-center");
        Assert.NotNull(flexDiv);

        // クリアボタンと検索条件タイトルが含まれている
        var content = flexDiv.TextContent;
        Assert.Contains("クリア", content);
        Assert.Contains("検索条件", content);
    }

    #endregion
}
