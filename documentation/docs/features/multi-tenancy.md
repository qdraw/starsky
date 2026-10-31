# Multi-tenancy

One Starsky installation can serve several separate photo libraries, called **tenants**. Each tenant has its
own storage folder, index, thumbnails, settings, notifications and real-time messages. A user has one login
and can be a member of several tenants, with a different role in each.

The **default tenant** is the installation as you know it: the existing storage folder, the existing urls
(`/starsky/...`) and the desktop apps. When you do not use tenants, nothing changes.

## Urls

| Tenant | Url |
|---|---|
| default | `/starsky/` |
| `acme` | `/starsky/t/acme/` |

Everything of a tenant lives below its prefix: the web app, the API (`/starsky/t/acme/api/...`), the
real-time socket (`/starsky/t/acme/realtime`) and the MCP endpoint (`/starsky/t/acme/mcp`).
A tenant id is lowercase letters, digits and single dashes (`acme`, `family-2`).
An unknown, disabled or invalid tenant answers `404`.

## Roles and rights

| Who | May |
|---|---|
| Member (`User`) | use the photos of the tenant |
| `Administrator` of a tenant | do the above, change the settings of the tenant, add, change and remove members |
| Platform administrator | create, disable and delete tenants. Gives **no** access to the photos of a tenant |

* The first user of an installation is the platform administrator. When you upgrade, the existing
  administrators become platform administrators.
* A tenant always keeps at least one administrator.
* A user that is no member of a tenant gets `403` on its API. The web app then shows the tenants
  the user can choose from.
* Changing process-wide settings (`POST /api/env`) needs a platform administrator as soon as there is
  more than one tenant. They are shared by all tenants.
* A membership change works within 30 seconds at most (the rights are cached for that time).

## Switching tenants

A user with more than one tenant sees "Switch to ..." entries in the menu. Switching is a full page load,
so nothing of the other tenant stays in the page. After login, without a target page, the user goes to the
tenant that was used last (when still a member), or straight to the only tenant.

## API

| Request | Who |
|---|---|
| `GET /api/tenants/mine` | any user: own tenants, last used tenant and where a login lands |
| `POST /api/tenants/last-used` | any member: remember this tenant for the next login |
| `GET/POST /api/tenants/members`, `PUT/DELETE /api/tenants/members/{userId}` | administrator of the tenant in the url |
| `GET/POST /api/tenants`, `POST /api/tenants/{id}/disable`, `.../enable`, `DELETE /api/tenants/{id}` | platform administrator |

Deleting a tenant only marks it as deleting (nobody can use it, the background services skip it).
Files and database rows are not removed.

## Folders

* The default tenant keeps its `StorageFolder`, `ThumbnailTempFolder` and `TempFolder`.
* Other tenants get `TenantsRoot/{id}/storage`, `.../thumbnails` and `.../temp` (setting `TenantsRoot`,
  default: a `tenants` folder next to the application), or explicit absolute folders.
* A tenant folder may not be inside the folder of another tenant (it would be indexed by that tenant),
  and tenant folders may not overlap. The application refuses to start, or to create the tenant, when they do.

## Command line tools

Every command line tool takes `--tenant acme` and then works on that tenant. Without it, the default tenant.

## Database

Tested with SQLite and MariaDB 12.3 (fresh database, upgrade of a database with users and photos, the whole
end-to-end suite). The migration `MultiTenancy` makes the existing administrators platform administrators and
puts all existing rows in the default tenant. The key columns use `varchar(63)`: MySQL does not accept `TEXT` in a
key. The migrations of Starsky as a whole do not run on MySQL 8 (an older migration fails on `FileHash`); the
documented databases are SQLite and MariaDB.

## Accounts

* Registering never adds an *existing* account to a tenant. A new account joins the default tenant, or the tenant of
  the url when an administrator of that tenant creates it; anybody else (also open registration) creates an account
  without a membership, and a tenant administrator adds it.
* When registering is closed, only administrators create accounts.

## Deleting a tenant

`DELETE /api/tenants/{id}` only marks the tenant as deleting (nobody can use it, background work skips it, open
sockets are closed). The data stays until a platform administrator purges it:
`POST /api/tenants/{id}/purge?confirm={id}` (irreversible, only for a tenant that is deleting) removes its rows
(index, import history, notifications, settings, thumbnails, queue, memberships) and, when the folders are in the
`TenantsRoot` layout, the folders. Explicit folders that an administrator pointed to are not touched. The id is not
free again.

## Good to know

* Background work (sync, thumbnails, the disk watcher, queued jobs) runs per tenant. A job runs as the tenant
  that queued it.
* Search suggestions, metadata and similar caches are kept per tenant.
* Exports, publish zips, upload and import staging and previews use the temp folder of the tenant.
* Memberships are checked for `/api/*`, `/realtime` and `/mcp`; sockets are closed when the access ends.
* Process-wide settings (`/api/env`) and the cache overview are for platform administrators once there is more
  than one tenant; the host folders are hidden from other users.
* Every log line of a request has the scope `TenantId`.
* The command line tools with a full `--path`, and the geo and publish command line tools, do not support
  `--tenant` (use `--subpath`).
* Cloud import and the demo data cleanup only work on the default tenant for now.

See [the security audit](../developer-guide/security/multi-tenancy-security-audit.md) for what was checked.
