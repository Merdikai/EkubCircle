EkubCircle — Schema + Wireframes + Team Roles

This folder contains the architecture/design package for the EkubCircle hackathon project.

Files

01_UPDATED_SCHEMA.md

02_UPDATED_WIREFRAMES.md

03_UPDATED_TEAM_ROLES.md

Core Stack

Angular 16+

ASP.NET Core Web API

Entity Framework Core

Relational database

Checkpoint 1 Core Schema

Users
Circles
CircleMembers
Rounds

Final Supporting Tables

Payments
JoinRequests
Notifications

Main Rule

The server is the source of truth for Ekub rules.

The frontend can disable invalid actions for good UX, but the API must enforce the rules independently.

MVP

Authentication → Create Circle → Add Members → Start → Record Payments → Payout → Next Round → History.

Optional Features

Join requests, notifications, extra contribution/chance, server-side draw, and late flags should only be implemented after the core MVP is stable.