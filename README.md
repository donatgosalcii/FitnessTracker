[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

# FitnessTracker

FitnessTracker is a web application built with ASP.NET Core 8.0 using Clean Architecture principles. It allows users to track their fitness activities—such as logging workouts, exercises, and workout sets—in a secure and extensible platform. It also includes an AI-powered fitness assistant to answer fitness-related questions.

---

## Project Goals

- Log and manage workouts and exercises.
- Track sets with reps, weights, and durations.
- Provide user authentication via Identity and JWT.
- Offer both Razor Pages UI and API access.
- AI-powered fitness assistant to answer questions about workouts, nutrition, and general fitness.

---

## Tech Stack

| Layer              | Tech/Tool                                           |
|-------------------|-----------------------------------------------------|
| Framework          | ASP.NET Core 8.0                                    |
| Architecture       | Clean Architecture (Domain, Application, etc.)     |
| Authentication     | ASP.NET Core Identity + JWT Bearer Token           |
| UI                 | Razor Pages + Bootstrap                             |
| Data Access        | EF Core 8 (Code-First) + SQL Server                 |
| AI Integration     | Ollama with Llama 3                                 |
| IDE (Dev)          | JetBrains Rider on Ubuntu Linux                     |

---

## Solution Structure

```
src/
├── FitnessTracker.Domain        # Entities, relationships, EF configurations
├── FitnessTracker.Application   # DTOs, Interfaces, Services
├── FitnessTracker.Infrastructure# EF DbContext, Repositories, JWT Auth
└── FitnessTracker.WebUI         # Razor Pages, Controllers, Program.cs
```

---

## Authentication Overview

- **ASP.NET Core Identity (Cookies):** Used for UI session auth (Razor Pages).
- **JWT Bearer Token:** Used for backend API auth (issued on login, stored in HttpOnly cookie).
- **Mixed-Mode Auth:** Configured to support both seamlessly.

---

## Features

### Account Management
- Register/Login/Logout with secure password handling
- Role-based access control (User, Admin)
- JWT-based token issuance with expiration and claims

### Muscle Groups (Admin-only)
- CRUD operations via API and UI
- Linked to exercises

### Exercises (Admin-only)
- Create/Edit/Delete exercises with associated muscle groups
- Uses multi-select UI components
- Visible in workout forms

### Workouts (Users)
- Log new workouts with multiple sets
- View workout history and details
- Edit and delete past workouts
- Dynamic client-side JavaScript for managing sets

### AI Fitness Assistant
- Chat interface for asking fitness-related questions
- Powered by Ollama with the Llama 3 model
- Provides expert answers about workouts, nutrition, exercise technique, and more
- Runs locally with no external API costs

---

## Configuration

### `appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FitnessTrackerDb;User Id=sa;Password=YourPasswordHere;"
},
"JwtSettings": {
  "Secret": "YourSuperSecretKeyHere",
  "Issuer": "http://localhost:5179",
  "Audience": "http://localhost:5179",
  "ExpiryMinutes": 60
},
"Ollama": {
  "BaseUrl": "http://localhost:11434",
  "Model": "llama3",
  "SystemPrompt": "You are a knowledgeable fitness assistant. Provide accurate, helpful advice about exercise, nutrition, and general fitness."
}
```

### `launchSettings.json`
```json
"applicationUrl": "https://localhost:7115;http://localhost:5179"
```

---

## Testing Status

| Area                  | Status   |
|-----------------------|----------|
| Account API           | Tested via Postman/UI |
| MuscleGroup API/UI    | Working & verified    |
| Exercise API/UI       | Working & verified    |
| Workout Logging       | Working with dynamic JS |
| Workout Editing       | In Progress (currently being tested) |
| AI Fitness Assistant  | Working with Ollama integration |

---

## Known Issues / Workarounds

- HTTPS redirect may fail due to local dev certs; HTTP used by default.
- JWT Bearer cookies stored as `HttpOnly` to prevent XSS access.
- WorkoutSet editing uses a "delete-and-reinsert" pattern for simplicity.
- UI pages currently built for desktop browser usage.
- Ollama must be installed and running for the AI chat feature to work properly.

---

## Next Steps

- Complete Edit Workout Testing
- Add server/client-side validations (FluentValidation)
- Implement structured logging (e.g., Serilog)
- Write unit and integration tests
- Improve error handling and user messages
- Add Swagger for API documentation
- Deploy with Docker + CI/CD
- Enhance AI assistant with more fitness-specific knowledge

---

## Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/your-username/FitnessTracker.git
   cd FitnessTracker
   ```

2. Apply Migrations:
   ```bash
   dotnet ef database update --project src/FitnessTracker.Infrastructure
   ```

3. Install Ollama (for AI chat feature):
   ```bash
   # For Linux
   curl -fsSL https://ollama.com/install.sh | sh
   
   # For macOS
   curl -fsSL https://ollama.com/install.sh | sh
   
   # For Windows, download from https://ollama.com/download
   ```

4. Pull the Llama 3 model:
   ```bash
   ollama pull llama3
   ```
   
5. Ensure Ollama is running:
   ```bash
   # Ollama should start automatically after installation
   # If not, run:
   ollama serve
   
   # In a separate terminal window, you can test Ollama with:
   ollama run llama3 "Give me a quick fitness tip"
   ```

6. Run the app:
   ```bash
   dotnet run --project src/FitnessTracker.WebUI
   ```

7. Open in browser:  
   `http://localhost:5179` or `https://localhost:7115`

8. Navigate to the AI Assistant page to ask fitness-related questions.

---

## AI Fitness Assistant Setup

The application includes an AI-powered fitness assistant that allows users to ask questions about workouts, nutrition, and general fitness topics. This feature requires Ollama to be installed and running on your system.

### Ollama Configuration

- **Installation**: Follow the steps in the "Getting Started" section to install Ollama.
- **Model**: The application uses the Llama 3 model by default, which provides high-quality responses for fitness questions.
- **Customization**: You can modify the system prompt in `appsettings.json` to customize the AI assistant's behavior.
- **Troubleshooting**: If the AI assistant returns generic responses, ensure that:
  - Ollama is running (`ollama serve`)
  - The Llama 3 model is downloaded (`ollama pull llama3`)
  - The application can connect to Ollama at `http://localhost:11434`

---

## Developer Notes

- Built with Clean Architecture for maintainability.
- All services are injected via `AddScoped`.
- Supports Razor Pages UI and RESTful API simultaneously.
- Uses IHttpClientFactory for internal API calls.
- AI chat implementation has a fallback mechanism if Ollama is unavailable.

---

## License

MIT License. See `LICENSE.txt` for details.

---

## Acknowledgements

- Bootstrap & jQuery for frontend components
- Microsoft Identity for authentication
- FluentValidation (planned) for model validation
- Ollama and Meta's Llama 3 model for the AI fitness assistant
