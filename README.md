🇺🇸 English | [🇧🇷 Português](README.pt-BR.md)

# Electronic Time Clock System (Ponto Eletrônico)

Full-stack time-tracking application built with **Angular 15** and **.NET 6**, built as a technical challenge. Employees can clock in and out and review their history through a secure, simple interface.

> **Note:** the user interface is in Brazilian Portuguese.

## Screenshots

| Login | Dashboard and history |
| --- | --- |
| ![Login screen](docs/screenshots/login.png) | ![Dashboard with daily status and time records](docs/screenshots/dashboard.png) |

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Data Model](#data-model)
- [Security](#security)
- [Tech Stack](#tech-stack)
- [Production Setup](#production-setup)
- [Possible Improvements](#possible-improvements)
- [License](#license)

## Features

**Authentication and security**
- JWT-based login and user registration
- Session control with automatic token renewal
- Secure logout
- Route protection with Angular guards

**Time tracking**
- Clock in and clock out with automatic date and time
- Optional description for each record (the data model also supports break start/end types)

**Viewing and querying**
- Dashboard with the real-time status of the day and a live digital clock
- Full history of time records
- Date range filters, pagination and column sorting (date/time, type, description)

## Architecture

**Backend (.NET 6)**
- REST API with ASP.NET Core 6
- Authentication with ASP.NET Identity and JWT Bearer
- SQL Server with Entity Framework Core (code-first migrations)
- Input validation with FluentValidation
- Interactive API documentation with Swagger/OpenAPI

**Frontend (Angular 15)**
- Angular 15.2 with Angular Material
- JWT handling through HTTP interceptors
- Reactive forms with validation
- Responsive layout for desktop and mobile

## Project Structure

```
PontoEletronico/
├── PontoEletronico.Api/          # Backend (.NET 6)
│   ├── Controllers/              # API endpoints
│   ├── Services/                 # Business logic
│   ├── Models/                   # Domain models
│   ├── DTOs/                     # Data transfer objects
│   ├── Data/                     # Database context
│   ├── Validators/               # FluentValidation rules
│   ├── Middleware/               # Custom middleware
│   └── Migrations/               # EF Core migrations
├── PontoEletronico.App/          # Frontend (Angular)
│   ├── src/app/components/       # UI components
│   ├── src/app/services/         # Angular services
│   ├── src/app/models/           # TypeScript interfaces
│   ├── src/app/interceptors/     # HTTP interceptors
│   ├── src/app/guards/           # Route guards
│   └── src/assets/               # Static assets
└── README.md
```

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Node.js 16+](https://nodejs.org/)
- [Angular CLI 15+](https://angular.io/cli)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server

You need **two terminal windows**, one for the backend and one for the frontend. Both must be running at the same time.

### 1. Clone the repository

```bash
git clone https://github.com/CodeDLucas/PontoEletronico.git
cd PontoEletronico
```

### 2. Backend

Edit `PontoEletronico.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PontoEletronicoDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "PontoEletronicoAPI",
    "Audience": "PontoEletronicoApp",
    "ExpirationMinutes": 60
  }
}
```

Create the database and run the API:

```bash
cd PontoEletronico.Api
dotnet ef database update
dotnet run
```

The API runs at `https://localhost:7059` and the Swagger UI is available at `https://localhost:7059/swagger`.

### 3. Frontend

```bash
cd PontoEletronico.App
npm install
ng serve
```

Make sure `src/environments/environment.ts` points to the API:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7059'
};
```

The app runs at `http://localhost:4200`.

## API Endpoints

**Authentication**
- `POST /api/auth/login` - Log in
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/logout` - Log out
- `POST /api/auth/refresh` - Renew the token
- `GET /api/auth/verify` - Verify the token

**Time records**
- `POST /api/timerecord` - Create a record
- `GET /api/timerecord` - List records (with filters)
- `GET /api/timerecord/{id}` - Get a record
- `GET /api/timerecord/today` - Records of the current day
- `GET /api/timerecord/summary` - Summary grouped by day
- `DELETE /api/timerecord/{id}` - Delete a record

**Users**
- `GET /api/user/profile` - Get the profile
- `PUT /api/user/profile` - Update the profile
- `POST /api/user/change-password` - Change the password

## Data Model

**ApplicationUser**
- `Id` (string), `FullName` (string), `Email` (string, unique)
- `EmployeeCode` (string, optional)
- `CreatedAt` (DateTime), `IsActive` (bool)

**TimeRecord**
- `Id` (int), `UserId` (string, FK to ApplicationUser)
- `Timestamp` (DateTime)
- `Type` (enum: ClockIn / ClockOut / BreakStart / BreakEnd)
- `Description` (string, optional), `CreatedAt` (DateTime)

## Security

- **HTTPS** redirection enforced
- **JWT** tokens with configurable expiration
- **CORS** restricted to allowed origins
- **Validation** on both client and server
- **SQL injection** prevention through parameterized queries (Entity Framework)
- **Authorization** by authenticated user
- **Password policy:** at least 6 characters, with lowercase, uppercase and a digit; account lockout after 5 failed attempts

## Tech Stack

**Backend:** .NET 6, ASP.NET Core Web API, Entity Framework Core 6.0.33, ASP.NET Identity, FluentValidation 11.3.0, JWT Bearer Authentication, SQL Server, Swagger/OpenAPI

**Frontend:** Angular 15.2.0, Angular Material 15.2.9, TypeScript 4.9.4, RxJS 7.8.0, SCSS

## Production Setup

**Backend**
1. Create `appsettings.Production.json` with the production connection string, a unique and secure JWT key, and logging settings.
2. Configure CORS for the production domain.

**Frontend**
1. Set the production API URL in `environment.prod.ts`.
2. Build with `ng build --configuration=production` and serve the files in `dist/`.

## Possible Improvements

- Automated tests (unit and integration)
- CI pipeline
- Upgrade to current LTS versions of .NET and Angular
- Containerization with Docker

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.
