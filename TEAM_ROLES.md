EkubCircle — Updated Team Roles

Team Size: 5

The team should split work so that every member owns a meaningful part of the application and can explain it during judging. The hackathon specifically evaluates teamwork/code defense and can ask a non-presenting member to explain an endpoint, database query, or TypeScript function. fileciteturn0file0L127-L130

1. Team Lead / Product Owner

Owns

overall scope

architecture coordination

requirements

integration

final demo

README coordination

Checkpoint responsibility

Explain problem and MVP

Explain architecture

Present the 4-table core schema

Coordinate wireframes

Track decisions

During implementation

Review frontend/backend/database contracts

Keep scope under control

Coordinate Git merges

Make sure no business rule is implemented only in the UI

Coordinate final integration

Must understand

all major API endpoints

circle state machine

round state machine

fixed receiver logic

payout rules

2. Frontend Developer — Angular

Owns

Angular application

components

routing

forms

validation

loading states

error messages

dashboard UI

Main screens

Login
Create Circle
Manage Members
Join Requests
Member Home
Organizer Dashboard
Current Round
Payment Modal
Payout Confirmation
Round History
Notifications
Completed Circle

Main services

AuthService
CircleService
MemberService
RoundService
PaymentService
JoinRequestService
NotificationService

Must not do

Do not make the frontend the source of truth for business rules.

Example:

Frontend disables Pay Out
        +
Backend also rejects invalid Pay Out

Both are required.

3. Backend Developer — ASP.NET Core Web API

Owns

API controllers

services

DTOs

validation

authentication/authorization

server-side Ekub rules

Suggested controllers

AuthController
CirclesController
MembersController
JoinRequestsController
RoundsController
PaymentsController
PayoutsController
NotificationsController

Suggested services

AuthService
CircleService
MembershipService
JoinRequestService
RoundService
PaymentService
PayoutService
NotificationService

Critical server rules

The API must enforce:

Members can only be added while the circle is Forming.

Start locks the member list.

Start creates/fixes the payout order.

Receiver is selected from fixed order.

Organizer cannot type an arbitrary receiver.

Every member must pay before payout.

A member may receive at most once.

A previous receiver still appears in future payment lists.

Duplicate normal payment is rejected.

A paid-out round cannot be paid out again.

Final payout completes the circle.

The hackathon strongly rewards clean separation between controllers, services, and models. fileciteturn0file0L123-L123

4. Database + Full-Stack Developer

Owns

EF Core entities

DbContext

migrations

relationships

constraints

seed data

database/API integration

Checkpoint schema

Users
Circles
CircleMembers
Rounds

Final supporting schema

Users
Circles
CircleMembers
Rounds
Payments
JoinRequests
Notifications

Important design decisions

Do not create:

Admins

Use:

Users.Role

for system roles.

Use:

CircleMembers.RoleInCircle

for:

Organizer
Member

Recommended constraints

UNIQUE(CircleId, UserId)
UNIQUE(CircleId, RoundNumber)

For required payment uniqueness, use an appropriate database constraint/index so duplicate normal payments cannot be inserted accidentally.

5. QA + Testing Developer

Owns

test plan

acceptance testing

negative cases

regression testing

demo smoke test

bug tracking

Critical test cases

Authentication

valid login

invalid login

unauthorized API request

Circle

create circle

invalid contribution

add member

duplicate member

start circle

add member after Start

Join Requests

send request

accept

reject

duplicate pending request

request after Start

Payments

record normal payment

duplicate normal payment

wrong member

invalid amount

payment on closed round

correct pot

Payout

payout before all paid → 400

payout after all paid → success

payout twice → reject

receiver already received → reject

arbitrary receiver → reject

Round progression

open next round

next receiver is correct

previous receiver still pays

final member receives

circle becomes Completed

UI reliability

empty forms

loading states

server errors

friendly error messages

responsive layout

6. Responsibility Matrix

Area

Lead

Frontend

Backend

DB/Full-stack

QA

Requirements

A/R

C

C

C

C

Architecture

A/R

C

R

R

C

Wireframes

A

R

C

C

C

Angular

C

R

C

C

T

API

C

C

R

C

T

Business Rules

A

C

R

C

T

Database

C

I

C

R

T

Payments

C

R

R

R

T

Join Requests

C

R

R

R

T

Notifications

C

R

R

R

T

Testing

A

C

C

C

R

Git Integration

R

C

C

C

C

Final Demo

R

C

C

C

C

Legend:

R = Responsible
A = Accountable
C = Consulted
I = Informed
T = Test/Verify

7. Git Responsibilities

The hackathon evaluates Git history, descriptive commits, and contribution from team members throughout the competition window. fileciteturn0file0L125-L125

Suggested branches:

main

feature/angular-ui
feature/api
feature/database
feature/testing
feature/integration

Example commits

feat: add circle creation endpoint
feat: create member dashboard
feat: implement fixed payout order
feat: add payment recording
feat: validate payout eligibility
feat: add join request flow
test: reject duplicate payments
fix: prevent payout before all members pay
docs: update README

Do not leave all commits to one person.

8. 10-Hour Work Breakdown

Phase 1 — Understand

Everyone

read challenge

confirm MVP

identify hard rules

Phase 2 — Design

Lead + Frontend + Backend + DB + QA

schema

wireframes

API contract

test cases

Phase 3 — Database

DB/Full-stack + Backend

EF entities

relationships

migration

seed data

Phase 4 — Backend

Backend

auth

circles

members

rounds

payments

payout

Phase 5 — Frontend

Frontend

login

dashboard

member screens

organizer screens

history

Phase 6 — Integration

Frontend + Backend + DB

connect Angular to API

connect API to EF Core

test real data flow

Phase 7 — QA

QA leads

negative tests

payout rules

duplicate payment

round progression

error handling

Phase 8 — Demo

Everyone

run clean seed

verify localhost launch

verify complete journey

prepare technical explanations

The final submission requires a working Git repository, README, verified localhost launch steps, and a clean database seed script. fileciteturn0file0L129-L130

9. Demo Ownership

Person 1 — Lead

Problem → solution → architecture

Person 2 — Frontend

Show UI and member experience

Person 3 — Backend

Explain API/business-rule enforcement

Person 4 — Database

Explain EF Core/schema/query flow

Person 5 — QA

Explain test cases and failure prevention

Everyone should still understand the complete user journey.

10. Scope Priority

MUST HAVE

Authentication

Create circle

Add members

Start circle

Fixed payout order

Record payments

Show pot

Show paid/unpaid

Enforce payout rules

Open next round

Round history

NICE TO HAVE

Join requests

Notifications

Extra contribution

Server-side draw

Late flags/fines

If time becomes short, remove optional features rather than risking the working MVP.

The rubric prioritizes functional completeness, and the final demo is expected to show the working software journey rather than screenshots. fileciteturn0file0L123-L128