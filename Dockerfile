FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Kika.WhatsAppAutomation.Api.csproj ./
RUN dotnet restore Kika.WhatsAppAutomation.Api.csproj

COPY Program.cs appsettings.json ./
COPY Controllers/ ./Controllers/
COPY Options/ ./Options/
RUN dotnet publish Kika.WhatsAppAutomation.Api.csproj \
    --configuration Release --no-restore --output /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /out/ ./

ENV ASPNETCORE_HTTP_PORTS=5090 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 5090
USER $APP_UID
ENTRYPOINT ["dotnet", "Kika.WhatsAppAutomation.Api.dll"]
