フェーズ1（NewsAPI）の作業を整理した **ToDoリスト** をまとめました ✅

---

# 📝 フェーズ1 ToDoリスト（NewsAPI）

### 1. プロジェクト準備

* [ ] Blazor WebAssembly プロジェクト作成 (`dotnet new blazorwasm`)
* [ ] ソリューション作成＆テストプロジェクト追加（xUnit + bUnit）
* [ ] 必要な NuGet パッケージ導入（`System.Reactive` など）
* [ ] NewsAPI の API キー取得 & `appsettings.json` に保存

---

### 2. 基本的な検索実装（Step 1-1）

* [ ] 検索ボックスのUI作成（MudBlazor利用）
* [ ] 入力イベントを `IObservable<string>` に変換
* [ ] `Throttle`（入力の0.5秒停止で確定）
* [ ] `DistinctUntilChanged`（同じキーワードは無視）
* [ ] `SelectMany`（非同期API呼び出し）

---

### 3. 複数条件の組み合わせ（Step 1-2）

* [ ] カテゴリ選択（チェックボックス or ドロップダウン）
* [ ] キーワード入力とカテゴリを `CombineLatest` で合成
* [ ] `Filter/Where` で条件絞り込み
* [ ] `Switch` で古いリクエストをキャンセル

---

### 4. エラーハンドリングとUX改善（Step 1-3）

* [ ] `Catch` でAPIエラー処理
* [ ] `StartWith` で初期状態を表示
* [ ] `Retry` で失敗時に再試行
* [ ] ローディングスピナー表示（MudProgressCircular）

---

### 5. テスト & リファクタリング

* [ ] xUnit でユニットテスト作成（検索ロジック）
* [ ] bUnit でコンポーネントテスト（UIの動作確認）
* [ ] コードの関数型スタイルへのリファクタリング
* [ ] 学習メモ（つまずき・理解できたこと）を残す

---

👉 このToDoリストを「進行管理ボード（例：GitHub Projects / Trello）」に落とし込むと、進めやすくなります。

次のステップは、**「プロジェクトのセットアップ」から着手**する形でよいですか？
