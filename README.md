# 🐕 Dog Race Betting System (Powered by .NET Aspire)

A real-time, event-driven dog race betting system using Clean Architecture, CQRS, SignalR, Redis, and .NET Aspire for orchestrated microservice-style composition.

---

## 🧱 Project Structure (Clean Architecture + .NET Aspire)

### Domain Layer
- Core business entities:
  - **Race**, **Bet**, **Ticket**
- Domain logic and validation rules for betting

### Application Layer
- Implements **CQRS pattern** (Command Query Responsibility Segregation)
- Encapsulates use cases:
  - Placing bets
  - Processing races
  - Validating tickets
- Enforces business constraints

### Infrastructure Layer
- **Entity Framework Core** with **PostgreSQL**
- **Redis**:
  - As a distributed **cache**
  - For **Pub/Sub** messaging
- In-memory **wallet simulation** per session

### API Layer (ASP.NET Core Web API)
- REST endpoints to:
  - Place bets
- **SignalR integration** for real-time updates (race results, wallet changes)
- Composed via **.NET Aspire AppHost**

### Background Service (Separate Project)
- Runs independently as a **hosted worker service**
- Responsible for:
  - Generating new race rounds
  - Determining race winners
  - Processing bet outcomes and payouts
  - Publishing results to Redis Pub/Sub and SignalR
- Registered and orchestrated through Aspire

### Aspire AppHost
- Coordinates services:
  - API
  - Background Worker
  - Redis
  - PostgreSQL
- Enables diagnostics, orchestration, and local development efficiency

---

## 🚀 Core Features

- Automatically create and maintain at least 5 upcoming dog races
- Finalize races 5 seconds before start and determine winners
- Process tickets and payouts
- Push real-time race results via **SignalR**
- Wallet simulation with an initial 100-unit balance
- Ticket lifecycle: validation → withdrawal → finalization
- Modular architecture with independent background service
- Uses **Redis Pub/Sub** for internal service communication
- Real-time and event-driven via **SignalR** and background worker

---

## 🛠 Technologies Used

| Tech | Purpose |
|------|---------|
| **.NET 9 / C#** | Backend runtime |
| **ASP.NET Core Web API** | API layer |
| **Entity Framework Core** | ORM |
| **PostgreSQL** | Relational database |
| **Redis** | Caching and Pub/Sub |
| **SignalR** | Real-time client communication |
| **Hosted Worker Service** | Background job execution |
| **.NET Aspire** | App orchestration and diagnostics |
| **Docker** | Containerization support |

---

## ⚙️ Getting Started