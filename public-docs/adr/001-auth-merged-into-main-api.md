# ADR 001: Merge authentication into the main API

## Context

The application previously considered a separate authentication service. The deployed project targets a low-resource hosting model where minimizing running services matters.

## Decision

Authentication endpoints live inside `WestcoastCars.Api` instead of a separate auth service.

## Why

- Minimizes deployment resource usage by avoiding an extra always-on service.
- Reduces operational overhead for a small hosted application.
- Still fits the current modular-monolith deployment model.

## Consequence

- Authentication and business endpoints share one deployed API surface.
- Deployment requires fewer compute resources than a split auth-service setup.
- Repository documentation should describe one API, not two services.
