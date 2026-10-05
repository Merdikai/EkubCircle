EkubCircle — Updated Schema

1. Important Checkpoint Decision

For the Checkpoint 1 architecture/schema presentation, keep the database model to the required 4 core tables:

Users

Circles

CircleMembers

Rounds

Supporting features such as individual payment records, join requests, and persistent notifications can be normalized into additional tables during implementation.

This keeps the checkpoint simple while preserving a realistic final design.

2. Core ER Diagram

erDiagram
    USERS ||--o{ CIRCLES : creates
    USERS ||--o{ CIRCLE_MEMBERS : joins
    CIRCLES ||--o{ CIRCLE_MEMBERS : contains
    CIRCLES ||--o{ ROUNDS : has
    CIRCLE_MEMBERS ||--o{ ROUNDS : receives

    USERS {
        int Id PK
        string FullName
        string Email UK
        string PasswordHash
        string Role
        datetime CreatedAt
    }

    CIRCLES {
        int Id PK
        string Name
        decimal ContributionAmount
        string MeetingLabel
        string Status
        int CreatedByUserId FK
        datetime CreatedAt
        datetime StartedAt
        datetime CompletedAt
    }

    CIRCLE_MEMBERS {
        int Id PK
        int CircleId FK
        int UserId FK
        int MemberOrder
        string RoleInCircle
        bool HasReceived
        datetime JoinedAt
    }

    ROUNDS {
        int Id PK
        int CircleId FK
        int RoundNumber
        int ReceiverMemberId FK
        string Status
        decimal PotAmount
        datetime PaidOutAt
    }

3. Users

Field

Type

Key

Description

Id

int

PK

Unique user ID

FullName

string



User's name

Email

string

UK

Login email

PasswordHash

string



Hashed password

Role

string



System-level role

CreatedAt

datetime



Account creation

Admin decision

Do not create a separate Admins table.

Use:

Users.Role

for system-level roles such as:

User
Admin

If your application does not need system administrators, simply keep the role field ready but use User for normal accounts.

Circle-specific authority belongs elsewhere:

CircleMembers.RoleInCircle

with:

Organizer
Member

The organizer is also a member.

4. Circles

Field

Type

Key

Description

Id

int

PK

Circle ID

Name

string



Circle name

ContributionAmount

decimal



Required contribution per round

MeetingLabel

string



Weekly / Monthly label

Status

string



Forming / Active / Completed

CreatedByUserId

int

FK

Creator

CreatedAt

datetime



Creation timestamp

StartedAt

datetime



Start timestamp

CompletedAt

datetime



Completion timestamp

Circle lifecycle

FORMING
   ↓
START
   ↓
ACTIVE
   ↓
last member receives
   ↓
COMPLETED

Rules

Members may be added while Forming.

Start Circle locks the member list.

MeetingLabel is only a label, not a scheduling system.

A circle is completed after every member has received exactly once.

5. CircleMembers

This is the membership/relationship table between Users and Circles.

Field

Type

Key

Description

Id

int

PK

Membership ID

CircleId

int

FK

Circle

UserId

int

FK

User

MemberOrder

int



Fixed payout order

RoleInCircle

string



Organizer / Member

HasReceived

bool



Whether member already received

JoinedAt

datetime



Membership timestamp

Recommended constraint

UNIQUE(CircleId, UserId)

A user cannot be added to the same circle twice.

Critical design decision

At Start Circle:

Member list is locked.

Members are assigned a fixed MemberOrder.

Every round uses that order.

The organizer cannot type a receiver manually.

Example:

1 → Hana
2 → Dawit
3 → Meron
4 → Selam

Therefore:

Round 1 → Hana
Round 2 → Dawit
Round 3 → Meron
Round 4 → Selam

6. Rounds

Field

Type

Key

Description

Id

int

PK

Round ID

CircleId

int

FK

Circle

RoundNumber

int



1, 2, 3...

ReceiverMemberId

int

FK

Fixed receiver

Status

string



Open / PaidOut

PotAmount

decimal



Recorded pot

PaidOutAt

datetime



Payout timestamp

Round lifecycle

OPEN
  ↓
all members paid
  ↓
PAY OUT
  ↓
PAID OUT

Server rules

The API must reject:

payout before every member has paid

payout of an already-paid-out round

payout to a member who already received

manually supplied arbitrary receiver

duplicate payment

A receiver continues paying in later rounds.

7. Final Implementation Supporting Tables

These are useful for the actual application, but do not make the Checkpoint 1 diagram unnecessarily large.

Payments

Recommended fields:

Field

Description

Id

Payment ID

RoundId

Round

MemberId

Member

Amount

Amount recorded

PaymentType

Normal / Extra

ChanceCount

Optional

PaidAt

Time

RecordedByUserId

User who recorded it

Why Payments is useful

It lets the application answer:

Who paid this round?

Who is unpaid?

What amount was recorded?

Was this a normal or extra contribution?

When was it recorded?

Who recorded it?

The pot can be calculated from valid payments.

8. JoinRequests

If the organizer can send a request to a person who is not yet a member, create:

JoinRequests

Field

Description

Id

Request ID

CircleId

Target circle

RequestedUserId

Person invited

RequestedByUserId

Organizer

Status

Pending / Accepted / Rejected

Message

Optional message

CreatedAt

Sent time

RespondedAt

Response time

Flow

Organizer
   ↓
Send Join Request
   ↓
Pending
   ↓
User Accepts
   ↓
Create CircleMember

After Start Circle, new membership requests should be blocked because the member list is locked.

9. Notifications

If notifications must remain as an in-app history, use one:

Notifications

Field

Description

Id

Notification ID

UserId

Recipient

Type

Notification type

Title

Short title

Message

Content

RelatedEntityId

Optional related record

IsRead

Read status

CreatedAt

Creation time

ReadAt

Read time

Possible types:

JoinRequest

JoinRequestAccepted

JoinRequestRejected

CircleStarted

RoundOpened

PaymentRecorded

PayoutCompleted

One notification table is enough.

10. Extra Chance / Extra Contribution

A separate table is not automatically required.

Start with:

Payments.PaymentType = Normal | Extra
Payments.ChanceCount

Only create a separate ExtraContributions table if the rules become significantly more complex.

Important product decision

The challenge's MVP does not define an extra-chance lottery.

Before implementing it, the team must decide:

Does extra payment increase the pot?

Does it increase winning probability?

How many extras are allowed?

Is the normal payment required first?

Can extras be added every round?

Is this an actual draw or simply an extra recorded contribution?

Do not add undefined lottery behavior to the MVP.

11. Final Database Picture

                 ┌──────────────┐
                 │    USERS     │
                 └──────┬───────┘
                        │
              ┌─────────┴─────────┐
              ↓                   ↓
      ┌──────────────┐    ┌───────────────┐
      │   CIRCLES    │───→│ CIRCLEMEMBERS │
      └──────┬───────┘    └───────┬───────┘
             │                    │
             ↓                    ↓
      ┌──────────────┐      receiver/order
      │    ROUNDS    │
      └──────────────┘

Final implementation additionally:
Payments
JoinRequests
Notifications

12. Recommended Presentation to Judges

Say:

"We designed four core entities for the circle lifecycle: Users, Circles, CircleMembers, and Rounds. CircleMembers holds the fixed payout order and circle-specific organizer role. The final implementation can normalize payment records, join requests, and persistent notifications into supporting tables."

This shows that the team understands both the required architecture and database normalization.