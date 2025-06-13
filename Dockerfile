FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV BUILD_CONFIGURATION=$BUILD_CONFIGURATION

WORKDIR /src

# Copy csproj and restore as distinct layers for better caching
COPY ["PhotoAiBackend.csproj", "./"]
RUN dotnet restore "PhotoAiBackend.csproj"

# Copy everything else
COPY . .

# Build the app
RUN dotnet build "PhotoAiBackend.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoAiBackend.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PhotoAiBackend.dll"]