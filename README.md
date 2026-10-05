# EkubCircle — Rotating Savings & Credit Associations Digital Ledger

**QIYAS Full-Stack Development Hackathon 2026 — Challenge 3: EkubCircle**

EkubCircle is an audit-compliant, robust digital ledger for Ethiopian Rotating Savings and Credit Associations (Ekub / ዕቁብ). The backend is architected in ASP.NET Core with Entity Framework Core, SQLite, and an enterprise **Multi-Project Onion (Clean) Architecture** leveraging **MediatR (CQRS)** and **AutoMapper**.

---

## 👥 Team Information

- **Project**: EkubCircle
- **Challenge**: Challenge 3 — EkubCircle
- **Team**: EkubCircle Dev Team
- **Roles**:
  - Backend Architect & Engineer: ASP.NET Core, EF Core, CQRS with MediatR, Domain Engine, SQLite, REST API
  - Frontend Developer: User Interface & Client Experience (Separately developed)

---

## 🏛️ Onion Architecture & Directory Tree

The backend strictly adheres to concentric architectural layers with dependency inversion where dependencies point strictly inward toward the core domain:

```
backend/
├── EkubCircle.sln
├── src/
│   ├── EkubCircle.Domain/                                 # Innermost Core Domain Layer
│   │   ├── Entities/                                      # Domain Entities (No external dependencies)
│   │   │   ├── User.cs                                    # Registered accounts (Admin, Organizer, Member)
│   │   │   ├── Circle.cs                                  # Savings circle aggregate root
│   │   │   ├── CircleMember.cs                            # Membership with deterministic order & receipt status
│   │   │   ├── Round.cs                                   # Pre-scheduled deterministic rounds
│   │   │   └── Payment.cs                                 # Contribution & Payout records (with IsLate flag)
│   │   ├── Enums/                                         # Pure Domain Enums
│   │   │   └── EkubEnums.cs                               # UserRole, CircleStatus, CircleRole, RoundStatus, PaymentType
│   │   └── Exceptions/                                    # Domain Exceptions
│   │       └── DomainExceptions.cs                        # DomainException, NotFoundException, BusinessRuleException
│   │
│   ├── EkubCircle.Application/                             # Application / Use Cases Layer
│   │   ├── Abstractions/                                  # Application Abstractions
│   │   ├── Commands/                                      # CQRS State-Mutating Commands (MediatR IRequest)
│   │   │   ├── Auth/                                      # RegisterUserCommand, LoginUserCommand
│   │   │   ├── Circles/                                   # CreateCircleCommand, AddMemberCommand, RemoveMemberCommand, StartCircleCommand
│   │   │   ├── Payments/                                  # RecordPaymentCommand (anti-duplicate, late tracking)
│   │   │   └── Rounds/                                    # ExecutePayoutCommand, DrawRoundWinnerCommand (Fair Draw)
│   │   ├── Queries/                                       # CQRS Read Queries (MediatR IRequest)
│   │   │   ├── Auth/                                      # GetCurrentUserQuery
│   │   │   ├── Circles/                                   # GetUserCirclesQuery, GetCircleByIdQuery, GetCircleSummaryQuery
│   │   │   ├── Payments/                                  # GetPaymentsQuery
│   │   │   └── Rounds/                                    # GetCurrentRoundQuery, GetCircleRoundsQuery
│   │   ├── Handlers/                                      # MediatR Command & Query Handlers
│   │   │   ├── Auth/                                      # RegisterUserCommandHandler, LoginUserCommandHandler, GetCurrentUserQueryHandler
│   │   │   ├── Circles/                                   # CreateCircleCommandHandler, AddMemberCommandHandler, etc.
│   │   │   ├── Payments/                                  # RecordPaymentCommandHandler, GetPaymentsQueryHandler
│   │   │   └── Rounds/                                    # ExecutePayoutCommandHandler, DrawRoundWinnerCommandHandler, etc.
│   │   ├── Common/                                        # Common Interfaces & Mappings
│   │   │   ├── Interfaces/                                # IEkubDbContext, ITokenService, IPasswordHasher, IJwtTokenGenerator
│   │   │   └── Mappings/                                  # AutoMapper Profile (MappingProfile)
│   │   └── DTOs/                                          # Data Transfer Objects
│   │       ├── Auth/                                      # Register, Login, User, AuthResponse DTOs
│   │       ├── Circles/                                   # CreateCircle, AddMember, CircleDto, CircleSummaryDto
│   │       ├── Payments/                                  # RecordPayment, PaymentDto
│   │       └── Rounds/                                    # CurrentRoundDto, PayoutResultDto, DrawWinnerDto
│   │
│   ├── EkubCircle.Infrastructure/                          # Outermost Infrastructure Layer
│   │   ├── Identity/                                      # Security & Token implementations
│   │   │   ├── TokenService.cs                            # HMAC-SHA256 JWT Token Generation
│   │   │   └── PasswordHasher.cs                          # BCrypt-compatible PBKDF2 Password Hashing
│   │   ├── Persistence/                                   # Data Access & Database Configurations
│   │   │   ├── Context/                                   # EF Core DbContext & Factory
│   │   │   │   ├── EkubDbContext.cs                       # Implements IEkubDbContext
│   │   │   │   └── EkubDbContextFactory.cs                # Design-time factory for EF Core migrations
│   │   │   ├── Configurations/                            # Fluent Entity Configurations & Constraints
│   │   │   │   ├── UserConfiguration.cs                   # Email unique constraint
│   │   │   │   ├── CircleConfiguration.cs                 # Decimal precision & foreign keys
│   │   │   │   ├── CircleMemberConfiguration.cs           # UNIQUE(CircleId, UserId)
│   │   │   │   ├── RoundConfiguration.cs                  # UNIQUE(CircleId, RoundNumber)
│   │   │   │   └── PaymentConfiguration.cs                # UNIQUE(RoundId, MemberId, PaymentType)
│   │   │   └── SeedData/                                  # Deterministic Seed Data
│   │   │       └── DbInitializer.cs                       # Seeds Admin, Organizer, Members, Demo Circle & Rounds
│   │
│   └── EkubCircle.API/                                     # Presentation Layer (Thin Web API Controllers)
│       ├── Controllers/                                   # Thin API Controllers (Inject ONLY ISender & IMapper)
│       │   ├── AuthController.cs                          # /api/auth
│       │   ├── CirclesController.cs                       # /api/circles
│       │   ├── RoundsController.cs                        # /api/rounds
│       │   └── PaymentsController.cs                      # /api/payments
│       ├── Middlewares/                                   # Global Middlewares
│       │   └── ExceptionMiddleware.cs                     # Converts DomainExceptions to standardized HTTP 400/404/500 JSON
│       └── Program.cs                                     # Dependency Injection Composition Root & Swagger
└── test-api.ps1                                           # 10-Step Automated Verification Suite
```

