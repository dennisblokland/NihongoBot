# Use a .NET 8 base image for the container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Copy the project files and build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NihongoBot/NihongoBot.csproj", "NihongoBot/"]
RUN dotnet restore "NihongoBot/NihongoBot.csproj"
COPY . .
WORKDIR "/src/NihongoBot"
RUN dotnet build "NihongoBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NihongoBot.csproj" -c Release -o /app/publish

# Set the environment variable for the Telegram API key
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TelegramBotToken=YOUR_TELEGRAM_BOT_TOKEN

# Specify the entry point for the application
ENTRYPOINT ["dotnet", "NihongoBot.dll"]
