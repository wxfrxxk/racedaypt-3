# RaceDay (PROG6212 POE)

RaceDay is a full-stack event management platform for South African road running, walking and cycling events.
Organisers create and manage events, categories and results. Participants browse events, enter a category,
track their enrolments and view their personal race history.

| Part | Content |
|------|---------|
| Part 1 | ERD, API endpoint plan and SQL script (`/docs`) |
| Part 2 | ASP.NET Core Web API (`RaceDay.API`) with EF Core, session authentication, Swagger and unit tests (`RaceDay.API.Tests`) |
| Part 3 | ASP.NET Core MVC front end (`RaceDay.MVC`), Azure Blob Storage, Docker and CI/CD |

- **YouTube video (Part 3) // I could not upload the video , apologies for that. 
- **CI/CD green build:**

  My screenshots will be displayed here.
  ## Project Screenshots

## RaceDay Project Screenshots

## Project Screenshots

## Project Screenshots

![Screenshot 1](docs/Screenshot%202026-11-09%20224606.png)

![Screenshot 2](docs/Screenshot%202026-11-09%20230958.png)

![Screenshot 3](docs/Screenshot%202026-11-09%20231024.png)

![Screenshot 4](docs/Screenshot%202026-11-09%20231128.png)

![Screenshot 5](docs/Screenshot%202026-11-09%20231154.png)

![Screenshot 6](docs/Screenshot%202026-11-09%20231210.png)

![Screenshot 7](docs/Screenshot%202026-11-09%20231302.png)

![Screenshot 8](docs/Screenshot%202026-11-09%20231315.png)

## Roles

| Role | Can do |
|------|--------|
| **Organiser** | Dashboard of own events and enrolments, create / edit / delete events (with banner image), manage categories, view and confirm / cancel enrolments, capture and publish results |
| **Participant** | Register an account, browse and filter upcoming events, view event details and categories, enter an event by choosing a category, view own enrolments and statuses, view own results (time and position), edit profile and upload a profile picture |

Organiser accounts are created by the administrator (seeded). Registration through the website creates Participants.
Roles are enforced in the API (session + role checks) and mirrored in the MVC app (role filters and role-specific menus).

## Architecture

```
Browser  ->  RaceDay.MVC (ASP.NET Core MVC)  ->  RaceDay.API (Web API + EF Core)  ->  SQL Server
                                                         |
                                                         +->  Azure Blob Storage (event banners, profile pictures)
```

- The MVC project has **no database access and no business logic**. Every operation is an HTTP call to the API.
- On login the API creates a server-side session. The MVC app keeps the API's session cookie in its own session
  and forwards it on every call, so the API always knows the user id and role.
- Images are uploaded from MVC to the API (`multipart/form-data`). Only the API talks to Azure Blob Storage;
  the resulting blob URL is stored in the database and shown in the MVC pages.

## Run locally (Visual Studio)

Requirements: .NET 8 SDK, SQL Server (Express is fine), Visual Studio 2022.

1. Open `RaceDay.sln`.
2. In `RaceDay.API/appsettings.json` set `ConnectionStrings:RaceDayConnection` to your SQL Server instance.
   The default database name is `RaceDayPart3`; it is created automatically.
3. Create the EF Core migration once (Package Manager Console, **Default project: RaceDay.API**):
   ```
   Add-Migration InitialCreate
   ```
   Commit the generated `Migrations` folder. The API applies migrations and seeds demo data on start-up.
4. Right-click the solution, **Configure Startup Projects**, choose **Multiple startup projects** and set
   `RaceDay.API` and `RaceDay.MVC` to **Start**. Press F5.
   - API (Swagger): http://localhost:5100/swagger
   - MVC app: http://localhost:5200

### Demo accounts (seeded)

| Role | Email | Password |
|------|-------|----------|
| Organiser | `thabo@raceday.co.za` | `Organiser#123` |
| Organiser | `lerato@raceday.co.za` | `Organiser#123` |
| Participant | `sipho@example.com` | `Participant#123` |
| Participant | `aisha@example.com` | `Participant#123` |

Passwords are stored hashed (PBKDF2, SHA-256, 100 000 iterations).

## Azure Blob Storage

1. In the Azure portal create a **Storage account** (Standard, LRS) and enable **Allow Blob anonymous access**
   (Configuration blade) so the browser can display uploaded images.
2. Copy the connection string (Security + networking -> Access keys).
3. Set it **without committing it**:
   - Visual Studio: right-click `RaceDay.API` -> **Manage User Secrets** and add
     `{ "AzureBlob": { "ConnectionString": "<your connection string>" } }`
   - Docker: `AZURE_BLOB_CONNECTION_STRING` environment variable (see below).
4. The API creates the container `raceday-images` on first upload.

If no connection string is configured, the API falls back to a local `wwwroot/uploads` folder so the app still runs
during development and in CI. For marking, run with Azure configured.

## Docker

The MVC application is containerised (`RaceDay.MVC/Dockerfile`, multi-stage build, non-root user).

**Single command (API already running locally on port 5100):**
```bash
docker build -t raceday-mvc ./RaceDay.MVC
docker run --rm -p 8080:8080 --add-host=host.docker.internal:host-gateway -e ApiBaseUrl=http://host.docker.internal:5100/ -e ApiPublicUrl=http://localhost:5100 raceday-mvc
```
Open http://localhost:8080.

**Whole platform (SQL Server + API + MVC):**
```bash
docker compose up --build
```
MVC: http://localhost:8080, Swagger: http://localhost:5100/swagger.
To use Azure: `AZURE_BLOB_CONNECTION_STRING="<connection string>" docker compose up --build`.

## CI/CD

`.github/workflows/ci.yml` runs on every push: validates the repository structure, restores and builds the whole
solution (API, tests and MVC), runs all unit tests and builds the API and MVC Docker images.

## Changes to the Part 1 / Part 2 design

Part 3 required a few additions to the data model. They are created by the EF Core migration:

| Table | Change | Reason |
|-------|--------|--------|
| `Event` | `Distance` (decimal), `BannerUrl` (nullable) | Event distance and banner image (Azure Blob Storage) |
| `User` | `ProfilePictureUrl` (nullable) | Profile picture (Azure Blob Storage) |
| `Registration` | `Status` (`Pending` / `Confirmed` / `Cancelled`, default `Pending`) | Colour-coded enrolment status |

New API endpoints added for the MVC app:

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| GET | `/api/Users/me` | Any | View own profile |
| PUT | `/api/Users/me` | Any | Update own profile |
| POST | `/api/Users/me/picture` | Any | Upload profile picture |
| GET | `/api/Events/mine` | Organiser | Dashboard data with enrolment counts |
| POST | `/api/Events/{id}/banner` | Organiser (owner) | Upload event banner |
| GET | `/api/Enrolments/event/{eventId}` | Organiser (owner) | Enrolments with participant, category, status and result |
| PUT | `/api/Enrolments/{id}/status` | Organiser (owner) | Confirm / cancel an enrolment |
| GET | `/api/Enrolments/my` | Any | Own enrolments with event details |
| GET | `/api/Enrolments/results/my` | Any | Own results with position out of total finishers |

## AI usage disclosure

AI assistance (Claude) was used for ideas and assistance while planning and scaffolding Part 3 (MVC structure, views, Docker and CI files).
The code was reviewed, run and tested by me, and I can explain every part of it in the video. Its unfortunate that i cant upload the video or do it due to time. 
