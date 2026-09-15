# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is Starsky?

Starsky is a photo-management platform with a .NET/ASP.NET Core backend, a React frontend, several CLI tools, and native macOS (Swift/AppKit) and Windows (WPF) desktop apps. All business logic lives in the backend; the frontends are display/interaction layers only.

## Repository layout

| Path | What it is |
|---|---|
| `starsky/` | .NET solution (web API + all CLI tools + libraries) |
| `starsky/starsky/` | ASP.NET Core web API (`starsky.sln`) |
| `starsky/starsky/clientapp/` | React front-end (Vite + Jest + Testing Library) |
| `starsky/starskytest/` | MSTest unit tests for the entire .NET solution |
| `starsky/starsky.feature.*` | Feature-scoped .NET libraries |
| `starsky/starsky.foundation.*` | Infrastructure/shared .NET libraries |
| `starsky-tools/` | Node.js helper scripts — **no `package.json` dependencies** (except Cypress for end2end) |
| `mac/` | Native Swift/AppKit macOS app (WKWebView shell around the backend) |
| `windows/` | Windows desktop app |
| `documentation/` | Docusaurus site; output goes to `documentation/docs` |

> **Do not edit files inside `documentation/docs/` directly.**
> Many subdirectories are auto-copied from their source locations by `documentation/scripts/prestart.js` every time the docs site starts.
> Folders containing a `__do_not_edit_this__folder` marker are generated — edit the source files instead.
> The marker file lists the exact source → destination mapping for that folder.
> To change what text appears in a `__do_not_edit_this__folder` file, update the corresponding `writeFile(...)` call in `documentation/scripts/prestart.js`.

## Commands

### Full build (all projects + tests)

```bash
./build.sh          # macOS / Linux
.\build.ps1         # Windows PowerShell
```

### .NET backend

```bash
# Do not infer the working directory from the terminal prompt. Resolve the repository root
# and change to it in the same command before running repository commands.

# Run all .NET tests with Microsoft Testing Platform (MTP)
cd "$(git rev-parse --show-toplevel)/starsky" && dotnet run --project starskytest/starskytest.csproj

# Run a single test class
cd "$(git rev-parse --show-toplevel)/starsky" && dotnet run --project starskytest/starskytest.csproj -- --filter "FullyQualifiedName~ClassName"

# Format all .csproj files
cd "$(git rev-parse --show-toplevel)/starsky" && ./format.sh
```

### Windows desktop app (from the repository root on Windows)

```powershell
# Run all Windows Microsoft Testing Platform (MTP) tests
Set-Location (Join-Path (git rev-parse --show-toplevel) 'windows'); dotnet run --project starsky.Tests/starsky.Tests.csproj
```

### React clientapp (from `starsky/starsky/clientapp/`)

```bash
npm start          # dev server on :3000 (requires API on :4000)
npm test           # interactive watch mode
npm run test:ci    # non-interactive CI run
```

### macOS native app (from `mac/`)

```bash
xcodegen generate   # regenerate .xcodeproj from project.yml (required after adding/removing files)

# Build
xcodebuild build -project starsky.xcodeproj -scheme starsky \
  -configuration Debug -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO

# Test (XCTest)
xcodebuild test -project starsky.xcodeproj -scheme starsky \
  -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO
```

### Version bump

```bash
node starsky-tools/build-tools/app-version-update.js
```

## End-to-end tests (`starsky-tools/end2end/`)

When working on Cypress end-to-end tests, follow the guidelines in
[`documentation/docs/developer-guide/testing/end2end-guidelines.md`](documentation/docs/developer-guide/testing/end2end-guidelines.md).

Key rules at a glance:
- **Poll, don't wait** — use a retry loop to check API state after any write; never `cy.wait(N)` + immediate assertion.
- **Check specific file paths** — never assert on total item counts; other suites leave files behind.
- **Content-based selectors** — prefer `cy.contains(...)` over `.eq(N)`.
- **Enable-guard before clicking** — add `.should("not.be.disabled")` before any modal action button.
- **15 s timeout on modal content** — folder lists and collection members load from the API; the default 4 s is not enough on Windows CI.
- **No `cy.intercept` on GET** — browser cache silently skips the network on Windows; use DOM assertions or direct API polling instead.
- **Intercepts before the click** — set up `cy.intercept` aliases before the action that triggers the request.
- **Assert contenteditable value after blur** — add `.should("have.text", value)` after each field blur when filling multiple fields in sequence.
- **Clear sessionStorage before reload** — `cy.then(() => { sessionStorage.clear(); })` before any reload that verifies a backend write.
- **API-based cleanup** — use a direct `DELETE /starsky/api/delete` request covering all possible file locations; never rely on UI selection for cleanup.

## .NET testing conventions

- Test framework: **MSTest** — use `[TestMethod]` + `[DataRow]` for parameterised tests. `[DataTestMethod]` is deprecated; do not use it.
- **Never use mocks.** Every object that needs a fake has a hand-written `FakeMyObject` or `FakeIMyObject` in `starskytest/FakeCreateAn/` or `starskytest/FakeMocks/`. Use those.
- Tests that touch `ApplicationDbContext` must extend `DatabaseTest` (in `starskytest/DatabaseTest.cs`); this sets up and tears down the in-memory database automatically.
- Tests live in `starsky/starskytest/`, mirroring the source project structure.

## Code conventions

- **Avoid new NuGet/npm dependencies.** Prefer a small in-tree helper over adding a package.
- Frontend: Prettier enforces formatting. Install a Prettier editor plugin; the CI will fail on violations.
- TypeScript: use types everywhere, avoid `any`.
- Comments should explain *why*, not *what*. Avoid restating what the code already says.
- Documentation for users goes in `documentation/docs/`.
- **Do not create git commits.** Leave committing to the user.
- **Do not use explicit system binary paths in tests** (e.g. `/usr/bin/true`, `/usr/bin/xattr`). Use the class's default constructor or a `FakeStarskyBin`-style helper; the existing `TestableBackendService` already provides the right defaults for process tests.

## macOS app architecture (mac/)

The native app (`mac/`) is a thin AppKit shell around a `WKWebView`. It operates in two modes:

- **Local mode (default):** finds a free TCP port, launches the bundled `starsky` ASP.NET Core binary as a child process, polls `/api/health`, then loads the web UI at `http://localhost:{port}`.
- **Remote mode:** connects to a user-configured URL; bundled backend is never started.

Key services: `BackendService` (process lifecycle), `PortFinder`, `SettingsService`, `FileWatcherService`, `WindowManager`. Auto-updates use Sparkle 2. The Xcode project is generated from `project.yml` via `xcodegen` — always run `xcodegen generate` after modifying `project.yml` or adding/removing Swift files.

## macOS app — bundled backend paths

The app expects the ASP.NET Core binary inside the app bundle at:
- `starsky.app/Contents/MacOS/runtime-starsky-osx-arm64/starsky` (Apple Silicon)
- `starsky.app/Contents/MacOS/runtime-starsky-osx-x64/starsky` (Intel)

These are copied at build time from `starsky/osx-arm64/` and `starsky/osx-x64/`. A build warning is emitted when they are missing; Local mode will not work without them.

## Documentation
 > Documentation output must be written to `documentation/docs` relative to the repository root.
 > Folders with `__do_not_edit_this__folder` are auto-copied from their source locations by `documentation/scripts/prestart.js` every time the docs site starts. 
 > edit the source files instead.