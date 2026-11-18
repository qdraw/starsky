---
sidebar_position: 1
---

# Developer Guide

This guide is for developers who want to understand or contribute to Starsky. It covers the
architecture, conventions, testing and build pipelines. For how to use the app, see the
[Features](../features/readme.md) section instead.

## Architecture in short

All business logic lives in the .NET backend. The React web app, the macOS and Windows desktop
apps, the CLI tools and the [MCP server](../features/mcp.md) are thin layers over the same
services, so each rule (for example how a file is renamed) exists in one place.

The solution is split into `starsky.feature.*` libraries (one per capability, such as search,
import or trash) and `starsky.foundation.*` libraries (shared infrastructure such as storage,
database and metadata). See the [architecture overview](../advanced-options/readme.md#architecture-overview).

## Conventions

- Tests use MSTest and hand-written fakes, no mocking framework
- Few dependencies: prefer a small in-tree helper over a new NuGet/npm package
- End-to-end tests follow the [Cypress guidelines](testing/end2end-guidelines.md)
- Code quality is tracked with SonarCloud and Codecov, see [code quality](code-quality.md)

## Contributing

Contributions are welcome: documentation fixes, bug fixes, new features and translations.
Read the [Contributing Guide](contributing/CONTRIBUTING.md) first, and use GitHub issues for bugs
and [GitHub Discussions](https://github.com/qdraw/starsky/discussions) for questions and ideas.
