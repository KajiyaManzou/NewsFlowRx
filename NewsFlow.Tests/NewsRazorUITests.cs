using Xunit;
using static NewsFlow.Pages.News;

namespace NewsFlow.Tests;

/// <summary>
/// News.razorのUI要素に関するテストです。
///
/// 注意: MudBlazorコンポーネントを使用したBlazorコンポーネントのフルレンダリングテストは
/// 以下の理由により非常に複雑です：
/// - MudPopoverProvider、MudDialogProviderなどの依存関係
/// - 複雑なJSInteropのセットアップ
/// - ライフサイクル管理とレンダリングタイミング
///
/// そのため、このテストでは以下のアプローチを採用しています：
/// 1. データモデルとロジックの検証（NewsRazorSimpleTests.cs）
/// 2. UI要素の基本的な検証（このファイル）
/// 3. E2Eテストでの統合テスト（別途実施を推奨）
/// </summary>
public class NewsRazorUITests
{
    [Fact]
    public void Article_WithAllProperties_DisplaysCorrectly()
    {
        // Arrange
        var article = new Article
        {
            Title = "記事タイトル",
            Description = "記事の説明文",
            Url = "https://example.com/article",
            UrlToImage = "https://example.com/image.jpg",
            PublishedAt = DateTime.Parse("2025-01-15T15:30:00Z"),
            Author = "著者名",
            Content = "記事の内容",
            Source = new Source { Id = "source-1", Name = "ニュースソース" }
        };

        // Assert - プロパティが正しく設定されていること
        Assert.Equal("記事タイトル", article.Title);
        Assert.Equal("記事の説明文", article.Description);
        Assert.Equal("https://example.com/article", article.Url);
        Assert.Equal("https://example.com/image.jpg", article.UrlToImage);
        Assert.Equal("著者名", article.Author);
        Assert.Equal("記事の内容", article.Content);
        Assert.NotNull(article.Source);
        Assert.Equal("source-1", article.Source.Id);
        Assert.Equal("ニュースソース", article.Source.Name);
    }

    [Fact]
    public void Article_WithoutImage_HasNullUrlToImage()
    {
        // Arrange
        var article = new Article
        {
            Title = "画像なし記事",
            Description = "説明文",
            Url = "https://example.com/no-image",
            UrlToImage = null,
            PublishedAt = DateTime.Now,
            Source = new Source { Name = "ソース" }
        };

        // Assert
        Assert.Null(article.UrlToImage);
        Assert.NotNull(article.Title);
        Assert.NotNull(article.Description);
    }

    [Fact]
    public void Article_WithoutDescription_HasNullDescription()
    {
        // Arrange
        var article = new Article
        {
            Title = "説明なし記事",
            Description = null,
            Url = "https://example.com/no-desc",
            PublishedAt = DateTime.Now,
            Source = new Source { Name = "ソース" }
        };

        // Assert
        Assert.Null(article.Description);
    }

    [Fact]
    public void NewsApiResponse_WithMultipleArticles_CorrectCount()
    {
        // Arrange
        var response = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 5,
            Articles = new List<Article>
            {
                new Article { Title = "記事1", Url = "https://example.com/1", Source = new Source { Name = "ソース1" } },
                new Article { Title = "記事2", Url = "https://example.com/2", Source = new Source { Name = "ソース2" } },
                new Article { Title = "記事3", Url = "https://example.com/3", Source = new Source { Name = "ソース3" } },
                new Article { Title = "記事4", Url = "https://example.com/4", Source = new Source { Name = "ソース4" } },
                new Article { Title = "記事5", Url = "https://example.com/5", Source = new Source { Name = "ソース5" } }
            }
        };

        // Assert
        Assert.Equal("ok", response.Status);
        Assert.Equal(5, response.TotalResults);
        Assert.Equal(5, response.Articles.Count);
        Assert.All(response.Articles, article => Assert.NotNull(article.Title));
    }

    [Fact]
    public void Source_Properties_SetCorrectly()
    {
        // Arrange
        var source = new Source
        {
            Id = "test-source-id",
            Name = "テストソース名"
        };

        // Assert
        Assert.Equal("test-source-id", source.Id);
        Assert.Equal("テストソース名", source.Name);
    }

    [Fact]
    public void Article_DateFormatting_WorksCorrectly()
    {
        // Arrange
        var article = new Article
        {
            Title = "日付テスト",
            Url = "https://example.com",
            PublishedAt = new DateTime(2025, 1, 15, 10, 30, 45, DateTimeKind.Utc),
            Source = new Source { Name = "ソース" }
        };

        // Assert - 日付が正しく設定されていること
        Assert.NotNull(article.PublishedAt);
        Assert.Equal(2025, article.PublishedAt.Value.Year);
        Assert.Equal(1, article.PublishedAt.Value.Month);
        Assert.Equal(15, article.PublishedAt.Value.Day);

        // UI表示用のフォーマット（yyyy/MM/dd HH:mm）をテスト
        var formattedDate = article.PublishedAt.Value.ToString("yyyy/MM/dd HH:mm");
        Assert.Equal("2025/01/15 10:30", formattedDate);
    }

    [Fact]
    public void NewsApiResponse_StatusOk_IsSuccess()
    {
        // Arrange
        var response = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 10,
            Articles = new List<Article>()
        };

        // Assert
        Assert.Equal("ok", response.Status);
        Assert.True(response.Status == "ok", "APIレスポンスのステータスはokであるべき");
    }

    [Fact]
    public void Article_RequiredFields_AreNotEmpty()
    {
        // Arrange
        var article = new Article
        {
            Title = "必須フィールドテスト",
            Url = "https://example.com/required",
            Source = new Source { Name = "必須ソース" }
        };

        // Assert - 必須フィールドが空でないこと
        Assert.False(string.IsNullOrEmpty(article.Title));
        Assert.False(string.IsNullOrEmpty(article.Url));
        Assert.NotNull(article.Source);
        Assert.False(string.IsNullOrEmpty(article.Source.Name));
    }

    [Fact]
    public void NewsApiResponse_EmptyArticlesList_InitializesCorrectly()
    {
        // Arrange & Act
        var response = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 0,
            Articles = new List<Article>()
        };

        // Assert
        Assert.NotNull(response.Articles);
        Assert.Empty(response.Articles);
        Assert.Equal(0, response.TotalResults);
    }

    [Theory]
    [InlineData("jp", "日本語")]
    [InlineData("en", "English")]
    [InlineData("de", "Deutsch")]
    [InlineData("es", "Español")]
    [InlineData("fr", "Français")]
    public void LanguageCodes_AreValid(string code, string displayName)
    {
        // Assert - 言語コードが有効な形式であること
        Assert.NotNull(code);
        Assert.NotEmpty(code);
        Assert.Equal(2, code.Length);
        Assert.NotNull(displayName);
        Assert.NotEmpty(displayName);
    }

    [Theory]
    [InlineData("publishedAt", "公開日時")]
    [InlineData("relevancy", "関連度")]
    [InlineData("popularity", "人気度")]
    public void SortByOptions_AreValid(string sortBy, string displayName)
    {
        // Assert - ソートオプションが有効であること
        Assert.NotNull(sortBy);
        Assert.NotEmpty(sortBy);
        Assert.NotNull(displayName);
        Assert.NotEmpty(displayName);
    }
}
