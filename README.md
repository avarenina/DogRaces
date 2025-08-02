# Dog Race Betting System — Clean Architecture Backend

## What's included in this project?

- **Domain layer** with core entities:
  - Rounds (races), Dogs, Tickets, Wallets
  - Business rules and domain models for dog race betting

- **Application layer** implementing CQRS pattern:
  - Commands and Queries for betting, offers, and processing
  - Use case handlers encapsulating business logic
  - Validation of tickets and betting constraints

- **Infrastructure layer** with:
  - PostgreSQL integration using Entity Framework Core
  - Background services for race round creation and result processing
  - In-memory wallet simulation per user session
  - Logging support (Serilog)
  - **AspNetCore.Identity** for local authentication

- **API layer** (ASP.NET Core Web API) that:
  - Exposes REST endpoints for placing bets, querying rounds, and checking wallet balance
  - Uses MediatR to dispatch commands and queries to Application layer
  - Depends on both Application and Infrastructure projects for clean dependency flow

- **Tests**  that:
  - ...


---

## Core features

- Create and maintain at least 5 upcoming dog race rounds with predefined start times.
- Five seconds before each round:
  - Automatically determine and finalize the race winner.
  - Process all tickets, updating statuses (Won, Lost, etc.) and handling payouts.
- Simulate user wallets with initial balances of 100 units.
- Support one bet type: race winner.
- Ticket lifecycle with validation, funds withdrawal, revalidation, and finalization.
- Clean separation of concerns using Clean Architecture and CQRS.
- Background processing with HostedServices for scheduled tasks.

---

## Technologies and tools

- **.NET 9 / C#**
- **ASP.NET Core Web API**
- **Entity Framework Core** with **PostgreSQL**
- **MediatR** for CQRS
- **Serilog** for logging
- Background services with `IHostedService`
- Docker support for local development

---

## How to run
