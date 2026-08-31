# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is Starsky?

Starsky is a photo-management platform with a .NET/ASP.NET Core backend, a React frontend, several CLI tools, an Electron desktop app, and a native macOS app (Swift/AppKit). All business logic lives in the backend; the frontends are display/interaction layers only.

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

## Commands

### Full build (all projects + tests)

```bash
./build.sh          # macOS / Linux
.\build.ps1         # Windows PowerShell
```

### .NET backend

```bash
# Run all .NET tests
dotnet test starsky/starskytest/starskytest.csproj

# Run a single test class
dotnet test starsky/starskytest/starskytest.csproj --filter "FullyQualifiedName~ClassName"

# Format all .csproj files
cd starsky && ./format.sh
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

# Test (57 tests, XCTest)
xcodebuild test -project starsky.xcodeproj -scheme starskyTests \
  -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO
```

### Version bump

```bash
node starsky-tools/build-tools/app-version-update.js
```

## .NET testing conventions

- Test framework: **MSTest** — use `[TestMethod]` + `[DataRow]` for parameterised tests. `[DataTestMethod]` is deprecated; do not use it.
- **Never use mocks.** Every object that needs a fake has a hand-written `FakeMyObject` or `FakeIMyObject` in `starskytest/FakeCreateAn/` or `starskytest/FakeMocks/`. Use those.
- Tests that touch `ApplicationDbContext` must extend `DatabaseTest` (in `starskytest/DatabaseTest.cs`); this sets up and tears down the in-memory database automatically.
- Tests live in `starsky/starskytest/`, mirroring the source project structure.

## Code conventions

- **No try/catch in feature/foundation code.** Exceptions are caught at the top-level middleware; let them propagate.
- **Avoid new NuGet/npm dependencies.** Prefer a small in-tree helper over adding a package.
- Frontend: Prettier enforces formatting. Install a Prettier editor plugin; the CI will fail on violations.
- TypeScript: use types everywhere, avoid `any`.
- Comments should explain *why*, not *what*. Avoid restating what the code already says.
- Documentation for users goes in `documentation/docs/`.
- **Do not create git commits.** Leave committing to the user.

## macOS app architecture (mac/)

The native app (`mac/`) is a thin AppKit shell around a `WKWebView`. It operates in two modes:

- **Local mode (default):** finds a free TCP port, launches the bundled `starsky` ASP.NET Core binary as a child process, polls `/api/health`, then loads the web UI at `http://localhost:{port}`.
- **Remote mode:** connects to a user-configured URL; bundled backend is never started.

Key services: `BackendService` (process lifecycle), `PortFinder`, `SettingsService`, `FileWatcherService`, `WindowManager`. Auto-updates use Sparkle 2. The Xcode project is generated from `project.yml` via `xcodegen` — always run `xcodegen generate` after modifying `project.yml` or adding/removing Swift files.

## macOS app — bundled backend paths

The app expects the ASP.NET Core binary inside the app bundle at:
- `starsky.app/Contents/MacOS/runtime-starsky-osx-arm64/starsky` (Apple Silicon)
- `starsky.app/Contents/MacOS/runtime-starsky-osx-x64/starsky` (Intel)

These are copied at build time from `starskydesktop/runtime-starsky-mac-arm64/` and `starskydesktop/runtime-starsky-mac-x64/`. A build warning is emitted when they are missing; Local mode will not work without them.
