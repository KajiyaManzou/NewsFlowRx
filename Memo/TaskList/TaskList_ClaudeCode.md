フェーズ1（NewsAPI）の詳細なTODOリストを作成します！

## 📋 フェーズ1：NewsAPI 開発 TODOリスト

### 🔧 セットアップフェーズ（所要時間：1-2時間）

#### ✅ 環境準備
- [x] Blazor WebAssemblyプロジェクトを作成
  ```bash
  dotnet new blazorwasm -o NewsFlow
  cd NewsFlow
  ```
- [ ] NewsAPI アカウント登録 & APIキー取得
  - [ ] https://newsapi.org/ でサインアップ
  - [ ] APIキーをメモ（appsettings.jsonに保存）
- [ ] 必要なNuGetパッケージのインストール
  - [x] `Mudblazor`
  - [ ] `System.Reactive` （Rx.NET）
  - [x] `System.Net.Http.Json`
  - [x] （オプション）`Blazored.LocalStorage` - APIキー保存用

#### ✅ プロジェクト構成
- [ ] フォルダ構造を作成
  ```
  /Models      - データモデル
  /Services    - API通信サービス
  /Pages       - Blazorページ
  /Components  - 再利用可能なコンポーネント
  ```
- [ ] `.gitignore` にAPIキーの設定ファイルを追加
- [ ] `appsettings.json` または `appsettings.Development.json` を作成

---

### 📝 ステップ1-1：基本的な検索機能（1-2日）

#### ✅ モデル作成
- [ ] `NewsArticle.cs` - 記事データモデル
  - [ ] Title（タイトル）
  - [ ] Description（説明）
  - [ ] Url（記事URL）
  - [ ] UrlToImage（画像URL）
  - [ ] PublishedAt（公開日時）
  - [ ] Source（情報源）
- [ ] `NewsApiResponse.cs` - APIレスポンスモデル
  - [ ] Status
  - [ ] TotalResults
  - [ ] Articles（記事リスト）

#### ✅ サービス層実装
- [ ] `INewsApiService.cs` - インターフェース作成
  - [ ] `Task<NewsApiResponse> SearchAsync(string query)`
- [ ] `NewsApiService.cs` - 実装クラス
  - [ ] HttpClientの注入
  - [ ] APIキーの設定
  - [ ] GETリクエストの実装
  - [ ] エラーハンドリング（基本版）
- [ ] `Program.cs` でサービス登録
  ```csharp
  builder.Services.AddScoped<INewsApiService, NewsApiService>();
  ```

#### ✅ リアクティブ検索の実装
- [ ] `SearchComponent.razor` 作成
  - [ ] 検索ボックス（input要素）
  - [ ] 検索結果表示エリア
  - [ ] ローディングインジケーター
- [ ] Rxオペレーターの実装
  - [ ] `Subject<string>` で検索キーワードストリーム作成
  - [ ] `Debounce(TimeSpan.FromMilliseconds(500))` 実装
  - [ ] `DistinctUntilChanged()` 実装
  - [ ] `SelectMany` で非同期API呼び出し
  - [ ] `Subscribe` で結果をUI反映
- [ ] IDisposableの実装（Subscriptionの破棄）

#### ✅ UI実装
- [ ] 検索ボックスのスタイリング
- [ ] 記事カードコンポーネント作成
  - [ ] タイトル表示
  - [ ] 説明文表示（省略処理）
  - [ ] 画像表示（サムネイル）
  - [ ] 公開日時表示
  - [ ] リンクボタン
- [ ] レスポンシブデザイン対応

#### ✅ テスト
- [ ] 検索が正常に動作することを確認
- [ ] Debounceが効いているか確認（連続入力時）
- [ ] 同じキーワードで重複リクエストしないか確認
- [ ] エラー時の挙動確認

---

### 🔄 ステップ1-2：複数条件の組み合わせ（2-3日）

#### ✅ UIコンポーネント追加
- [ ] カテゴリ選択ドロップダウン作成
  - [ ] Business, Entertainment, Health, Science, Sports, Technology
- [ ] 言語選択ドロップダウン（オプション）
  - [ ] 日本語(ja), 英語(en)
- [ ] 日付範囲フィルター（オプション）
  - [ ] From/To の日付ピッカー

#### ✅ モデル拡張
- [ ] `SearchParameters.cs` 作成
  - [ ] Query（キーワード）
  - [ ] Category（カテゴリ）
  - [ ] Language（言語）
  - [ ] FromDate/ToDate（日付範囲）
