# NewsFlowBB (旧 NewsFlow)

ニュース検索アプリケーション - .NET Blazor WebAssembly + NewsAPI

![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-purple)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![BootstrapBlazor](https://img.shields.io/badge/BootstrapBlazor-9.11-blue)
![PWA](https://img.shields.io/badge/PWA-Ready-green)
![Tests](https://img.shields.io/badge/Tests-xUnit%20%2B%20bUnit-success)

## 📖 概要

NewsFlowBBは、.NET Blazor WebAssemblyとNewsAPIを使用したニュース検索アプリケーションです。.NET Blazor WebAssemblyプログラミングの学習を目的とした教育プロジェクトとして開発されました。

### UIフレームワーク変更履歴

**2025年：MudBlazor → BootstrapBlazor へ移行**

MudBlazorは優れたUIコンポーネントライブラリですが、単体テスト（xUnit + bUnit）の実装が複雑で、チーム開発での保守性に課題があることが判明しました。そのため、より軽量でテストしやすいBootstrapBlazorに移行しました。

**移行の主な理由：**
- 単体テストの実装が容易
- コンポーネントのシンプルな構造
- チーム開発での保守性向上
- Bootstrapベースの親しみやすいUI


### 主な機能

- 🔍 **キーワード検索**: スペース区切りでAND検索に対応
- 📅 **期間指定**: 開始日・終了日で検索範囲を絞り込み
- 🌐 **多言語対応**: 日本語、英語、ドイツ語など7言語に対応
- 📊 **並び替え**: 公開日時、関連度、人気度で並び替え
- 📱 **PWA対応**: オフラインキャッシュとインストール機能
- 🎨 **モダンUI**: BootstrapBlazorによる直感的なBootstrapデザイン
- ✅ **単体テスト**: xUnit + bUnitによるコンポーネントテスト

## 🛠️ 技術スタック

- **フレームワーク**: .NET 8.0 Blazor WebAssembly
- **UIライブラリ**: BootstrapBlazor 9.11
- **API**: NewsAPI (無料プラン)
- **デプロイ**: GitHub Pages + GitHub Actions
- **ストレージ**: Blazored.LocalStorage
- **PWA**: Service Worker + Web App Manifest
- **テスト**: xUnit 2.5 + bUnit 1.40 + Moq 4.20

## 🎯 学習ポイント

このプロジェクトで学べる内容：

1. **Blazor WebAssembly**
   - コンポーネント指向の開発
   - データバインディング
   - ライフサイクルメソッド

2. **PWA開発**
   - Service Workerによるキャッシング
   - オフライン対応
   - インストール可能なWebアプリ

3. **単体テスト**
   - xUnitによるユニットテスト
   - bUnitによるBlazorコンポーネントテスト
   - Moqによるモックオブジェクト作成

4. **CI/CD**
   - GitHub Actionsによる自動デプロイ
   - GitHub Secretsによる機密情報管理
   - GitHub Pagesへの静的サイト配信

## 🚀 クイックスタート

### 前提条件

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [NewsAPI](https://newsapi.org/)の無料アカウント

### ローカル環境でのセットアップ

1. **リポジトリのクローン**
```bash
git clone https://github.com/KajiyaManzou/NewsFlowBB.git
cd NewsFlowBB
```

2. **APIキーの設定**
```bash
cp NewsFlow/wwwroot/appsettings.json.example NewsFlow/wwwroot/appsettings.json
```

`appsettings.json`にNewsAPIキーを設定：
```json
{
  "ApiKeys": {
    "NewsAPIKey": "your-api-key-here"
  }
}
```

3. **アプリケーションの起動**
```bash
dotnet restore NewsFlow/NewsFlow.csproj
dotnet watch run --project NewsFlow/NewsFlow.csproj
```

ブラウザで http://localhost:5000 にアクセス

4. **テストの実行**
```bash
dotnet test NewsFlow.Tests/NewsFlow.Tests.csproj
```

## 📱 PWA機能

このアプリケーションはPWAとして動作します：

- ✅ **オフライン対応**: 一度訪問すればオフラインでも表示可能
- ✅ **インストール可能**: ホーム画面に追加してアプリとして利用
- ✅ **自動更新**: 新バージョンが自動的に検出・更新
- 🔄 **キャッシュ戦略**: 静的リソースを効率的にキャッシュ

## 🚢 デプロイ

### GitHub Pagesへの自動デプロイ

1. **GitHub Pagesの設定**
   - リポジトリの `Settings` → `Pages`
   - Source: `GitHub Actions`

2. **GitHub Secretsの設定**
   - `Settings` → `Secrets and variables` → `Actions`
   - `New repository secret`をクリック
   - Name: `NEWS_API_KEY`
   - Value: あなたのNewsAPIキー

3. **デプロイ**
   ```bash
   git push origin main
   ```

   mainブランチへのプッシュで自動的にビルド・デプロイが実行されます。

### デプロイURL

- 本番環境: https://kajiyamanzou.github.io/NewsFlowBB/

## ⚠️ 制限事項と注意点

### NewsAPI無料プランの制限

NewsAPIの無料プランには以下の制限があります：

| 項目 | 制限内容 |
|------|---------|
| アクセス元 | **localhostのみ** |
| リクエスト数 | 100リクエスト/日 |
| データ取得範囲 | 過去1ヶ月まで |
| 商用利用 | 不可 |

**GitHub Pagesでの動作について**：
- ❌ 本番環境（https://kajiyamanzou.github.io/NewsFlowBB/）では426エラーが発生します
- ✅ ローカル開発環境（http://localhost:5000）では正常に動作します

### 本番環境で動作させる方法

#### 方法1: バックエンドプロキシを使用（推奨）

無料のサーバーレスサービスを経由してNewsAPIを呼び出します：

```
ブラウザ → 自分のバックエンドAPI → NewsAPI
```

**推奨サービス**：
- [Vercel Functions](https://vercel.com/docs/functions)
- [Netlify Functions](https://www.netlify.com/products/functions/)
- [Cloudflare Workers](https://workers.cloudflare.com/)

#### 方法2: 有料プランにアップグレード

- 費用: $449/月〜
- 詳細: https://newsapi.org/pricing

#### 方法3: 別のAPIを使用

無料で本番環境からアクセス可能なAPI：
- [GNews API](https://gnews.io/) - 100リクエスト/日（無料）
- [Currents API](https://currents.api.currentsapi.services/) - 600リクエスト/日（無料）

### セキュリティに関する注意

**Blazor WebAssemblyの制限**：

このアプリケーションはクライアントサイドで実行されるため：
- ✅ APIキーはGitリポジトリに含まれません
- ✅ GitHub Secretsで管理されます
- ❌ ブラウザの開発者ツールでAPIキーが見える可能性があります

**本番環境ではバックエンドAPIの使用を推奨します。**

## 📂 プロジェクト構造

```
NewsFlowBB/
├── NewsFlow/                    # メインプロジェクト
│   ├── Pages/
│   │   ├── News.razor          # ニュース検索ページ
│   │   ├── Home.razor          # ホームページ
│   │   └── ...
│   ├── Layout/
│   │   ├── MainLayout.razor    # メインレイアウト
│   │   └── NavMenu.razor       # ナビゲーションメニュー
│   ├── wwwroot/
│   │   ├── index.html          # エントリーポイント
│   │   ├── service-worker.js   # Service Worker
│   │   ├── manifest.webmanifest # PWA マニフェスト
│   │   └── appsettings.json    # 設定ファイル（Git管理外）
│   ├── Program.cs              # アプリケーションエントリーポイント
│   └── NewsFlow.csproj         # プロジェクトファイル
├── NewsFlow.Tests/              # テストプロジェクト
│   ├── [テストファイル].cs      # xUnit + bUnit テスト
│   └── NewsFlow.Tests.csproj   # テストプロジェクトファイル
├── .github/
│   ├── workflows/
│   │   └── deploy.yml          # GitHub Actions ワークフロー
│   └── scripts/
│       └── update-service-worker-assets.sh
└── README.md
```

## 🤝 コントリビューション

プルリクエストを歓迎します！大きな変更の場合は、まずissueを開いて変更内容を議論してください。

## 📄 ライセンス

このプロジェクトはMITライセンスの下で公開されています。

## 🙏 謝辞

- [NewsAPI](https://newsapi.org/) - ニュースデータ提供
- [BootstrapBlazor](https://www.blazor.zone/) - UIコンポーネントライブラリ
- [.NET Team](https://dotnet.microsoft.com/) - Blazor WebAssemblyフレームワーク
- [bUnit](https://bunit.dev/) - Blazorコンポーネントテストライブラリ

---

**学習プロジェクトとして作成されました** 🎓

このアプリケーションは、Blazor WebAssemblyの学習を目的としています。商用利用を想定していません。
