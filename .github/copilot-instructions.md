
In Starksy we use .NET with the minumum amount of dependencies.

In the front-end / clientapp we use react with vite and jest tests.
In starsky-tools we use nodejs without any package.json dependencies. except for end2end test, that is in cypress

For unit testing we use mstest and custom Fake Builders. So for myObject, there is highly likely to exist a FakeMyObject or FakeIMyObject. NEVER suggest Mock.

When using ApplicationDbContext use DatabaseTest as a base class for testing. This will ensure that the database is properly set up and torn down for each test.

When writing tests use `[TestMethod]` and `[DataRow]` to cover multiple scenarios in a single test method. `[DataTestMethod]` is deprecated use `[TestMethod]` with `[DataRow]` instead.

- Unit tests are located in `starsky/starskytest`.
- Run Microsoft Testing Platform (MTP) tests from the repository root with:
  `cd starsky && dotnet run --project starskytest/starskytest.csproj`
- Run Windows Microsoft Testing Platform (MTP) tests from the repository root on Windows with:
  `cd windows && dotnet run --project starsky.Tests/starsky.Tests.csproj`
- Run macOS XCTest project tests from `mac/` with:
  `xcodebuild test -project starsky.xcodeproj -scheme starsky -destination 'platform=macOS' CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO`

Be concise.

Do not commit in any branch

If I say "review": Review the code. When reviewing, start with a list of what needs to improve, then separately compliment on the good stuff. We don't use try catch, errors are caught on a higher level.

Always start with a summary in bullets, then full response.

When I ask to explain: explain from a functional point of view what the code does. Do not explain what the fields or the methods do, because the names should be self-explanatory. Then tell me what technical principles have been used. At the end note the design patterns used and the dependencies.

Documentation output must be written to `documentation/docs` relative to the repository root.

## Desktop clients

- Mac/Windows clients are thin shells; business logic belongs in the .NET backend.

- `mac/` is a native Swift/AppKit client that hosts the web app in a `WKWebView`.
  Run `xcodegen generate` after adding or removing Swift files.
- `windows/` is a native WPF client that hosts the web app in `WebView2`.
