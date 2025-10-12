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

### ローカル開発環境

1. https://newsapi.org/ でアカウント登録してAPIキーを取得
2. `NewsFlow/wwwroot/appsettings.json.example` をコピーして `appsettings.json` を作成：

```bash
cp NewsFlow/wwwroot/appsettings.json.example NewsFlow/wwwroot/appsettings.json
```

3. `appsettings.json` にAPIキーを設定：

```json
{
  "ApiKeys": {
    "NewsAPIKey": "your-api-key-here"
  }
}
```

⚠️ **注意**: `appsettings.json` は `.gitignore` に含まれているため、Gitにコミットされません。

### GitHub Pages デプロイ用

1. GitHubリポジトリの `Settings` → `Secrets and variables` → `Actions` に移動
2. `New repository secret` をクリック
3. 以下のシークレットを追加：
   - **Name**: `NEWS_API_KEY`
   - **Value**: あなたのNewsAPI キー

GitHub Actions が自動的にこのシークレットを使用してデプロイ時に `appsettings.json` を生成します。

## ⚠️ セキュリティに関する重要な注意事項

**Blazor WebAssemblyの制限**：

このアプリケーションはクライアントサイド（ブラウザ）で実行されるため、以下の点に注意してください：

- ✅ APIキーはGitリポジトリには含まれません
- ✅ GitHub Secretsで安全に管理されます
- ❌ **しかし、デプロイ後はブラウザの開発者ツールでAPIキーが見える可能性があります**

これはBlazor WebAssemblyの仕様上の制限です。完全にAPIキーを隠蔽するには、バックエンドAPIを経由する必要があります。

**学習目的での使用**：
- NewsAPIの無料プランは1日100リクエストまでの制限があります
- 学習目的であれば、この方法でも問題ありません
- 本番環境では必ずバックエンドAPIを使用してください

## ⚠️ NewsAPI の重要な制限事項

**NewsAPI 無料プランの制限**：

NewsAPIの無料プランは**localhost（開発環境）からのアクセスのみ**許可されています。

- ✅ ローカル開発環境: 動作します
- ❌ GitHub Pages等の本番環境: **426 Upgrade Required エラー**が発生します

### 本番環境で動作させる方法

1. **有料プランにアップグレード**（$449/月〜）
   - https://newsapi.org/pricing

2. **バックエンドプロキシを使用**（推奨）
   - Vercel Functions, Netlify Functions, Cloudflare Workers等の無料サービスを経由
   - バックエンドからNewsAPIを呼び出し、Blazorアプリからバックエンドを呼び出す

3. **別のニュースAPIを使用**
   - GNews API (https://gnews.io/) - 無料プランあり
   - Currents API (https://currents.api.currentsapi.services/) - 無料プランあり

### デモについて

このリポジトリのGitHub Pagesデモは、NewsAPIの制限により**ローカル開発環境でのみ動作確認可能**です。
