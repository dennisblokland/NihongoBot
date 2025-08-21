# NihongoBot Admin Interface

This document describes the admin web interface for NihongoBot, which provides comprehensive management capabilities for both web admin users and Telegram bot users.

## Features

### Authentication & Security
- ASP.NET Core Identity for secure authentication
- Role-based access control (admin-only access)
- Password policies and account lockout protection
- Secure cookie-based session management

### User Management

#### Web Admin Users
- **Full CRUD Operations**: Create, read, update, and delete web admin users
- **Account Management**: Enable/disable user accounts
- **Profile Management**: Update user names and email addresses
- **Password Management**: Set secure passwords with validation

#### Telegram Users
- **View Only**: Display all Telegram bot users with their statistics
- **Delete Only**: Remove Telegram users (all other management via Telegram bot)
- **Search & Filter**: Find users by username or Telegram ID
- **Statistics View**: Monitor user streaks, questions per day, and activity

### Dashboard & Analytics
- **Overview Statistics**: Total users, active users, system status
- **Recent Activity**: Latest user registrations and activity
- **Top Streaks**: Leaderboard of users with highest learning streaks
- **Usage Metrics**: Analytics on user engagement and bot usage

## Default Admin Account

The system automatically creates a default admin account on first run:

- **Username**: `admin`
- **Password**: `Admin123!`
- **Email**: `admin@nihongobot.local`

You can customize these settings in `appsettings.json`:

```json
{
  "AdminSettings": {
    "DefaultAdminEmail": "admin@nihongobot.local",
    "DefaultAdminUsername": "admin",
    "DefaultAdminPassword": "Admin123!",
    "DefaultAdminFirstName": "Admin",
    "DefaultAdminLastName": "User"
  }
}
```

## Access Points

- **Admin Login**: `/admin/login`
- **Dashboard**: `/admin` (redirects to dashboard after login)
- **Web Users**: `/admin/webusers`
- **Telegram Users**: `/admin/telegramusers`
- **Statistics**: `/admin/statistics`

## API Endpoints

The admin interface uses the following REST API endpoints:

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `GET /api/auth/user` - Get current user info

### Web Users Management
- `GET /api/webusers` - List all web users
- `GET /api/webusers/{id}` - Get specific web user
- `POST /api/webusers` - Create new web user
- `PUT /api/webusers/{id}` - Update web user
- `DELETE /api/webusers/{id}` - Delete web user

### Telegram Users Management
- `GET /api/telegramusers` - List all Telegram users
- `GET /api/telegramusers/{id}` - Get specific Telegram user
- `DELETE /api/telegramusers/{id}` - Delete Telegram user
- `GET /api/telegramusers/top-streaks` - Get top 10 users by streak

## Architecture

### Clean Architecture Implementation
- **Domain Layer**: User entities with business logic encapsulation
- **Application Layer**: Services for user management and admin operations
- **Infrastructure Layer**: Repositories and external service integrations
- **Presentation Layer**: Blazor WebAssembly client and API controllers

### Key Components

#### Domain Entities
- `WebUser`: Admin users for web interface access
- `User` (Telegram): Bot users from Telegram integration

#### Services
- `WebUserService`: Business logic for web user management
- `AdminInitializationService`: Default admin user setup
- `UserRepository`: Telegram user data access
- `WebUserRepository`: Web user data access

#### Security Features
- ASP.NET Core Identity integration
- Custom `ApplicationUser` linked to `WebUser` entities
- Authorization middleware protecting all admin endpoints
- Password policies and account lockout protection

## Database Schema

### Identity Tables
Standard ASP.NET Core Identity tables are created for authentication:
- `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.

### Custom Tables
- `WebUsers`: Admin user profiles and settings
- `Users`: Telegram bot users (existing table)

### Relationships
- `ApplicationUser.WebUserId` → `WebUsers.Id` (1:1 relationship)

## Development Notes

### Requirements Fulfilled
✅ Separate handling for Telegram vs Web Interface users  
✅ Web users: full CRUD operations via admin interface  
✅ Telegram users: view and delete only (management via Telegram)  
✅ ASP.NET Core Identity for authentication and authorization  
✅ Admin-only access with configurable default login  
✅ Usage statistics and system monitoring dashboard  
✅ Responsive Blazor WebAssembly interface  

### Technology Stack
- **Frontend**: Blazor WebAssembly with Bootstrap 5
- **Backend**: ASP.NET Core Web API
- **Authentication**: ASP.NET Core Identity
- **Database**: PostgreSQL with Entity Framework Core
- **Styling**: Bootstrap 5 + Font Awesome icons

### Security Considerations
- All admin endpoints require authentication
- Password complexity requirements enforced
- Account lockout after failed attempts
- Secure cookie settings for session management
- HTTPS recommended for production deployment