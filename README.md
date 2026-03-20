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

The repo includes `vercel.json` configured to publish the Blazor WebAssembly app as a static site on Vercel with SPA fallback routing.
