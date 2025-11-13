# NihongoBot

NihongoBot is a Telegram bot written in C# to help users learn Hiragana.

## Features

- Learn Hiragana characters
- streak tracking

## Prerequisites

- [.NET Core 10.0 SDK](https://dotnet.microsoft.com/download)
- A valid Telegram Bot Token

## Running Locally

1. Clone the repository:
    ```sh
    git clone https://github.com/dennisblokland/NihongoBot.git
    cd NihongoBot
    ```

2. Restore dependencies for all projects:
    ```sh
    dotnet restore
    ```

3. Create an `appsettings.Development.json` file in the `NihongoBot.Server` and  `NihongoBot` directory with the following content:
    ```json
    {
        "TelegramBotToken": "YOUR_TELEGRAM_BOT_TOKEN"
    }
    ```

4. Navigate to the `NihongoBot.AppHost` directory:
    ```sh
    cd NihongoBot.AppHost
    ```

5. Set up client assets (for admin interface):
    ```sh
    cd NihongoBot.Client
    dotnet tool install -g Microsoft.Web.LibraryManager.Cli
    libman restore
    cd ..
    ```

6. Run the application:
    ```sh
    dotnet run
    ```

## Migrations

To apply database migrations, follow these steps:

1. Navigate to the `NihongoBot.Persistence` directory:
    ```sh
    cd NihongoBot.Persistence
    ```

2. Add a new migration (if needed):
    ```sh
    dotnet ef migrations add <MigrationName>
    ```

3. Apply the migrations By starting the app.

4. To remove migrations run the following command:
    ```sh
    dotnet ef migrations remove --force
    ```

## Usage

Register yourself by sending /start to the bot

## Contributing

We welcome contributions to NihongoBot! Please read our [Contributing Guidelines](CONTRIBUTING.md) for information about:
- Development setup and workflow
- Code style guidelines  
- Testing requirements
- Deployment approval process

## License

This project is licensed under the GNU GPL V3
