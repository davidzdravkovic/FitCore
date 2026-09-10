# Phase 01 — Identities data model

Describes in plain words what business rules are applied with the phase one data model.

## Platform Admin

One unique email per admin.

## Platform login token

One unique token represents one admin login.

At most there can be one unused login meaning only one usable token, the last login disables in the service all before login's tokens, the DB enforces it with partial unique index as a depth guard.
UsedAt is nullable since defines the token validation.

Each login belongs to one admin.

## Invitation

One unique token represents one invitation.

There can be only one pending invitation per tenant - email.

Weird window nearly impossible to happen if 2 tenants share the same email, the first tenant invitation is discarded by the second tenant with the same email, in worst case the tenant should ask for invitation token again.
UsedAt is nullable since defines the token validation.

## Members

Needs at least to have some kind of contact: phone or mail.

Makes uniqness on contact and the tenant where the user belongs, the tenant should not have 1 same contact for 2 different members.
Optional on password, the admin creates a member without it and later the member can create the credentials.

## Staff

Can belong to one tenant, it needs to have a unique email for that tenant.
Optional on password, invitation imposes 2 phases of creation. The admin which is a staff and starts with the password, can create staff's that can create/own their credentials.

## Staff Invite

One admin invitation represents only one invite token, the staff must be created before, the tenant that invites the staff must exists. the staff is able to use the last invitation not collecting in the scope of the expired time, the last invitation disables all unused invitations.

## Member Invite

Same as staff for this phase but they are decoupled later another rules will be introduced.