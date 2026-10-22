---
title: Build Tools Docs
---

[< starsky/starsky-tools docs](../readme.md)

# Build Tools Docs

-   Scripts that are helping building the application
-   depends on nodejs or powershell,
-   **not needed** to run npm install first
-   does not need to install dependencies

### app-version-update.js

Checks the App Version Update. Update the project versions to have the same version. The version is specified in the file

```
npm run app-version-update
```

### Clientapp create-react-app-update

Regenerate npm package.json file for front-end with the latest version of create react app

```
npm run clientapp-create-react-app-update
```

### documentation-create-docusaurus-update

Regenerate npm package.json file for docusaurus

```
npm run documentation-create-docusaurus-update
```

### audit-dotnet.ps1

Powershell script to check if there are vulnerablies in the .NET solution
Requires .NET 5.0.200 or newer installed
And as argument the targetFolder

### audit-npm.ps1

For checking if there are vulnerablies and update in node.js/npm

### nuget-package-list.js

Creates an list to able to cache the Nuget packages fast

```
npm run nuget-package-list
```

### project-guid.js

Check for duplicate Project GUID's

```
npm run project-guid
```

### release-version-check.js

Make sure the version in files are matching the release version in the branch or tag name.
When this is not the case auto fix it.

#### Github Tags:

```
export GITHUB_REF=refs/tags/v0.3.0
```

#### Azure Devops release branches

```
export BUILD_SOURCEBRANCH=refs/heads/release/0.4.2
```

Note: The release branches should **not** start with a v

### coverage-new-code.js

Coverage restricted to the lines you changed on this branch, the same way SonarQube
reports "coverage on new code": (covered lines + covered branches) / (total lines + total
branches), counted over the added/changed lines only.

It reads `starsky/starskytest/coverage-merge-sonarqube.xml`, so build that first when it
is missing or stale:

```
cd starsky && ./build.sh --target CoverageReport
```

Then, from `starsky-tools/build-tools`:

```
node coverage-new-code.js                       all changed source files
node coverage-new-code.js --summary             lines changed, without the per-line detail
node coverage-new-code.js starsky/starsky/Program.cs
node coverage-new-code.js --base=develop        compare against another base branch
```

```
 83.33%  lines 2/2  branches 3/4  starsky/starsky.foundation.webtelemetry/Helpers/SetupLogging.cs
        line   33: covered=true
        line   38: covered=true  branches 3/4   <-- uncovered
```

Notes:

-   The first number is the branch (or file) total, `covered/total` are the changed lines
    and their branches. Every line listed after it is a changed line that the report knows
    about; `covered=false` or a partly covered branch gets an `<-- uncovered` mark.
-   `100.00%  lines 0/0` means there was nothing to measure, usually because the changed
    lines are comments, usings or braces. A changed file that is missing from the report
    altogether (no test project, or excluded from coverage) is marked
    `<-- not in coverage report`, since that is the case where new code is untested.
-   Only committed changes count. The diff is `<base>...HEAD`, so uncommitted work is
    invisible and shows up as `no changed lines`; commit first.