- [ ] `NewsApiService` にパラメータ追加
  ```csharp
  Task<NewsApiResponse> SearchAsync(SearchParameters parameters)
  ```

#### ✅ リアクティブストリームの合成
- [ ] 各入力項目のSubject作成
  - [ ] `Subject<string>` searchKeyword
  - [ ] `Subject<string>` category
  - [ ] `Subject<string>` language
- [ ] `CombineLatest` で複数ストリームを結合
  ```csharp
  Observable.CombineLatest(
      searchKeyword, 
      category, 
      language,
      (q, c, l) => new SearchParameters(q, c, l)
  )
  ```
- [ ] `Where` でフィルタリング（空文字チェック等）
- [ ] `Switch` で古いリクエストのキャンセル実装

#### ✅ UI更新
- [ ] フィルターパネルのレイアウト調整
- [ ] 検索条件の表示（現在の絞り込み状態）
- [ ] リセットボタン追加

#### ✅ テスト
- [ ] 複数条件での検索が正常動作するか
- [ ] 条件変更時に古いリクエストがキャンセルされるか
- [ ] UIの操作性確認

---

### ⚠️ ステップ1-3：エラーハンドリングとローディング状態（1-2日）

#### ✅ ローディング状態管理
- [ ] `isLoading` 状態変数の追加
- [ ] スピナー/スケルトンUI実装
- [ ] `StartWith` でデフォルト状態設定

#### ✅ エラーハンドリング
- [ ] エラーモデル作成 `ErrorInfo.cs`
  - [ ] Message（エラーメッセージ）
  - [ ] Type（エラー種別）
- [ ] `Catch` オペレーターの実装
  ```csharp
  .Catch<NewsApiResponse, Exception>(ex => 
  {
      // エラーハンドリング
      return Observable.Return(new NewsApiResponse { /* empty */ });
  })
  ```
- [ ] エラー表示UIコンポーネント
  - [ ] エラーメッセージ表示
  - [ ] リトライボタン
- [ ] HTTPステータスコード別のエラーメッセージ
  - [ ] 401: APIキーエラー
  - [ ] 429: レート制限超過
  - [ ] 500: サーバーエラー

#### ✅ リトライロジック
- [ ] `Retry` オペレーターの実装
  ```csharp
  .Retry(3) // 3回までリトライ
  ```
- [ ] 指数バックオフの実装（オプション）
- [ ] リトライ回数の表示

#### ✅ 空状態の処理
- [ ] 検索結果が0件の場合のUI
- [ ] 初期状態（検索前）のUI
- [ ] "結果なし"のイラスト/メッセージ

#### ✅ テスト
- [ ] ネットワークエラー時の挙動
- [ ] APIキーエラー時の挙動
- [ ] レート制限時の挙動
- [ ] ローディング表示の確認
- [ ] リトライ機能の確認

---

### 🎨 仕上げ・リファクタリング（1日）

#### ✅ コード品質向上
- [ ] コードレビュー（自己チェック）
  - [ ] SOLID原則に沿っているか
  - [ ] DRY原則（重複排除）
  - [ ] 関数型プログラミングの原則
- [ ] コメント・ドキュメント追加
- [ ] マジックナンバーの定数化

#### ✅ パフォーマンス最適化
- [ ] 不要な再レンダリングの削減
- [ ] 画像の遅延読み込み
- [ ] メモリリーク確認（Subscription破棄）

#### ✅ ドキュメント作成
- [ ] README.md 作成
  - [ ] プロジェクト概要
  - [ ] セットアップ手順
  - [ ] APIキーの設定方法
  - [ ] 学習したリアクティブパターンの説明
- [ ] 学習メモのまとめ
  - [ ] つまずいた箇所
  - [ ] 理解できたパターン
  - [ ] 次フェーズへの課題

#### ✅ Git管理
- [ ] .gitignore の最終確認
- [ ] コミットメッセージの整理
- [ ] ブランチ戦略の確認
- [ ] GitHubへプッシュ

---

## 📊 進捗管理のヒント

各タスク完了時に：
1. ✅ チェックを入れる
2. 所要時間をメモする
3. 学んだことを簡単にメモする
4. つまずいた箇所を記録する

### 推奨ペース
- **Week 1**: セットアップ + ステップ1-1
- **Week 2**: ステップ1-2 + ステップ1-3
- **Week 3**: 仕上げ・リファクタリング

このTODOリストをベースに、どこから始めますか？具体的なコード実装をサポートします！