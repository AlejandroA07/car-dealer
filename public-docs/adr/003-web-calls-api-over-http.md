# ADR 003: Keep the web app talking to the API over HTTP

## Context

The repository contains both a user-facing web application and an API. The deployment model already treats them as separate services inside the same stack.

## Decision

`WestcoastCars.Web` calls `WestcoastCars.Api` over HTTP instead of invoking application logic in-process.

## Why

- Matches the Docker Compose and hosted deployment topology.
- Keeps the API as the single backend contract for UI-facing operations.
- Preserves separation between presentation concerns and backend behavior.

## Consequence

- The web application depends on a configured API base URL.
- End-to-end behavior should be reasoned about as `Web -> API -> Database`.
