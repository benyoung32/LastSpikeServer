# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link GameplaySessionTracker/*.csproj GameplaySessionTracker/
RUN dotnet restore GameplaySessionTracker/GameplaySessionTracker.csproj

# Copy source code and publish app
COPY --link GameplaySessionTracker/ GameplaySessionTracker/
WORKDIR /source/GameplaySessionTracker
RUN dotnet publish -a x64 --use-current-runtime --self-contained false -o /app

# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# FINAL STAGE
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Expose port 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GameplaySessionTracker.dll"]
