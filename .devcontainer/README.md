# NewsFlow  DevContainer環境

このフォルダには、NewsFlow プロジェクト用のDevContainer設定が含まれています。

## 環境概要

- **ベースイメージ**: Microsoft .NET 8 SDK
- **開発言語**: C# (.NET 8)
- **フレームワーク**: Blazor WebAssembly
- **追加ツール**: Node.js, PowerShell, Git

## セットアップ手順

### 1. 必要な前提条件
- Docker Desktop がインストールされていること
- Visual Studio Code がインストールされていること
- VS Code の Dev Containers 拡張機能がインストールされていること

### 2. DevContainer の起動方法

1. VS Code でこのプロジェクトフォルダを開く
2. コマンドパレット（Cmd/Ctrl + Shift + P）を開く
3. "Dev Containers: Reopen in Container" を選択
4. 初回はDockerイメージのビルドに時間がかかります（数分程度）

### 3. 環境確認

コンテナが起動したら、以下のコマンドで環境を確認できます：

```bash
# .NET のバージョン確認
dotnet --version

# Node.js のバージョン確認
node --version

# Blazor WebAssembly テンプレートが利用可能か確認
dotnet new blazorwasm --help
```

## 含まれる VS Code 拡張機能

- C# Dev Kit
- Blazor WebAssembly Companion
- PowerShell
- Path Intellisense
- Auto Rename Tag

## ポート設定

- **5000**: HTTP (Blazor開発サーバー)
- **5001**: HTTPS (Blazor開発サーバー)
- **3000**: 開発用サーバー（必要に応じて）

## 環境変数

- `ASPNETCORE_ENVIRONMENT=Development`
- `DOTNET_USE_POLLING_FILE_WATCHER=true`

## トラブルシューティング

### コンテナが起動しない場合
1. Docker Desktop が起動していることを確認
2. Docker Desktop のリソース設定を確認（メモリ4GB以上推奨）

### 拡張機能が動作しない場合
1. コンテナ内でVS Codeを再起動
2. コマンドパレットから "Developer: Reload Window" を実行

## 参考リンク# ...existing code...

## MudBlazor UI フレームワークの追加

このプロジェクトには MudBlazor が導入されています。  
Blazor プロジェクトで MudBlazor を利用するには、以下の手順が自動で実行されます。

1. DevContainer のビルド時に `dotnet add package MudBlazor` が実行されます。
2. 必要に応じて `_Imports.razor` などで `@using MudBlazor` を追加してください。
3. `MainLayout.razor` などで MudBlazor のコンポーネントを利用できます。

公式ドキュメント: [MudBlazor](https://mudblazor.com/)

# ...existing code...

- [Dev Containers documentation](https://code.visualstudio.com/docs/devcontainers/containers)
- [.NET 8 documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Blazor WebAssembly documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)