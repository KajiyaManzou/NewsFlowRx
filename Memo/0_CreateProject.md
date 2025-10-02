# プロジェクト作成

## 1. .NET Blazor WebAssemby NewsFlowプロジェクト作成

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet new blazorwasm -n NewsFlow --pwa
The template "Blazor WebAssembly Standalone App" was created successfully.
This template contains technologies from parties other than Microsoft, see https://aka.ms/aspnetcore/8.0-third-party-notices for details.

Processing post-creation actions...
Restoring /workspace/NewsFlow/NewsFlow.csproj:
  Determining projects to restore...
  Restored /workspace/NewsFlow/NewsFlow.csproj (in 5.57 sec).
Restore succeeded.
```

## 2. xUnit NewsFlow.Testsプロジェクト作成

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet new xunit -n NewsFlow.Tests
The template "xUnit Test Project" was created successfully.

Processing post-creation actions...
Restoring /workspace/NewsFlow.Tests/NewsFlow.Tests.csproj:
  Determining projects to restore...
  Restored /workspace/NewsFlow.Tests/NewsFlow.Tests.csproj (in 151 ms).
Restore succeeded.


devuser@7a0a3ae9d5e6:/workspace$ 
```

## 3. ソリューションファイル作成、プロジェクト追加

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet new sln -n NewsFlow
The template "Solution File" was created successfully.

devuser@7a0a3ae9d5e6:/workspace$ dotnet sln add NewsFlow/NewsFlow.csproj 
Project `NewsFlow/NewsFlow.csproj` added to the solution.
devuser@7a0a3ae9d5e6:/workspace$ dotnet sln add NewsFlow.Tests/NewsFlow.Tests.csproj 
Project `NewsFlow.Tests/NewsFlow.Tests.csproj` added to the solution.
devuser@7a0a3ae9d5e6:/workspace$ 
```

## 4. xUnit プロジェクト bUnit パッケージ追加

```bash
devuser@7a0a3ae9d5e6:/workspace$ cd NewsFlow.Tests/
devuser@7a0a3ae9d5e6:/workspace/NewsFlow.Tests$ dotnet add package bunit
  Determining projects to restore...
  Writing /tmp/tmpd9AU9Z.tmp
~~ 省略
devuser@7a0a3ae9d5e6:/workspace/NewsFlow.Tests$ 
```

## 5. サンプルプログラムビルド

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet build  NewsFlow/NewsFlow.csproj 
  Determining projects to restore...
  All projects are up-to-date for restore.
  NewsFlow -> /workspace/NewsFlow/bin/Debug/net8.0/NewsFlow.dll
  NewsFlow (Blazor output) -> /workspace/NewsFlow/bin/Debug/net8.0/wwwroot

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.08
devuser@7a0a3ae9d5e6:/workspace$ 
```

## 6. サンプルプログラム実行

ブラウザで`http://localhost:5065`にアクセスし、サンプルプログラム起動確認

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet watch --project NewsFlow/NewsFlow.csproj 
dotnet watch ⌚ Polling file watcher is enabled
dotnet watch 🔥 Hot reload enabled. For a list of supported edits, see https://aka.ms/dotnet/hot-reload.
  💡 Press "Ctrl + R" to restart.
dotnet watch 🔧 Building...
  Determining projects to restore...
  All projects are up-to-date for restore.
  NewsFlow -> /workspace/NewsFlow/bin/Debug/net8.0/NewsFlow.dll
  NewsFlow (Blazor output) -> /workspace/NewsFlow/bin/Debug/net8.0/wwwroot
dotnet watch 🚀 Started
warn: Microsoft.AspNetCore.Hosting.Diagnostics[15]
      Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://localhost:5065'.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5065
dotnet watch 🌐 Unable to launch the browser. Navigate to http://localhost:5065
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /workspace/NewsFlow
dotnet watch ⌚ Connecting to the browser is taking longer than expected ...
dotnet watch ⌚ Connecting to the browser is taking longer than expected ...
dotnet watch 🛑 Shutdown requested. Press Ctrl+C again to force exit.
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...

devuser@7a0a3ae9d5e6:/workspace$ 
```

## 7. サンプルテスト　ビルド、実行

```bash
devuser@7a0a3ae9d5e6:/workspace$ dotnet build  NewsFlow.Tests/NewsFlow.Tests.csproj 
  Determining projects to restore...
  All projects are up-to-date for restore.
  NewsFlow.Tests -> /workspace/NewsFlow.Tests/bin/Debug/net8.0/NewsFlow.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.88
devuser@7a0a3ae9d5e6:/workspace$ dotnet test NewsFlow.Tests/NewsFlow.Tests.csproj 
  Determining projects to restore...
  All projects are up-to-date for restore.
  NewsFlow.Tests -> /workspace/NewsFlow.Tests/bin/Debug/net8.0/NewsFlow.Tests.dll
Test run for /workspace/NewsFlow.Tests/bin/Debug/net8.0/NewsFlow.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (arm64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: < 1 ms - NewsFlow.Tests.dll (net8.0)
devuser@7a0a3ae9d5e6:/workspace$ 
```