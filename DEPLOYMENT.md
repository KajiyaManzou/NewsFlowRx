# NewsFlowRx - GitHub Pagesデプロイメントガイド

## 📋 概要

このドキュメントでは、NewsFlowRxをGitHub Pagesにデプロイする手順を説明します。

---

## 🚀 デプロイ手順

### 1. GitHub Pagesの有効化

1. GitHubリポジトリのページにアクセス
2. **Settings** タブをクリック
3. 左サイドバーから **Pages** を選択
4. **Source** で **GitHub Actions** を選択
5. 設定を保存

### 2. GitHub Secretsの設定

NewsAPIキーを安全に保管するため、GitHub Secretsを設定します。

1. リポジトリの **Settings** → **Secrets and variables** → **Actions**
2. **New repository secret** をクリック
3. 以下の設定を追加：

| Name | Value |
|:---|:---|
| `NEWS_API_KEY` | あなたのNewsAPIキー（[NewsAPI](https://newsapi.org/)から取得） |

**APIキーの取得方法:**
1. https://newsapi.org/ にアクセス
2. **Get API Key** をクリック
3. 無料アカウントを作成
4. ダッシュボードからAPIキーをコピー

### 3. デプロイの実行

以下のいずれかの方法でデプロイを実行します：

#### 方法A: mainブランチへのプッシュ（自動）

```bash
git add .
git commit -m "Deploy to GitHub Pages"
git push origin main
```

mainブランチへのプッシュで自動的にGitHub Actionsが起動します。

#### 方法B: 手動実行

1. GitHubリポジトリの **Actions** タブにアクセス
2. 左サイドバーから **Deploy NewsFlowRx to GitHub Pages** を選択
3. **Run workflow** ボタンをクリック
4. ブランチ（main）を選択
5. **Run workflow** をクリック

### 4. デプロイ状況の確認

1. **Actions** タブでワークフローの実行状況を確認
2. ✅ 緑色のチェックマークが表示されればデプロイ成功
3. ❌ 赤色のバツマークが表示された場合は、ログを確認してエラーを修正

### 5. デプロイされたサイトへのアクセス

デプロイ完了後、以下のURLでアクセスできます：

```
https://<ユーザー名>.github.io/NewsFlowRx/
```

例: https://kajiyamanzou.github.io/NewsFlowRx/

---

## 🔧 GitHub Actionsワークフローの詳細

### ワークフローファイル

`.github/workflows/deploy.yml`

### 主要なステップ

1. **Checkout**: ソースコードをチェックアウト
2. **Setup .NET**: .NET 8.0 SDKをセットアップ
3. **Restore**: NuGetパッケージを復元
4. **Build**: プロジェクトをビルド（Release構成）
5. **Publish**: アプリケーションを発行
6. **Create appsettings.json**: APIキーを含む設定ファイルを生成
7. **Set base path**: GitHub Pages用のベースパスを設定
8. **Update service-worker**: Service Workerのパスを更新
9. **Upload artifact**: ビルド成果物をアップロード
10. **Deploy**: GitHub Pagesにデプロイ

### 環境変数

| 変数名 | 説明 | 設定場所 |
|:---|:---|:---|
| `NEWS_API_KEY` | NewsAPIキー | GitHub Secrets |
| `BASE_PATH` | GitHub Pagesのベースパス | ワークフロー内で自動計算 |

---

## ⚠️ 重要な注意事項

### NewsAPI無料プランの制限

NewsAPIの無料プランには以下の制限があります：

| 項目 | 制限内容 |
|:---|:---|
| アクセス元 | **localhostのみ** |
| リクエスト数 | 100リクエスト/日 |
| データ取得範囲 | 過去1ヶ月まで |

### GitHub Pagesでの動作について

**⚠️ 重要:**
NewsAPIの無料プランはlocalhostからのリクエストのみを許可しているため、GitHub Pagesからのリクエストは **426エラー** が発生します。

```
❌ https://kajiyamanzou.github.io/NewsFlowRx/ → 426エラー
✅ http://localhost:5000 → 正常動作
```

### 本番環境で動作させる方法

#### 方法1: バックエンドプロキシを使用（推奨）

無料のサーバーレスサービスを経由してNewsAPIを呼び出します：

**推奨サービス:**
- [Vercel Functions](https://vercel.com/docs/functions)
- [Netlify Functions](https://www.netlify.com/products/functions/)
- [Cloudflare Workers](https://workers.cloudflare.com/)

**アーキテクチャ:**
```
ブラウザ → Vercel Function → NewsAPI
         (プロキシ)
```

#### 方法2: NewsAPI有料プランにアップグレード

- 費用: $449/月〜
- 詳細: https://newsapi.org/pricing

#### 方法3: 別のAPIを使用

無料で本番環境からアクセス可能なAPI：
- [GNews API](https://gnews.io/) - 100リクエスト/日（無料）
- [Currents API](https://currents.api.currentsapi.services/) - 600リクエスト/日（無料）

---

## 🔍 トラブルシューティング

### デプロイが失敗する

#### 問題1: NEWS_API_KEYが設定されていない

**エラー:**
```
Error: NEWS_API_KEY secret not found
```

**解決策:**
GitHub Secretsに`NEWS_API_KEY`を設定してください。

#### 問題2: ビルドエラー

**エラー:**
```
Build FAILED
```

**解決策:**
1. ローカルで`dotnet build`が成功することを確認
2. .NET 8.0 SDKがインストールされていることを確認
3. すべての依存関係が正しく設定されていることを確認

#### 問題3: ページが表示されない

**症状:**
デプロイは成功したが、ページが404エラーで表示されない

**解決策:**
1. GitHub Pages設定でSourceが「GitHub Actions」になっていることを確認
2. デプロイ後、数分待ってから再度アクセス
3. ブラウザのキャッシュをクリア

### 426エラーが表示される

**症状:**
```
Upgrade Required (426)
```

**原因:**
NewsAPIの無料プランはlocalhostからのみアクセス可能

**解決策:**
上記「本番環境で動作させる方法」を参照してください。

---

## 📊 デプロイ後の確認事項

### チェックリスト

- [ ] GitHub Pagesが有効化されている
- [ ] `NEWS_API_KEY`がGitHub Secretsに設定されている
- [ ] ワークフローが正常に完了している（✅緑色）
- [ ] デプロイされたURLにアクセスできる
- [ ] UIが正しく表示される
- [ ] Rx.NET機能（自動検索）が動作する
- [ ] クリアボタンが正しく配置されている
- [ ] PWA機能が動作する（Service Worker）

### 動作確認手順

1. **UI表示確認**
   - ページタイトル「ニュース検索Rx」が表示される
   - 検索条件カードが表示される
   - クリアボタンが検索条件の左に配置されている

2. **Rx.NET機能確認**
   - キーワードを入力してから0.5秒後に自動検索される
   - 言語を変更すると即座に検索される
   - 日付や並び替えを変更すると自動検索される

3. **PWA機能確認**
   - ブラウザのアドレスバーにインストールアイコンが表示される
   - オフラインでもページが表示される（キャッシュ）

---

## 🔄 継続的デプロイメント

### 自動デプロイのトリガー

以下の場合に自動的にデプロイが実行されます：

1. **mainブランチへのプッシュ**
   ```bash
   git push origin main
   ```

2. **プルリクエストのマージ**
   mainブランチへのマージで自動実行

3. **手動実行**
   GitHub ActionsのUIから手動でトリガー

### デプロイフロー

```
コード変更
   ↓
git commit & push
   ↓
GitHub Actions起動
   ↓
ビルド & テスト
   ↓
成果物生成
   ↓
GitHub Pagesにデプロイ
   ↓
完了（数分後にサイト更新）
```

---

## 📝 その他の情報

### リポジトリ構成

```
NewsFlowRx/
├── .github/
│   ├── workflows/
│   │   └── deploy.yml          # デプロイワークフロー
│   └── scripts/
│       └── update-service-worker-assets.sh
├── NewsFlowRx/                 # メインプロジェクト
│   ├── NewsFlowRx.csproj
│   ├── Pages/
│   │   └── News.razor          # Rx.NET統合
│   └── wwwroot/
│       ├── index.html
│       ├── service-worker.js   # PWA
│       └── appsettings.json    # デプロイ時に生成
└── NewsFlowRx.Tests/           # テストプロジェクト
    ├── NewsFlowRxTests.cs
    ├── NewsFlowRxUITests.cs
    └── NewsFlowRxInteractionTests.cs
```

### 関連ドキュメント

- [README.md](README.md) - プロジェクト概要
- [NewsFlowRx.Tests/README.md](NewsFlowRx.Tests/README.md) - テストドキュメント
- [GitHub Actions公式ドキュメント](https://docs.github.com/actions)
- [GitHub Pages公式ドキュメント](https://docs.github.com/pages)

---

**最終更新**: 2025-10-13
