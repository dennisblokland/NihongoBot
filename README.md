# NihongoBot

NihongoBot is a Telegram bot written in C# to help users learn Hiragana.

## Features

- Learn Hiragana characters
- streak tracking

## Prerequisites

- [.NET Core 8.0 SDK](https://dotnet.microsoft.com/download)
- A valid Telegram Bot Token

## Running Locally

1. Clone the repository:
    ```sh
    git clone https://github.com/dennisblokland/NihongoBot.git
    cd NihongoBot
    ```

2. Restore dependencies:
    ```sh
    dotnet restore
    ```

3. Create an `appsettings.Development.json` file in the root directory with the following content:
    ```json
    {
        "TelegramBotToken": "YOUR_TELEGRAM_BOT_TOKEN"
    }
    ```

4. Run the application:
    ```sh
    dotnet run
    ```

## Usage

Register yourself by sending /start to the bot

## License

This project is licensed under the GNU GPL V3
