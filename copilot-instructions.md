# Copilot Instructions for NihongoBot

## Project Overview

NihongoBot is a Telegram bot written in C# that helps users learn Hiragana characters. The bot provides interactive learning experiences with features like streak tracking, daily questions, and leaderboards. The project implements Clean Architecture principles with clear separation of concerns across multiple layers.

### Key Features
- Hiragana character learning with visual questions
- User streak tracking and gamification
- Daily question scheduling via Hangfire
- Leaderboard functionality
- Configurable user settings (questions per day, word of the day)
- Image generation for Japanese characters using SkiaSharp

## Architecture & Design Patterns

This project follows **Clean Architecture** with the following layers:

### Core Layers
- **NihongoBot.Domain** - Domain entities, aggregates, and business rules
- **NihongoBot.Application** - Application services, handlers, and use cases

### Infrastructure Layers  
- **NihongoBot.Infrastructure** - External service integrations and infrastructure
- **NihongoBot.Persistence** - Data access layer with Entity Framework Core
- **NihongoBot.Server** - Web API server components
- **NihongoBot.Shared** - Shared models and utilities

### Entry Points
- **NihongoBot** - Console application entry point
- **NihongoBot.AppHost** - Application host for running the bot

### Testing
- **Tests/NihongoBot.Application.Tests** - Unit tests for application layer

## Technology Stack

### Core Technologies
- **.NET 9.0** - Primary framework
- **Entity Framework Core 9.0.7** - ORM for data access
- **PostgreSQL** - Primary database (via Npgsql)
- **Telegram.Bot 22.6.0** - Telegram Bot API integration

### Additional Dependencies
- **SkiaSharp 3.119.0** - Image generation for Japanese characters
- **Hangfire 1.8.20** - Background job scheduling
- **System.Drawing.Common** - Graphics support
- **Microsoft.Extensions.Hosting** - Application hosting
- **Newtonsoft.Json** - JSON serialization

## Project Structure Guidelines

### Domain Layer (`NihongoBot.Domain`)
- Contains aggregates: `User`, `Kana`, `Question`
- All domain entities inherit from `DomainEntity` base class
- Business rules and domain logic should be encapsulated in entities
- Repository interfaces are defined here

### Application Layer (`NihongoBot.Application`)
- **Handlers** - Command and callback handlers for Telegram interactions
- **Services** - Application services like `BotService`, `HiraganaService`
- **Models** - DTOs and application-specific models
- **Enums** - Application-level enumerations
- **Workers** - Background services and workers

### Key Patterns
- **Repository Pattern** - Data access abstraction
- **Command/Handler Pattern** - For Telegram bot commands
- **Dispatcher Pattern** - For routing commands and callbacks
- **Dependency Injection** - Used throughout the application

## Coding Conventions

### C# Style Guidelines
- Use **tab indentation** (size 4)
- Private fields prefixed with underscore: `_fieldName`
- Explicit type declarations preferred over `var`
- Braces required for all control structures
- Pascal case for public members, camel case for private fields

### Key Conventions from `.editorconfig`
```csharp
// Private fields
private readonly IUserRepository _userRepository;

// Explicit types preferred
ITelegramBotClient botClient = new TelegramBotClient();

// Method structure
public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Dependency Injection Patterns
- Constructor injection is the primary pattern
- Services are registered in extension methods (e.g., `AddInfrastructureServices()`)
- Use interfaces for all dependencies

## Telegram Bot Development Patterns

### Command Handlers
- Implement `ITelegramCommandHandler` interface
- Handle command parsing and validation
- Always include `CancellationToken` parameters
- Use dependency injection for repositories and services

```csharp
public class ExampleCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserRepository _userRepository;

    public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Callback Handlers
- Implement `ITelegramCallbackHandler` interface
- Handle inline keyboard callbacks
- Parse callback data appropriately

### Bot Service Patterns
- Use `BotService` as the main orchestrator
- Dispatch commands through `CommandDispatcher`
- Dispatch callbacks through `CallbackDispatcher`
- Always handle exceptions and log appropriately

