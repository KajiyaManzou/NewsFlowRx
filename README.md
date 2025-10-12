# NewsFlow

Learn .NET Blazor WebAssembly programming using NewsAPI

## 🚀 デプロイ

このアプリケーションは GitHub Pages に PWA としてデプロイされます。

### デプロイ手順

1. **GitHub Pages の設定**
   - GitHubリポジトリの `Settings` → `Pages` に移動
   - `Source` を `GitHub Actions` に設定

2. **自動デプロイ**
   - `main` ブランチにプッシュすると自動的にデプロイされます
   - GitHub Actions が自動的にビルド・デプロイを実行します

3. **アクセスURL**
   - https://kajiyamanzou.github.io/NewsFlow/

### 手動デプロイ

GitHub の `Actions` タブから `Deploy to GitHub Pages` ワークフローを手動実行できます。

## 🔧 ローカル開発

```bash
# リポジトリをクローン
git clone https://github.com/KajiyaManzou/NewsFlow.git
cd NewsFlow

# 依存関係の復元
dotnet restore NewsFlow/NewsFlow.csproj

# 開発サーバーの起動
dotnet watch run --project NewsFlow/NewsFlow.csproj
```

## 📱 PWA機能

このアプリケーションは Progressive Web App (PWA) として動作します：

- オフライン対応
- インストール可能
- プッシュ通知対応（予定）

## 🔑 NewsAPI キーの設定

NewsAPI を使用するには API キーが必要です：

1. https://newsapi.org/ でアカウント登録
2. API キーを取得
3. `NewsFlow/Pages/News.razor` の `ApiKey` プロパティを設定

```csharp
private string ApiKey => Configuration["ApiKeys:NewsAPIKey"] ?? "YOUR_API_KEY_HERE";
```

または、`appsettings.json` に設定：

```json
{
  "ApiKeys": {
    "NewsAPIKey": "your-api-key-here"
  }
}
```
