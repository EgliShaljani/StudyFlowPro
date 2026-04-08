# StudyFlowPro

StudyFlowPro is a portfolio-ready ASP.NET Core web application built for students who want a cleaner way to manage subjects, assignments, deadlines, and study momentum. The project is designed to feel like a real product rather than a classroom demo, with authentication, role-based access, dashboard analytics, filtering, seeded demo data, dark mode, and SignalR-powered deadline notifications.

## Project Overview

StudyFlowPro helps users:

- register and sign in with ASP.NET Identity
- manage subjects and study tasks in a structured workflow
- track progress through a modern dashboard
- filter work by subject, priority, status, and due date
- receive real-time deadline alerts for upcoming and overdue work
- explore admin and student role behavior with seeded demo accounts

## Key Features

- ASP.NET Core MVC application targeting `.NET 9`
- Entity Framework Core with SQLite
- ASP.NET Identity authentication and role-based authorization
- seeded `Admin` and `Student` demo users
- dashboard with total, completed, pending, and overdue task stats
- full CRUD flows for subjects and study tasks
- subject-linked task organization with priority and status badges
- search and filter experience for real task management workflows
- SignalR hub plus background refresh service for deadline notifications
- dark mode toggle with local preference persistence
- xUnit tests for domain logic and view model validation
- GitHub Actions workflow for build and test

## Technologies Used

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- ASP.NET Identity
- SignalR
- xUnit
- GitHub Actions

## Demo Accounts

The app seeds sample users on first run:

- Student: `student@studyflowpro.dev` / `StudyFlow123!`
- Admin: `admin@studyflowpro.dev` / `StudyFlow123!`
- Additional student: `alex@studyflowpro.dev` / `StudyFlow123!`

## Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 with ASP.NET and web development workload

### Run in Visual Studio

1. Open [StudyFlowPro.sln](StudyFlowPro.sln).
2. Set `StudyFlowPro.Web` as the startup project if needed.
3. Run the application.

On startup, the app will:

- apply EF Core migrations automatically
- create the SQLite database if it does not exist
- seed roles, demo users, subjects, and study tasks

### Run from the CLI

```bash
dotnet restore StudyFlowPro.sln
dotnet build StudyFlowPro.sln
dotnet run --project src/StudyFlowPro.Web
```

## Database Setup

SQLite is used by default through the connection string in [appsettings.json](src/StudyFlowPro.Web/appsettings.json).

The database file is created automatically on first run:

- `src/StudyFlowPro.Web/studyflowpro.db`

If you want to reset the demo data, delete the SQLite database file and run the app again.

If you prefer to apply migrations manually:

```bash
dotnet tool restore
dotnet dotnet-ef database update --project src/StudyFlowPro.Web --startup-project src/StudyFlowPro.Web
```

## Testing

Run the test suite with:

```bash
dotnet test StudyFlowPro.sln
```

The test project covers:

- overdue task calculation
- dashboard metric aggregation
- task form validation behavior

## Project Structure

```text
StudyFlowPro
|-- src
|   `-- StudyFlowPro.Web
|       |-- Areas/Identity
|       |-- Authorization
|       |-- Controllers
|       |-- Data
|       |-- Extensions
|       |-- Hubs
|       |-- Models
|       |-- Services
|       |-- ViewModels
|       |-- Views
|       `-- wwwroot
|-- tests
|   `-- StudyFlowPro.Tests
|-- docs
|   `-- screenshots
`-- .github/workflows
```

## Screenshots

Screenshot placeholders live in [docs/screenshots/README.md](docs/screenshots/README.md).

Suggested captures:

- dashboard overview
- task board with filters
- subject management screen
- dark mode UI
- login/register flow

## Continuous Integration

GitHub Actions is configured in [ci.yml](.github/workflows/ci.yml) to:

- restore dependencies
- build the solution
- run the test suite

## Future Improvements

- calendar and weekly planner views
- email reminders for deadlines
- drag-and-drop task organization
- attachment uploads for assignments
- richer admin reporting and user management
- study streaks and productivity trends

## Why This Works as a Portfolio Project

StudyFlowPro shows practical full-stack C# skills across authentication, EF Core data modeling, MVC architecture, real-time features, validation, testing, seeded demo data, and GitHub-ready documentation. It is intentionally easy to extend, so it can continue evolving into a stronger showcase project over time.
