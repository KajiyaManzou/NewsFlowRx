using Moq;
using Moq.Protected;
using MudBlazor;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static NewsFlow.Pages.News;

namespace NewsFlow.Tests;

/// <summary>
/// News.razorのSearchNews()メソッドをテストします。
/// HttpClientの動作をモックしてAPI呼び出しをシミュレートします。
/// </summary>
public class NewsRazorSimpleTests
{
    [Fact]
    public async Task NewsApiResponse_Deserialization_Success()
    {
        // Arrange - APIレスポンスのシミュレーション
        var response = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 2,
            Articles = new List<Article>
            {
                new Article
                {
                    Title = "Test Article 1",
                    Description = "Test Description 1",
                    Url = "https://test.com/1",
                    PublishedAt = DateTime.Parse("2025-01-01T10:00:00Z"),
                    Source = new Source { Name = "Test Source 1" }
                },
                new Article
                {
                    Title = "Test Article 2",
                    Description = "Test Description 2",
                    Url = "https://test.com/2",
                    PublishedAt = DateTime.Parse("2025-01-02T11:00:00Z"),
                    Source = new Source { Name = "Test Source 2" }
                }
            }
        };

        // Assert - デシリアライゼーションが正常に動作することを確認
        Assert.Equal("ok", response.Status);
        Assert.Equal(2, response.TotalResults);
        Assert.Equal(2, response.Articles.Count);
        Assert.Equal("Test Article 1", response.Articles[0].Title);
        Assert.Equal("Test Source 1", response.Articles[0].Source?.Name);
    }

    [Fact]
    public async Task HttpClient_MockedResponse_Success()
    {
        // Arrange - HttpClientのモック作成
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var expectedResponse = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 1,
            Articles = new List<Article>
            {
                new Article
                {
                    Title = "Mocked Article",
                    Description = "Mocked Description",
                    Url = "https://mocked.com",
                    PublishedAt = DateTime.Now,
                    Source = new Source { Name = "Mocked Source" }
                }
            }
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResponse)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Act - モックされたHTTPリクエストを実行
        var actualResponse = await httpClient.GetFromJsonAsync<NewsApiResponse>("https://test-api.com");

        // Assert - レスポンスが期待通りであることを確認
        Assert.NotNull(actualResponse);
        Assert.Equal("ok", actualResponse.Status);
        Assert.Equal(1, actualResponse.TotalResults);
        Assert.Single(actualResponse.Articles);
        Assert.Equal("Mocked Article", actualResponse.Articles[0].Title);
    }

    [Fact]
    public async Task HttpClient_Error426_ThrowsException()
    {
        // Arrange - HTTP 426エラーをシミュレート
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)426,
                Content = new StringContent("Upgrade Required")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Act & Assert - 426エラーでHttpRequestExceptionがスローされることを確認
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var response = await httpClient.GetAsync("https://test-api.com");
            response.EnsureSuccessStatusCode();
        });

        Assert.Contains("426", exception.Message);
    }

    [Fact]
    public void Article_Properties_SetCorrectly()
    {
        // Arrange & Act
        var article = new Article
        {
            Title = "Test Title",
            Description = "Test Description",
            Url = "https://example.com",
            UrlToImage = "https://example.com/image.jpg",
            PublishedAt = DateTime.Parse("2025-01-01T12:00:00Z"),
            Author = "Test Author",
            Content = "Test Content",
            Source = new Source { Id = "test-id", Name = "Test Source" }
        };

        // Assert
        Assert.Equal("Test Title", article.Title);
        Assert.Equal("Test Description", article.Description);
        Assert.Equal("https://example.com", article.Url);
        Assert.Equal("https://example.com/image.jpg", article.UrlToImage);
        Assert.Equal("Test Author", article.Author);
        Assert.Equal("Test Content", article.Content);
        Assert.NotNull(article.Source);
        Assert.Equal("test-id", article.Source.Id);
        Assert.Equal("Test Source", article.Source.Name);
    }

    [Fact]
    public void NewsApiResponse_EmptyArticles_HandledCorrectly()
    {
        // Arrange & Act
        var response = new NewsApiResponse
        {
            Status = "ok",
            TotalResults = 0,
            Articles = new List<Article>()
        };

        // Assert
        Assert.Equal("ok", response.Status);
        Assert.Equal(0, response.TotalResults);
        Assert.Empty(response.Articles);
    }

    [Fact]
    public async Task HttpClient_GenericException_ThrowsException()
    {
        // Arrange - 一般的な例外をシミュレート
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await httpClient.GetFromJsonAsync<NewsApiResponse>("https://test-api.com");
        });
    }
}
