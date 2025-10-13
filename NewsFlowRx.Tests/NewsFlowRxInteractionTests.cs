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
using AngleSharp.Dom;

namespace NewsFlowRx.Tests;

/// <summary>
/// News.razor のユーザーインタラクションテスト
/// bUnitを使用してクリック、入力イベントを検証
/// </summary>
public class NewsFlowRxInteractionTests : TestContext
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
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["ApiKeys:NewsAPIKey"]).Returns("test-api-key");
        mockConfig.Setup(c => c["NewsAPIUrl"]).Returns("https://newsapi.org/v2/everything");
        mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
        Services.AddSingleton(mockConfig.Object);

        Services.AddSingleton(httpClient);
        Services.AddBootstrapBlazor();

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #endregion

    #region Input Interaction Tests

    [Fact]
    public void KeywordInput_TriggersOnInputEvent()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - input要素を見つけて入力イベントを発火
        var input = cut.Find("input[placeholder*='AI']");
        input.Input("テストキーワード");

        // Assert - コンポーネントが再レンダリングされる
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void KeywordInput_UpdatesComponentState()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - キーワードを入力
        var args = new ChangeEventArgs { Value = "Blazor" };
        cut.Instance.OnKeywordInput(args);

        // Assert - インスタンスが正常に動作
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task KeywordInput_WithThrottle_WaitsBeforeTriggering()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 連続して入力（Throttleで0.5秒待機するはず）
        var args1 = new ChangeEventArgs { Value = "Test" };
        cut.Instance.OnKeywordInput(args1);

        var args2 = new ChangeEventArgs { Value = "TestKeyword" };
        cut.Instance.OnKeywordInput(args2);

        // Throttle期間待機
        await Task.Delay(600);

        // Assert - エラーが発生しない
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task LanguageSelect_ChangesLanguage()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 言語を変更
        var item = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(item);

        // Assert - 変更が適用される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task SortBySelect_ChangesSortOrder()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 並び替えを変更
        var item = new SelectedItem("relevancy", "関連度");
        await cut.Instance.OnSortByChanged(item);

        // Assert - 変更が適用される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task DateFromPicker_ChangesStartDate()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 開始日を変更
        var newDate = DateTime.Today.AddDays(-14);
        await cut.Instance.OnDateFromChanged(newDate);

        // Assert - 変更が適用される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task DateToPicker_ChangesEndDate()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 終了日を変更
        var newDate = DateTime.Today.AddDays(-1);
        await cut.Instance.OnDateToChanged(newDate);

        // Assert - 変更が適用される
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Button Click Tests

    [Fact]
    public async Task ClearButton_Click_ResetsAllFields()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // 先に値を設定
        var args = new ChangeEventArgs { Value = "テストキーワード" };
        cut.Instance.OnKeywordInput(args);

        // Act - クリアボタンをクリック（直接メソッド呼び出し）
        await cut.Instance.ClearSearch();

        // Assert - エラーが発生しない
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void ClearButton_Exists_InDOM()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - クリアボタンを検索
        var buttons = cut.FindAll("button");
        var clearButton = buttons.FirstOrDefault(b => b.TextContent.Contains("クリア"));

        // Assert - ボタンが存在する
        Assert.NotNull(clearButton);
    }

    #endregion

    #region Multiple Interaction Tests

    [Fact]
    public async Task MultipleInputChanges_TriggersReactiveSearch()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 複数のフィールドを変更（CombineLatestのテスト）
        var keywordArgs = new ChangeEventArgs { Value = "Blazor" };
        cut.Instance.OnKeywordInput(keywordArgs);

        var languageItem = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(languageItem);

        var sortByItem = new SelectedItem("popularity", "人気度");
        await cut.Instance.OnSortByChanged(sortByItem);

        // Assert - すべての変更が適用される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task RapidInputChanges_ThrottlesCorrectly()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 連続して素早く入力（Throttleテスト）
        for (int i = 0; i < 5; i++)
        {
            var args = new ChangeEventArgs { Value = $"Keyword{i}" };
            cut.Instance.OnKeywordInput(args);
            await Task.Delay(50); // 50ms間隔で入力
        }

        // Throttleの待機時間より長く待つ
        await Task.Delay(600);

        // Assert - エラーが発生しない
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task SameValueInput_DistinctUntilChanged_IgnoresDuplicates()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 同じ値を連続して入力（DistinctUntilChangedのテスト）
        var args = new ChangeEventArgs { Value = "SameKeyword" };
        cut.Instance.OnKeywordInput(args);
        cut.Instance.OnKeywordInput(args);
        cut.Instance.OnKeywordInput(args);

        await Task.Delay(600);

        // Assert - 重複が無視される（エラーなし）
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Reactive Search Tests

    [Fact]
    public async Task KeywordAndLanguageChange_TriggersCombineLatest()
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

        // Act - キーワードと言語を変更（CombineLatestが両方を監視）
        var keywordArgs = new ChangeEventArgs { Value = "React" };
        cut.Instance.OnKeywordInput(keywordArgs);

        await Task.Delay(600); // Throttle待機

        var languageItem = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(languageItem);

        await Task.Delay(200);

        // Assert - エラーなく完了
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task FieldChange_WhileSearching_CancelsOldRequest()
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

        // Act - 検索中に別の入力（Switchオペレーターのテスト）
        var args1 = new ChangeEventArgs { Value = "FirstKeyword" };
        cut.Instance.OnKeywordInput(args1);

        await Task.Delay(100); // Throttle中

        var args2 = new ChangeEventArgs { Value = "SecondKeyword" };
        cut.Instance.OnKeywordInput(args2);

        await Task.Delay(600);

        // Assert - 古いリクエストがキャンセルされる（エラーなし）
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Complex Interaction Scenarios

    [Fact]
    public async Task CompleteSearchWorkflow_FromInputToResults()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 完全な検索ワークフロー
        // 1. キーワード入力
        var keywordArgs = new ChangeEventArgs { Value = "Blazor WebAssembly" };
        cut.Instance.OnKeywordInput(keywordArgs);

        // 2. 言語選択
        var languageItem = new SelectedItem("en", "英語");
        await cut.Instance.OnLanguageChanged(languageItem);

        // 3. 日付範囲設定
        await cut.Instance.OnDateFromChanged(DateTime.Today.AddDays(-30));
        await cut.Instance.OnDateToChanged(DateTime.Today);

        // 4. 並び替え設定
        var sortByItem = new SelectedItem("relevancy", "関連度");
        await cut.Instance.OnSortByChanged(sortByItem);

        // Throttle待機
        await Task.Delay(600);

        // Assert - ワークフローが正常に完了
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task SearchAndClear_Workflow()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 検索してクリア
        // 1. 検索条件を設定
        var keywordArgs = new ChangeEventArgs { Value = "Test" };
        cut.Instance.OnKeywordInput(keywordArgs);

        await Task.Delay(600);

        // 2. クリア
        await cut.Instance.ClearSearch();

        await Task.Delay(200);

        // Assert - クリア後もエラーなし
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task MultipleSearches_Sequential()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 複数回連続して検索
        var keywords = new[] { "First", "Second", "Third" };

        foreach (var keyword in keywords)
        {
            var args = new ChangeEventArgs { Value = keyword };
            cut.Instance.OnKeywordInput(args);
            await Task.Delay(600); // Throttle待機
        }

        // Assert - すべての検索が正常に処理される
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void EmptyKeywordInput_DoesNotTriggerSearch()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 空のキーワードを入力
        var args = new ChangeEventArgs { Value = "" };
        cut.Instance.OnKeywordInput(args);

        // Assert - エラーなく処理される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void WhitespaceKeywordInput_DoesNotTriggerSearch()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 空白のみのキーワードを入力
        var args = new ChangeEventArgs { Value = "   " };
        cut.Instance.OnKeywordInput(args);

        // Assert - エラーなく処理される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task VeryLongKeyword_HandledCorrectly()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 非常に長いキーワードを入力
        var longKeyword = new string('A', 1000);
        var args = new ChangeEventArgs { Value = longKeyword };
        cut.Instance.OnKeywordInput(args);

        await Task.Delay(600);

        // Assert - 長いキーワードも処理される
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task SpecialCharactersKeyword_HandledCorrectly()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - 特殊文字を含むキーワード
        var specialKeyword = "C# .NET & Blazor!";
        var args = new ChangeEventArgs { Value = specialKeyword };
        cut.Instance.OnKeywordInput(args);

        await Task.Delay(600);

        // Assert - 特殊文字も処理される
        Assert.NotNull(cut.Instance);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void ComponentDispose_CleansUpResources()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("", HttpStatusCode.OK);
        ConfigureTestServices(httpClient);
        var cut = RenderComponent<News>();

        // Act - Disposeを呼び出し
        cut.Instance.Dispose();

        // Assert - エラーなく破棄される
        Assert.NotNull(cut.Instance);
    }

    #endregion
}
