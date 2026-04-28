FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
#RUN dotnet dev-certs https
WORKDIR /src

COPY ["nuget.config", "."]
COPY [".editorconfig", "."]

RUN dotnet nuget update source github \
    --username $NUGET_USERNAME \
    --password $NUGET_PASSWORD \
    --store-password-in-clear-text \
    --configfile nuget.config

COPY ["EducationContentService/", "EducationContentService/"]
RUN dotnet restore "EducationContentService/EducationContentService.Web/EducationContentService.Web.csproj"

WORKDIR "/src/EducationContentService/EducationContentService.Web"

RUN dotnet publish "EducationContentService.Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5238
ENTRYPOINT ["dotnet", "EducationContentService.Web.dll"]