🏋️ FitnessTracker

FitnessTracker is a web application built with ASP.NET Core 8.0 using Clean Architecture principles. It allows users to track their fitness activities—such as logging workouts, exercises, and workout sets—in a secure and extensible platform.

📌 Project Goals

Log and manage workouts and exercises.

Track sets with reps, weights, and durations.

Provide user authentication via Identity and JWT.

Offer both Razor Pages UI and API access.

💠 Tech Stack

Layer

Tech/Tool

Framework

ASP.NET Core 8.0

Architecture

Clean Architecture (Domain, Application, etc.)

Authentication

ASP.NET Core Identity + JWT Bearer Token

UI

Razor Pages + Bootstrap

Data Access

EF Core 8 (Code-First) + SQL Server

IDE (Dev)

JetBrains Rider on Ubuntu Linux

📂 Solution Structure

src/
├── FitnessTracker.Domain        # Entities, relationships, EF configurations
├── FitnessTracker.Application   # DTOs, Interfaces, Services
├── FitnessTracker.Infrastructure# EF DbContext, Repositories, JWT Auth
└── FitnessTracker.WebUI         # Razor Pages, Controllers, Program.cs

🔐 Authentication Overview

ASP.NET Core Identity (Cookies): Used for UI session auth (Razor Pages).

JWT Bearer Token: Used for backend API auth (issued on login, stored in HttpOnly cookie).

Mixed-Mode Auth: Configured to support both seamlessly.

📚 Features

✅ Account Management

Register/Login/Logout with secure password handling

Role-based access control (User, Admin)

JWT-based token issuance with expiration and claims

✅ Muscle Groups (Admin-only)

CRUD operations via API and UI

Linked to exercises

✅ Exercises (Admin-only)

Create/Edit/Delete exercises with associated muscle groups

Uses multi-select UI components

Visible in workout forms

✅ Workouts (Users)

Log new workouts with multiple sets

View workout history and details

Edit and delete past workouts

Dynamic client-side JavaScript for managing sets

⚙️ Configuration

appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FitnessTrackerDb;User Id=sa;Password=YourPasswordHere;"
},
"JwtSettings": {
  "Secret": "YourSuperSecretKeyHere",
  "Issuer": "http://localhost:5179",
  "Audience": "http://localhost:5179",
  "ExpiryMinutes": 60
}

launchSettings.json

"applicationUrl": "https://localhost:7115;http://localhost:5179"

🧪 Testing Status

Area

Status

Account API

✅ Tested via Postman/UI

MuscleGroup API/UI

✅ Working & verified

Exercise API/UI

✅ Working & verified

Workout Logging

✅ Working with dynamic JS

Workout Editing

🧰 In Progress (currently being tested)

🚧 Known Issues / Workarounds

HTTPS redirect may fail due to local dev certs; HTTP used by default.

JWT Bearer cookies stored as HttpOnly to prevent XSS access.

WorkoutSet editing uses a "delete-and-reinsert" pattern for simplicity.

UI pages currently built for desktop browser usage.

📊 Next Steps

✅ Complete Edit Workout Testing

\2b1c Add server/client-side validations (FluentValidation)

\2b1c Implement structured logging (e.g., Serilog)

\2b1c Write unit and integration tests

\2b1c Improve error handling and user messages

\2b1c Add Swagger for API documentation

\2b1c Deploy with Docker + CI/CD

🚀 Getting Started

Clone the repo:

git clone https://github.com/your-username/FitnessTracker.git
cd FitnessTracker

Apply Migrations:

dotnet ef database update --project src/FitnessTracker.Infrastructure

Run the app:

dotnet run --project src/FitnessTracker.WebUI

Open in browser:http://localhost:5179 or https://localhost:7115

👨‍💼 Developer Notes

Built with Clean Architecture for maintainability.

All services are injected via AddScoped.

Supports Razor Pages UI and RESTful API simultaneously.

Uses IHttpClientFactory for internal API calls.
