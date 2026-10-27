# FitCore.Api docs

Topic folders under `docs/`. The README at the repo root links here for the portfolio story.

## Evolution

How the domain model grows in phases. Phase write-ups hang off this overview. BI will be phase 3 here later.

- [Overview](evolution/README.md)
- [Phase 01 — Identities](evolution/phase-01-identities.md)
- [Phase 02 — Operations](evolution/phase-02-operations.md)

## Domain

Current business rules that must stay true.

- [States](domain/states.md)

## Concurrency

How those rules survive concurrent writers. Calendar locks live here.

- [Row lock ordering](concurrency/row-lock-ordering.md)
- [Invariants](concurrency/Invariants.md)
- [Architecture](concurrency/Architecture.md)

## Architecture

Cross-cutting mechanics that are neither a phase nor the state machine.

- [Guard rails](architecture/GuardRails.md)
- [Active tenant on the request](architecture/request-active-tenant.md)