> **Design Pattern Enforcement**: All controllers adhere to strict Clean Architecture rules: they inject **only `ISender` (MediatR)** and **`IMapper` (AutoMapper)**. No controllers perform business logic or touch entity repositories directly.

---

## 📊 Database Schema & ER Alignment

The database schema directly maps to the official Eraser ER diagram:

- **`USERS`**: `Id` (PK), `FullName`, `Email` (UNIQUE), `PasswordHash`, `Role` (Member, Organizer, Admin), `CreatedAt`
- **`CIRCLES`**: `Id` (PK), `Name`, `ContributionAmount`, `MeetingLabel`, `Status` (Forming, Active, Completed), `CreatedByUserId` (FK), `CreatedAt`, `StartedAt`, `CompletedAt`
- **`CIRCLE_MEMBERS`**: `Id` (PK), `CircleId` (FK), `UserId` (FK), `MemberOrder`, `RoleInCircle`, `HasReceived`, `JoinedAt` (Constraint: `UNIQUE(CircleId, UserId)`)
- **`ROUNDS`**: `Id` (PK), `CircleId` (FK), `RoundNumber`, `ReceiverMemberId` (FK), `Status` (Open, PaidOut), `PotAmount`, `PaidOutAt` (Constraint: `UNIQUE(CircleId, RoundNumber)`)
- **`PAYMENTS`**: `Id` (PK), `RoundId` (FK), `MemberId` (FK), `Amount`, `PaymentType`, `PaymentMethod`, `Notes`, `IsLate` (Boolean), `PaidAt`, `RecordedByUserId` (FK) (Constraint: `UNIQUE(RoundId, MemberId, PaymentType)`)

---

## 🔒 The 7 Core Server-Side Hard Rules Enforced

The API acts as the single source of truth and strictly enforces all business rules on the server side:

1. **Member List Locks on Start**: Circle members can only be added or removed when the circle is in `Forming` status. Once `POST /api/circles/{id}/start` is called, status switches to `Active` and member roster modifications are rejected (`HTTP 400 Bad Request`).
2. **Deterministic Payout Order**: Payout order ($1..N$) is locked deterministically at circle start based on member sequence. The system pre-assigns the receiver for each round.
3. **Exactly One Receiver per Round**: Each round has an assigned `ReceiverMemberId` that cannot be duplicated or bypassed.
4. **100% Member Contribution Gate**: Payout cannot occur until every registered member of the circle has paid their contribution for that round (`HTTP 400 Bad Request` if any member is unpaid).
5. **Single Pot Receipt Rule**: A member can receive the pot at most once (`HasReceived` flag). The server rejects payouts if a member already received a pot.
6. **Ongoing Payment Obligation**: Members who have already received their payout must continue contributing in all subsequent rounds.
7. **Duplicate Payment Prevention**: Composite database index `UNIQUE(RoundId, MemberId, PaymentType)` and handler validation prevent duplicate contributions for the same round (`HTTP 400 Bad Request`).

