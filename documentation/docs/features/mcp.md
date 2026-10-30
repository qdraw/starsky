# MCP for AI agents

Starsky can expose your photo library to AI agents (for example Claude Desktop or Claude Code)
through the [Model Context Protocol](https://modelcontextprotocol.io) (MCP). An agent can search
your photos, list folders and read metadata. Optionally it can edit metadata, rename and move
files to the trash, but only with an explicit confirmation step.

The business logic is the same as in the web app: the MCP tools call the same services as the
REST API. Nothing is sent to an external service by Starsky.

## Enable

The endpoint is disabled by default. Set the AppSettings (or environment variables):

```bash
app__McpEnabled=true
```

The endpoint is then available at `/starsky/mcp` (Streamable HTTP, stateless), for example
`http://localhost:4000/starsky/mcp`. It always needs an authenticated user:

- Desktop apps and `NoAccountLocalhost`: no credentials needed from localhost.
- Otherwise use HTTP basic authentication with a Starsky account
  (`Authorization: Basic ...`); there is no need to log in with a cookie first.

Because basic authentication sends the account password, use HTTPS when the server is not on
localhost. The endpoint has its own rate limit (120 requests per minute per client, disabled with
`app__UseRateLimiting=false`).

### Connect Claude Code

```bash
claude mcp add --transport http starsky http://localhost:4000/starsky/mcp
```

With basic authentication add `--header "Authorization: Basic <base64 of user:password>"`.

## Tools

Read-only tools (always available when `McpEnabled` is true):

| Tool             | What it does                                                         |
|------------------|----------------------------------------------------------------------|
| `search_photos`  | Search with the normal Starsky search syntax, one page per call      |
| `list_folder`    | Files and subfolders of a folder (at most 100 items per call)        |
| `get_photo_info` | Tags, title, description, color class, date and location of files    |
| `suggest_tags`   | Tags and keywords in use that start with the given text              |

Results are compact on purpose (no thumbnails or binary data), so they fit in an agent's context.

## Tools that change files

Disabled by default. Enable with:

```bash
app__McpAllowWrite=true
```

| Tool            | What it does                                                           |
|-----------------|------------------------------------------------------------------------|
| `move_to_trash` | Move files to the trash. There is no tool that deletes permanently.    |
| `update_metadata` | Change tags, title, description or color class (optionally append). Empty fields are not changed |
| `rename`        | Rename or move one file or folder, it never overwrites an existing one |

Every tool that changes files goes through the same guard rails:

1. **Off by default**, the tools are not even listed unless `McpAllowWrite` is true.
2. **Two steps**: the first call changes nothing and returns a preview and a `confirmToken`.
   The agent must call again with the same arguments and the token. The token works once, expires
   after 5 minutes and only for the same account, tool and arguments.
3. **Limit per call**: at most `McpMaxItemsPerCall` files _(default 25)_.
4. **Path checks**: only paths inside the storage folder; `..`, the root folder and read-only
   folders (`ReadOnlyFolders`) are refused.
5. **Audit log**: every preview, confirmation, refusal and result is written to the application
   log with the account id, tool and paths (search for `[Mcp]`).

Tool annotations (`readOnlyHint`, `destructiveHint`) are set, so clients can ask the user before
running a tool.

## Settings

1. `McpEnabled` - expose the MCP endpoint _(default false)_
2. `McpAllowWrite` - register the tools that change files _(default false)_
3. `McpMaxItemsPerCall` - maximum number of files per write call _(default 25)_
