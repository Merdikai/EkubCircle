# EkubCircle — Rotating Savings & Credit Associations Digital Ledger

**QIYAS Full-Stack Development Hackathon 2026 — Challenge 3: EkubCircle**

EkubCircle is an audit-compliant, robust digital ledger for Ethiopian Rotating Savings and Credit Associations (Ekub / ዕቁብ). The backend is architected in ASP.NET Core with Entity Framework Core, SQLite, and an **Onion (Clean) Architecture** leveraging **MediatR (CQRS)** and **AutoMapper**.

---

## 🏛️ Onion Architecture & Directory Tree

The backend is strictly divided into concentric architectural layers where dependencies point only inwards:

```
backend/EkubCircle.API/
├── Domain/                                 # Innermost Core Domain Layer
│   ├── Entities/                           # Domain Entities
│   │   ├── User.cs                         # Registered accounts (Admin / User)
│   │   ├── Circle.cs                       # Savings circle aggregate root
│   │   ├── CircleMember.cs                 # Membership with deterministic order & receipt status
│   │   ├── Round.cs                        # Pre-scheduled deterministic rounds
│   │   └── Payment.cs                      # Contribution & Payout ledger records
│   └── Exceptions/                         # Pure domain exceptions
│       └── DomainException.cs
├── Application/                            # Core Application / Use Cases Layer
│   ├── Commands/                           # CQRS State-Mutating Commands (MediatR IRequest)
│   │   ├── Auth/                           # RegisterUserCommand, LoginUserCommand
│   │   ├── Circles/                        # CreateCircleCommand, AddMemberCommand, RemoveMemberCommand, StartCircleCommand
│   │   ├── Payments/                       # RecordPaymentCommand (anti-duplicate)
│   │   └── Rounds/                         # ExecutePayoutCommand (100% contribution gate)
│   ├── Queries/                            # CQRS Read Queries (MediatR IRequest)
│   │   ├── Auth/                           # GetCurrentUserQuery
│   │   ├── Circles/                        # GetUserCirclesQuery, GetCircleByIdQuery
│   │   ├── Payments/                       # GetPaymentsQuery
│   │   └── Rounds/                         # GetCurrentRoundQuery, GetCircleRoundsQuery
│   ├── Common/                             # Shared mappings and interfaces
│   │   ├── Interfaces/                     # ITokenService
│   │   └── Mappings/                       # AutoMapper MappingProfile
│   └── DTOs/                               # Data Transfer Objects
│       ├── Auth/                           # Register, Login, User, AuthResponse
│       ├── Circles/                        # CreateCircle, AddMember, CircleDto, CircleDetailDto
│       ├── Payments/                       # RecordPayment, PaymentDto
│       └── Rounds/                         # CurrentRoundDto, PayoutResultDto, RoundSummaryDto
├── Infrastructure/                         # Outermost Infrastructure Layer
│   ├── Persistence/                        # Data access implementation
│   │   ├── EkubDbContext.cs                # EF Core context with composite unique constraints
│   │   ├── EkubDbContextFactory.cs         # Design-time factory for EF Core migrations
│   │   └── DbInitializer.cs                # Deterministic seed data generator
│   └── Services/                           # Concrete infrastructure services
│       └── JwtTokenService.cs              # HMAC-SHA256 JWT generator with Claims
└── Controllers/                            # Presentation Layer (Thin Web API Controllers)
    ├── AuthController.cs                   # Injects ONLY ISender and IMapper
    ├── CirclesController.cs                # Injects ONLY ISender and IMapper
    ├── RoundsController.cs                 # Injects ONLY ISender and IMapper
    └── PaymentsController.cs               # Injects ONLY ISender and IMapper
```

> **Design Pattern Note**: All controllers adhere to Clean Architecture standards by injecting **only `ISender` (MediatR)** and **`IMapper` (AutoMapper)**. No controllers depend directly on service implementations.

---

## 🔒 Server-Side Hard Rules Enforced

The API acts as the single source of truth and enforces strict business invariants, returning `HTTP 400 Bad Request` or `HTTP 403 Forbidden` on violations:

