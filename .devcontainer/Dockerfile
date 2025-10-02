# .NET 8 SDK を使用
FROM mcr.microsoft.com/dotnet/sdk:8.0

# 作業ディレクトリを設定
WORKDIR /workspace

# 必要なツールをインストール
RUN apt-get update && apt-get install -y \
    git \
    curl \
    wget \
    unzip \
    vim \
    nano \
    tree \
    && rm -rf /var/lib/apt/lists/*

# Node.js をインストール (Blazor WebAssembly開発に必要な場合があります)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs  

# エラー　スキップ
# PowerShell をインストール (オプション) 
#RUN curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
#    && echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-bullseye-prod bullseye main" > /etc/apt/sources.list.d/microsoft.list \
#    && apt-get update \
#    && apt-get install -y powershell \
#    && rm -rf /var/lib/apt/lists/*

# 開発ユーザーの設定
RUN useradd -m -s /bin/bash devuser \
    && usermod -aG sudo devuser

# エラー　スキップ
# .NET テンプレートをインストール
#RUN dotnet new install Microsoft.AspNetCore.Components.WebAssembly.ProjectTemplates

# 作業ディレクトリの権限設定
RUN chown -R devuser:devuser /workspace

# デフォルトユーザーを設定
USER devuser

# .NET CLI の初期設定
RUN dotnet --version

# gitignore
RUN dotnet new gitignore

EXPOSE 5000 5001