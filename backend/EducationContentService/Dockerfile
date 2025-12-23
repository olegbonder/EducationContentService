# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9001


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
#RUN dotnet dev-certs https
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["EducationContentService.Web/EducationContentService.Web.csproj", "EducationContentService.Web/"]
COPY ["EducationContentService.Contracts/EducationContentService.Contracts.csproj", "EducationContentService.Contracts/"]
COPY ["EducationContentService.Core/EducationContentService.Core.csproj", "EducationContentService.Core/"]
COPY ["EducationContentService.Domain/EducationContentService.Domain.csproj", "EducationContentService.Domain/"]
COPY ["EducationContentService.Infrastructure.Postgres/EducationContentService.Infrastructure.Postgres.csproj", "EducationContentService.Infrastructure.Postgres/"]
RUN dotnet restore "./EducationContentService.Web/EducationContentService.Web.csproj"
COPY . .
WORKDIR "/src/EducationContentService.Web"
RUN dotnet build "./EducationContentService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EducationContentService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EducationContentService.Web.dll"]