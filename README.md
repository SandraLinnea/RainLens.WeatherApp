# RainLens

RainLens is a modern weather dashboard built with Blazor WebAssembly and .NET, with real-time weather data, a 5-day forecast, favorites, recent searches, and a rotating global forecast in a polished responsive UI.

## Features

- City-based weather search
- Live weather overview with last updated timestamp
- 5-day forecast for the selected city
- Favorite locations saved in local storage
- Recent searches for quick lookups
- Global forecast that rotates between world cities every 5 seconds

## Tech Stack

- Blazor WebAssembly
- .NET 10
- HttpClient
- Open-Meteo APIs
- localStorage for lightweight persistence

## Getting Started

1. Restore and build the project.
2. Run the Blazor app locally.
3. Search for a city and explore the dashboard.

## Deployment

RainLens deploys to Vercel through GitHub Actions because the Vercel Git build environment does not provide `dotnet` for this project.

### Required GitHub secrets

- `VERCEL_TOKEN`
- `VERCEL_ORG_ID`
- `VERCEL_PROJECT_ID`

### Deployment flow

1. GitHub Actions builds the Blazor WebAssembly app with .NET.
2. The workflow creates a `.vercel/output` folder with the published static files.
3. Vercel CLI deploys that prebuilt output with SPA fallback routing.

### Important

If your Vercel project is still connected for automatic Git-based deployments, disable those or disconnect the repository. Otherwise Vercel may still try to run its own build and fail before the GitHub Actions deployment takes over.