---

## 🌟 Innovation & Extra Credit Features (Criterion 5: 10 pts)

1. **Server-Side Cryptographically Fair Draw Simulator (`POST /api/rounds/{roundId}/draw`)**:
   - Uses `RandomNumberGenerator` for cryptographically strong, non-deterministic random selection.
   - **Strict Rule Enforcement**: Only members who have **paid** for the round AND **have not yet received** a pot are eligible. Unpaid members are strictly disqualified from winning.
   - Automatically assigns the chosen member as the round's receiver.

2. **Completed-Circle Summary & Audit Report (`GET /api/circles/{id}/summary`)**:
   - Comprehensive audit endpoint for completed or active circles.
   - Calculates total pot collected, payout timelines, on-time vs. late contribution breakdown per member, and payout history.

3. **Late Contributor Flag & Audit Tracking (`IsLate`)**:
   - Payments track an `IsLate` boolean flag.
   - Enables organizers to audit timely payments vs late payments across all rounds.

---

## 👥 Seeded Demo Accounts (for Judges & Testing)

| Role | Email | Password | Full Name |
|---|---|---|---|
| **System Admin** | `admin@hackathon.local` | `Admin123!` | System Administrator |
| **Organizer** | `organizer@ekub.local` | `Ekub123!` | Abebe Bikila (Circle Organizer) |
| **Member 1** | `member1@ekub.local` | `Ekub123!` | Hana Girma |
| **Member 2** | `member2@ekub.local` | `Ekub123!` | Dawit Tadesse |
| **Member 3** | `member3@ekub.local` | `Ekub123!` | Meron Bekele |
| **Member 4** | `member4@ekub.local` | `Ekub123!` | Selam Haile |

---

## 🚀 Running the Backend

### Prerequisites
- [.NET 9 / .NET 10 SDK](https://dotnet.microsoft.com/)

### 1. Build & Run the API Server
```powershell
dotnet build backend/EkubCircle.sln
dotnet run --project backend/src/EkubCircle.API/EkubCircle.API.csproj --urls "http://localhost:5000"
```

- **Swagger UI**: Accessible at `http://localhost:5000/` or `http://localhost:5000/swagger`
- **CORS**: Configured with `AllowAll` for local frontend development servers.

### 2. Run the 10-Step Automated Verification Suite
We provide an automated PowerShell test suite verifying all 7 server-side rules and extra credit features end-to-end:

```powershell
powershell -ExecutionPolicy Bypass -File backend/test-api.ps1
```

**Verified Test Cases:**
1. Authentication & JWT profile retrieval
2. Circle creation & member invitations
3. Circle start & deterministic round generation
4. Server rejection of member additions after circle start (`HTTP 400`)
5. Round state & pot calculation engine
6. 100% member contribution gate (`HTTP 400` on premature payout attempt)
7. Payment recording & duplicate contribution rejection (`HTTP 400`)
8. Payout disbursement, round advancement, and duplicate payout rejection (`HTTP 400`)
9. Server-side fair draw simulator (verifying unpaid members cannot win)
10. Circle completion audit report generation

---

## 📡 API Endpoints Reference

### Authentication (`/api/auth`)
- `POST /api/auth/register` — Register a new account
- `POST /api/auth/login` — Log in and receive a JWT Bearer token
- `GET /api/auth/me` — Retrieve current authenticated user profile

### Circles (`/api/circles`)
- `POST /api/circles` — Create a new circle (creator assigned as Organizer)
- `GET /api/circles` — List user's circles (`?status=forming|active|completed`)
- `GET /api/circles/{id}` — Get circle details, members list, and round status
- `POST /api/circles/{id}/members` — Add member by email (Organizer only, Forming only)
- `DELETE /api/circles/{id}/members/{memberId}` — Remove member (Organizer only, Forming only)
- `POST /api/circles/{id}/start` — Lock member list, assign deterministic payout order, generate rounds
- `GET /api/circles/{id}/summary` — Completed-circle audit report and member metrics

### Rounds (`/api/rounds`)
- `GET /api/rounds/current?circleId={id}` — Current open round details, receiver info, member checklist, pot calculation
- `GET /api/rounds?circleId={id}` — List all rounds and payout statuses
- `POST /api/rounds/{roundId}/payout` — Execute pot payout (enforces 100% payment gate & single receipt rule)
- `POST /api/rounds/{roundId}/draw` — Server-side cryptographically fair draw simulator

### Payments (`/api/payments`)
- `POST /api/payments` — Record member contribution (prevents duplicate payments, supports `IsLate` flag)
- `GET /api/payments?circleId={id}&roundId={id}` — Audit trail of payments