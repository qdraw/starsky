# Account management Feature

## UserManager
To get login, change password, account removal etc.

## Multi-tenant session model

Account management now supports path-based tenancy and a DB-backed session cookie.

- cookie name: `.Starsky.Session`
- tenant context from `/{tenant}/...`
- tenant activation stored in `WebSessionTenant`

Full documentation:

- [Multi-tenant Session Cookie and Tenant Routing](../../starsky/docs/multi-tenant-session-cookie.md)