1. **Member List Lock**: Circle members can only be added while the circle status is `Forming`. Once `Start` is invoked, the circle becomes `Active` and the roster is permanently locked.
2. **Deterministic Payout Order**: At start, sequential `MemberOrder` (1..$N$) is deterministically locked and pre-assigned to all $N$ rounds. The organizer cannot manually type an arbitrary receiver.
3. **100% Member Contribution Gate**: A pot payout is rejected by the server unless 100% of the circle's members have paid their contribution for that round.
4. **Single Pot Receipt Rule**: A member can receive the pot at most once (`HasReceived` flag). Subsequent attempts to pay out to a prior recipient are rejected.
5. **Ongoing Payment Obligation**: Prior pot recipients remain active members and must contribute in every subsequent round.
6. **Anti-Duplicate Contribution**: Duplicate normal payments by the same member for the same round are strictly prevented at both the application level and via composite database indices: `UNIQUE(RoundId, MemberId, PaymentType)`.
7. **Round Progression**: Paying out round $K$ automatically transitions round $K+1$ to `Open`. When the final round is paid, the circle status transitions to `Completed`.

---

## 👥 Seeded Demo Accounts (for Judges & Testing)

| Role | Email | Password | Details |
|---|---|---|---|
| **System Admin** | `admin@hackathon.local` | `Admin123!` | System-wide administrator |
| **Organizer** | `organizer@ekub.local` | `Ekub123!` | Abebe Bikila (Circle Organizer) |
| **Member 1** | `member1@ekub.local` | `Ekub123!` | Hana Girma |
| **Member 2** | `member2@ekub.local` | `Ekub123!` | Dawit Tadesse |
| **Member 3** | `member3@ekub.local` | `Ekub123!` | Meron Bekele |
| **Member 4** | `member4@ekub.local` | `Ekub123!` | Selam Haile |

---

## 🚀 Running the Backend

### Prerequisites
- [.NET 9 / .NET 10 SDK](https://dotnet.microsoft.com/)

### 1. Start the API Server
```powershell
cd backend/EkubCircle.API
dotnet run --urls=http://localhost:5000
```
- **Swagger UI**: Accessible at `http://localhost:5000/` or `http://localhost:5000/swagger`
- **CORS**: Configured with `AllowAll` for local Angular dev servers.

### 2. Run Automated Verification Tests
We provide a comprehensive PowerShell test suite that automatically tests authentication, circle formation, member locking, round pot calculation, negative test gates (premature payout rejection, duplicate payment rejection), and round completion:
```powershell
# In a separate terminal while API is running:
powershell -ExecutionPolicy Bypass -File backend/test-api.ps1
```

---

## 📡 API Endpoints Summary

### Authentication (`/api/auth`)
- `POST /api/auth/register` — Register a new user account
- `POST /api/auth/login` — Log in and receive a JWT Bearer token
- `GET /api/auth/me` — Retrieve current authenticated user profile

### Circles (`/api/circles`)
- `POST /api/circles` — Create a new circle (creator is auto-assigned as Organizer)
- `GET /api/circles` — List user's circles (supports `?status=forming|active|completed`)
- `GET /api/circles/{id}` — Get circle details, members list, and round status
- `POST /api/circles/{id}/members` — Add member by email (Organizer only, Forming only)
- `DELETE /api/circles/{id}/members/{memberId}` — Remove member (Organizer only, Forming only)
- `POST /api/circles/{id}/start` — Lock member list, assign deterministic payout order, generate rounds

### Rounds (`/api/rounds`)
- `GET /api/rounds/current?circleId={id}` — Current open round details, receiver info, member checklist, pot calculation
- `GET /api/rounds?circleId={id}` — List all rounds and payout statuses
- `POST /api/rounds/{roundId}/payout` — Execute pot payout (enforces 100% payment gate & single receipt rule)

### Payments (`/api/payments`)
- `POST /api/payments` — Record member contribution (prevents duplicate payment)
- `GET /api/payments?circleId={id}&roundId={id}` — Audit trail of payments