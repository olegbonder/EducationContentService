FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Docker
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
WORKDIR /src

RUN dotnet nuget add source https://nuget.pkg.github.com/olegbonder/index.json \
    -n github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["FileService.Web/FileService.Web.csproj", "FileService.Web/"]
COPY ["FileService.Contracts/FileService.Contracts.csproj", "FileService.Contracts/"]
COPY ["FileService.Core/FileService.Core.csproj", "FileService.Core/"]
COPY ["FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
COPY ["FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj", "FileService.Infrastructure.Postgres/"]
COPY ["FileService.Infrastructure.S3/FileService.Infrastructure.S3.csproj", "FileService.Infrastructure.S3/"]
COPY ["FileService.VideoProcessing/FileService.VideoProcessing.csproj", "FileService.VideoProcessing/"]
RUN dotnet restore "FileService.Web/FileService.Web.csproj"
COPY . .

RUN dotnet tool install --global dotnet-ef 9.0.0
ENV PATH="root/.dotnet/tools:${PATH}"

WORKDIR "/src/FileService.Web"

RUN dotnet ef migrations bundle \
    --project "../FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj"
    --startup-project "FileService.Web.csproj"
    --output /app/efbundle \
    --configuration $BUILD_CONFIGURATION \
    --verbose \
    -- --configfile /src/nuget.config

RUN dotnet build "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

#Все операции от root
USER root

#Установка Ffmpeg и создание директорий
RUN apt-get update \
    && apt-get install -y ffmpeg \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY --from=build /app/efbundle .

USER $APP_UID

ENTRYPOINT ["dotnet", "FileService.Web.dll"]
