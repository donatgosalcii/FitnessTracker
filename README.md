# FitnessTracker

A personal project to build an ASP.NET Core 8 web application for tracking fitness activities (workouts, exercises, sets, etc.), structured using Clean Architecture principles. Currently focusing on building the backend API.

## Core Technologies

*   **Backend:** ASP.NET Core 8.0 (`net8.0`)
*   **Architecture:** Clean Architecture
*   **Data Access:** Entity Framework Core 8 (Code First)
*   **Database:** SQL Server
*   **Authentication:** ASP.NET Core Identity with JWT Bearer Tokens
*   **Presentation:** ASP.NET Core Web App (API Controllers primarily)

## Project Structure (Clean Architecture Layers)

*   `src/Domain`: Core entities, enums, domain logic, EF configurations (Fluent API).
*   `src/Application`: DTOs, Interfaces (Services, Repositories, DbContext), Application Services (Business Logic), Validation.
*   `src/Infrastructure`: DbContext implementation, Repository implementations, Migrations, External Service implementations (e.g., JWT Generator).
*   `src/Presentation/FitnessTracker.WebUI`: ASP.NET Core startup project, API Controllers, `Program.cs`, `appsettings.json`.

## Setup

1.  **Prerequisites:**
    *   .NET 8 SDK: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
    *   SQL Server Instance (e.g., LocalDB, Docker container, full instance). Ensure it's running and accessible.

2.  **Clone the Repository:**
    ```bash
    git clone <github.com/donatgosalcii/FitnessTracker>
    cd FitnessTracker
    ```

3.  **Configure Database Connection:**
    *   Open `src/Presentation/FitnessTracker.WebUI/appsettings.Development.json`.
    *   Locate the `ConnectionStrings.DefaultConnection` value.
    *   Update it to point to your SQL Server instance. Example:
        ```json
        "DefaultConnection": "Server=localhost,1433;Database=FitnessTrackerDb;User ID=sa;Password=YourStrong!Password;TrustServerCertificate=True;"
        ```
    *   **Important:** Ensure the `Database` name is `FitnessTrackerDb`, and replace the `Server`, `User ID`, and `Password` with your specific SQL Server details. (`TrustServerCertificate=True` might be needed for local development without proper SSL setup).

4.  **Apply Database Migrations:**
    *   Open a terminal in the **root directory** of the solution (where `FitnessTracker.sln` is).
    *   Run the EF Core migration command (this applies all pending migrations to create/update the database schema):
        ```bash
        dotnet ef database update --project src/Infrastructure/FitnessTracker.Infrastructure --startup-project src/Presentation/FitnessTracker.WebUI
        ```
        *Note: Ensure the paths in the command match your project structure if different.*

5.  **Build the Solution:**
    ```bash
    dotnet build
    ```

## Running the Application

1.  **Run from Terminal:**
    *   Ensure you are in the root directory.
    *   Run the WebUI project:
        ```bash
        dotnet run --project src/Presentation/FitnessTracker.WebUI/FitnessTracker.WebUI.csproj
        ```
2.  **Access the API:**
    *   The application will typically start on `https://localhost:7115` and `http://localhost:5179` (check the console output for the exact ports).
    *   API endpoints are accessed via `/api/...` (e.g., `https://localhost:7115/api/account/login`).

## Using the API

*   Use an API client like Postman or Insomnia.
*   **Registration:** `POST /api/account/register` (see `UserRegistrationDto.cs` for body).
*   **Login:** `POST /api/account/login` (see `UserLoginDto.cs` for body). Returns a JWT and user details on success.
*   **Protected Endpoints:** Most endpoints (like `/api/musclegroups`, `/api/exercises`) require authentication. You need to:
    1.  Log in to get a JWT `token`.
    2.  Include the token in the `Authorization` header for subsequent requests: `Authorization: Bearer <your_token_here>`.
*   **Admin Operations:** Some operations (creating/updating/deleting muscle groups and exercises) require the user to have the "Admin" role.

## Current API Endpoints

*   `/api/account/register` (POST)
*   `/api/account/login` (POST)
*   `/api/musclegroups` (GET, POST) - *POST requires Admin*
*   `/api/musclegroups/{id}` (GET, PUT, DELETE) - *PUT/DELETE require Admin*
*   `/api/exercises` (GET, POST) - *POST requires Admin*
*   `/api/exercises/{id}` (GET, PUT, DELETE) - *PUT/DELETE require Admin*
*   `/api/values/public` (GET) - *Test endpoint*
*   `/api/values/secure` (GET) - *Test endpoint, requires Auth*
*   `/api/values/admin` (GET) - *Test endpoint, requires Admin role*

*(This list will grow as development progresses)*

---

*Work in progress...*
