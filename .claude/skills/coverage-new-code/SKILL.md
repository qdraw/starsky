---
name: coverage-new-code
description: Measure test coverage restricted to the lines changed on this branch (SonarQube "coverage on new code"), and find which changed lines are untested. Use when asked about coverage of new/changed code, when checking whether a change is properly tested, or before wrapping up work that added tests.
---

# Coverage on new code

Run from the repo root:

```bash
node starsky-tools/build-tools/coverage-new-code.js --summary
```

With no paths it lists every changed source file (vs `master` by default):

```
 83.33%  lines 2/2  branches 3/4  starsky/starsky.foundation.webtelemetry/Helpers/SetupLogging.cs
100.00%  lines 0/0  branches 0/0  starsky/starsky/Program.cs
```

Then drop `--summary` (optionally with explicit paths) to get the per-line detail that
tells you *which* lines are missing:

```bash
node starsky-tools/build-tools/coverage-new-code.js starsky/.../SetupLogging.cs
        line   38: covered=true  branches 3/4   <-- uncovered
```

Other flags: `--base=<ref>` to diff against another branch.

## Before trusting the numbers

The tool reads `starsky/starskytest/coverage-merge-sonarqube.xml`, a merged .NET + clientapp
report that only a full build regenerates:

```bash
cd starsky && ./build.sh --target CoverageReport
```

Check the report's mtime against the code you are measuring. A stale report makes lines you
added afterwards simply absent, and files you added afterwards show up as
`<-- not in coverage report` — which looks like untested code but is really missing data.
Regenerate before reporting a number, and after adding tests.

## Reading the output

- `lines covered/total` and `branches covered/total` count only lines this branch added or
  rewrote (from `git diff <base>...HEAD --unified=0`).
- `100.00%  lines 0/0` means nothing to measure — changed lines that are comments, usings or
  braces. Not a problem.
- `<-- not in coverage report` means the file is absent from the report entirely: genuinely
  untested new code, or a file excluded from coverage (jest `collectCoverageFrom`, the
  `starskytest` project, generated code).
- `<-- no changed lines vs <base>` means the diff found nothing for that path — usually
  uncommitted work, since only committed changes are diffed. Commit first.
- A warning about a path matching several files means the argument was too short (e.g.
  `Program.cs`); use the repo-relative path.

Only committed changes count. For uncommitted work, run the tests directly instead
(`cd starsky && dotnet run --project starskytest/starskytest.csproj -- --filter "FullyQualifiedName~ClassName"`),
then commit and re-measure.