## Database & Entity Framework Patterns

### Entity Configuration
- Use Fluent API in `Configurations` folder
- Follow Entity Framework naming conventions
- Configure relationships explicitly

### Repository Pattern
- Implement repository interfaces in Domain layer
- Concrete implementations in Persistence layer
- Use async methods with `CancellationToken`
- Include `SaveChangesAsync` calls where needed

```csharp
public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.TelegramId == telegramId, cancellationToken);
}
```

## Image Generation Guidelines

### SkiaSharp Usage
- Use for generating Hiragana character images
- Include proper font handling for Japanese characters
- Font files stored in `fonts` directory
- Copy fonts to output directory in project files

### Resource Management
- Properly dispose of SkiaSharp resources
- Use `using` statements for image operations
- Handle font loading and caching appropriately

## Background Jobs & Scheduling

### Hangfire Integration
- Use for scheduling daily questions
- Implement job methods in services
- Configure recurring jobs in startup
- Handle timezone considerations for user scheduling

## Testing Guidelines

### Test Structure
- Unit tests in `Tests/NihongoBot.Application.Tests`
- Test application layer logic
- Mock external dependencies (Telegram API, database)
- Use descriptive test method names

### Testing Patterns
- Arrange-Act-Assert pattern
- Use `CancellationToken.None` for tests
- Mock repositories and external services
- Test both success and failure scenarios

## Configuration Management

### Settings Files
- `appsettings.json` for base configuration
- `appsettings.Development.json` for development overrides
- Environment variables support
- Sensitive data (bot tokens) in development files only

### Required Configuration
```json
{
    "TelegramBotToken": "YOUR_TELEGRAM_BOT_TOKEN"
}
```

## Development Workflow

### Running the Application
1. Set up PostgreSQL database
2. Create `appsettings.Development.json` with bot token
3. Run from `NihongoBot.AppHost` project
4. Database migrations apply automatically on startup

### Adding New Features

#### For New Commands
1. Create handler in `Application/Handlers`
2. Implement `ITelegramCommandHandler`
3. Register in `CommandDispatcher`
4. Add necessary repository methods
5. Write unit tests

#### For New Entities
1. Create entity in `Domain/Aggregates`
2. Add repository interface in `Domain/Interfaces`
3. Implement repository in `Persistence`
4. Add Entity Framework configuration
5. Create and apply migration

## Common Patterns & Best Practices

### Error Handling
- Use structured logging with `ILogger`
- Handle Telegram API exceptions gracefully
- Provide user-friendly error messages
- Log detailed errors for debugging

### Async/Await Usage
- All I/O operations should be async
- Always pass `CancellationToken`
- Use `ConfigureAwait(false)` in library code
- Handle cancellation appropriately

### Resource Management
- Dispose of resources properly
- Use `using` statements for disposables
- Cache expensive operations when appropriate
- Be mindful of memory usage with images

### Security Considerations
- Never log sensitive information (tokens, user data)
- Validate all user inputs
- Use parameterized queries (handled by EF Core)
- Implement proper authorization checks

## Helpful Context for Copilot

When working on this codebase:

1. **Domain Focus**: This is an educational bot for Japanese language learning
2. **User Experience**: Prioritize clear, helpful interactions
3. **Performance**: Consider image generation performance and caching
4. **Localization**: Be mindful of Japanese character handling
5. **Scalability**: Design for multiple concurrent users
6. **Reliability**: Handle network failures and API rate limits gracefully

### Common Use Cases to Suggest
- Adding new Telegram commands
- Implementing new learning features
- Extending user customization options
- Adding analytics and tracking
- Improving error handling and user feedback
- Optimizing image generation performance

### Code Patterns to Favor
- Clean Architecture principles
- SOLID principles
- Repository pattern for data access
- Command/Handler pattern for bot interactions
- Dependency injection throughout
- Async/await for all I/O operations