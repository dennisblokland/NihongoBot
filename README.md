# NihongoBot
NihongoBot is a Telegram bot written in C# to help users learn Hiragana. You can interact with the bot at https://t.me/DennisNihongoBot.

# Features
**Learn Hiragana characters**: The bot provides interactive lessons to help you master Hiragana.

**Streak tracking**: Keep track of your progress and maintain a streak to stay motivated.

# Prerequisites
* .NET Core 9.0 SDK: Download from https://dotnet.microsoft.com/download.
* A valid Telegram Bot Token: Required for running the bot.

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

## Running with Docker

1. Build the Docker image:
    ```sh
    docker build -t nihongobot .
    ```

2. Run the Docker container:
    ```sh
    docker run -e TelegramBotToken=YOUR_TELEGRAM_BOT_TOKEN nihongobot
    ```

## Usage

Register yourself by sending /start to the bot

## License

This project is licensed under the GNU GPL V3
