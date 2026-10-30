---
name: test-mcp-local
description: Test the Starsky MCP server (/starsky/mcp) locally on throwaway data - unit tests, a live end-to-end script over HTTP, and an optional running instance to connect Claude Code to. Use when asked to test, try or verify the MCP server or its tools (search_photos, list_folder, get_photo_info, suggest_tags, move_to_trash, update_metadata, rename), or after changing starsky.project.mcp.
---

# Test the MCP server locally

Never point this at a real photo library: the write tools move, rename and edit files.
Everything below uses throwaway storage under `/tmp` with two fixture JPEGs.

## 1. Unit and architecture tests

```bash
cd "$(git rev-parse --show-toplevel)/starsky" && dotnet build starskytest/starskytest.csproj \
  && dotnet run --project starskytest/starskytest.csproj --no-build --no-launch-profile -- \
  --filter "FullyQualifiedName~starsky.project.mcp|FullyQualifiedName~Architecture"
```

`--no-launch-profile` is needed: the launch profile passes `--urls`, which the test host rejects.
The architecture test fails if the MCP project references another feature project (Helix rules),
which is why the library lives in `starsky.project.mcp`.

## 2. Live end-to-end script

Build the web app, then run the script (about 1-2 minutes). It starts the app itself on ports
4981-4983, runs ~28 checks and cleans up.

```bash
cd "$(git rev-parse --show-toplevel)/starsky" && dotnet build starsky/starsky.csproj \
  && python3 -I .claude/skills/test-mcp-local/mcp_livetest.py
```

It covers three modes:

- **disabled** (`McpEnabled` off): `/mcp` is not served.
- **read-only** (`McpEnabled` only): initialize works, only the 4 read tools are listed, a write
  tool call fails, files are untouched.
- **full write** (`McpAllowWrite`, `McpMaxItemsPerCall=3`): list/search/info, `update_metadata`
  (preview changes nothing, single-use token, tag visible afterwards), the limit per call, path
  traversal, `rename` (token bound to the target, refuses to overwrite) and the `[Mcp]` audit log.

Exit code is 0 only when every check passes.

## 3. Run the app to try it with a real client

```bash
cd "$(git rev-parse --show-toplevel)/starsky" && python3 -I .claude/skills/test-mcp-local/mcp_livetest.py --serve
```

Starts the app on `http://localhost:4000/starsky/` with MCP on, writes enabled and
`NoAccountLocalhost` (no login from localhost). Then, in a **new** Claude Code session
(MCP servers load at session start):

```bash
claude mcp add --transport http starsky http://localhost:4000/starsky/mcp
```

Try "list the folder /trip", "search for white", "tag /trip/white.jpg with holiday". Clean up:

```bash
claude mcp remove starsky
```

Stop the app with Ctrl+C (or `kill <pid>` of the printed pid; do not `pkill` by name).

## Things that look like bugs but are not

- **File added on disk while the app runs**: tools say "0 file(s)" until the folder is synced
  (`POST /api/synchronize?f=/trip`). The index is filled at startup. REST behaves the same.
- **Trash**: `move_to_trash` uses the macOS system Trash, in a background job, so the file
  disappears a moment after the call. The result item status is `NotFoundSourceMissing` and the
  message counts the files that existed before. Look in Finder; `~/.Trash` is not readable from
  a sandboxed shell.
- **`suggest_tags`** can return `[]` for a just-added tag: `/api/suggest` does the same.
- **Unauthenticated request without `NoAccountLocalhost`**: 302 to the login page, not 401.
- **Python in tests**: use `HTTPCookieProcessor` (cookie from the first call) when mixing MCP
  with REST calls; REST answers 401 without it.
- **Fakes in unit tests**: use `FakeIHttpContextAccessor`, not `HttpContextAccessor` (it shares an
  AsyncLocal, so two instances in one test overwrite each other).
