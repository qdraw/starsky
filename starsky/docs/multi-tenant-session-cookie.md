# Multi-tenant Session Cookie and Tenant Routing

This document describes the multi-tenant session model in Starsky where one browser cookie can
activate multiple tenant contexts concurrently.

## Summary

- Tenant-scoped routes are path-prefixed: `/{tenant}/...`.
- Session cookie name is `.Starsky.Session`.
- Cookie payload is one opaque session id (no tenant list in cookie).
- Session and tenant activation state are stored in the database.
- Reserved global routes stay outside tenant prefix (for example `/-/tenants`,
  `/api/tenants/mine`, health endpoints).

## Data Model

Core entities:

- `Tenant`
- `TenantUser`
- `WebSession`
- `WebSessionTenant`
- `User.IsGlobalAdmin`

Important constraints:

- `Tenant.Slug` unique
- `TenantUser(TenantId,UserId)` unique
- `WebSession.SessionId` unique
- `WebSessionTenant(WebSessionId,TenantId)` unique

## Tenant Slug Policy

Slug format:

- lowercase letters, digits, and hyphen
- length: 3-50
- must start and end with alphanumeric

Regex used by validator:

`^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$`

## Request/Auth Flow

1. Tenant path middleware extracts `{tenant}` from `/{tenant}/...` and rewrites the request path
   to existing route patterns.
2. Session auth middleware reads `.Starsky.Session` and validates:
   - valid and non-revoked `WebSession`
   - tenant exists and is enabled
   - `WebSessionTenant` activation exists
   - `TenantUser` membership exists
3. Claims are populated for tenant-scoped calls.

Claims added:

- NameIdentifier
- tenant id
- tenant slug
- global admin flag
- tenant role

## HTTP Status Mapping

Tenant-auth middleware rules:

- missing or invalid session for authorized API: `401 Unauthorized`
- valid session but disabled tenant/non-member/missing activation: `403 Forbidden`

## Route Scope Rules

Tenant-required areas:

- API routes (`/api/...`) via tenant prefix
- account routes (`/account/...`) via tenant prefix
- UI routes like search/trash/import/preferences

Reserved global routes:

- `/-/tenants`
- `/api/tenants/mine`
- selected platform/global health routes

Non-tenant requests to tenant-required routes should return `404`.

## Account and Session Behavior

Login (`/{tenant}/api/account/login`):

- validates credentials
- creates first tenant only when no tenant exists yet
- enforces membership for existing tenants
- first member in a tenant becomes Tenant Admin
- creates/refreshes `WebSession`
- activates tenant in `WebSessionTenant`

Logout (`/{tenant}/api/account/logout`):

- removes only the current tenant activation mapping
- does not revoke the whole browser session

Global logout-all (`/api/account/logout-all`):

- revokes the entire `WebSession`
- clears `.Starsky.Session` cookie

Bootstrap rules:

- first-ever user becomes global admin
- global admin is not auto-member of all tenants
- unknown second tenant is not auto-created after first tenant exists

## Global Tenant Chooser

Global endpoint:

- `GET /api/tenants/mine`

Returns tenant summaries for current session user membership:

- slug
- name
- isEnabled
- tenantRole

UI route:

- `/-/tenants`

Behavior:

- disabled tenants are listed but not navigable
- includes empty-state handling

## Cookie Transition Notes

Migration from old cookie to session cookie:

- old cookie name: `_id`
- new cookie name: `.Starsky.Session`

Expected operational impact:

- existing users will need to log in again after rollout
- session handling is now centralized in DB tables

## Verification Checklist

- run targeted backend tests for account/middleware/session behavior
- verify 401 vs 403 matrix for tenant APIs
- verify concurrent tenant activation in one browser session
- verify tenant logout only deactivates one tenant context
- verify reserved global routes remain reachable
- verify non-tenant access to tenant-required routes returns 404
