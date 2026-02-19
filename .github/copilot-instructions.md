
In Starksy we use .NET with the minumum amount of dependencies.

In the front-end / clientapp we use react with vite and jest tests.
In starsky-tools we use nodejs without any package.json dependencies. except for end2end test, that is in cypress
In starskydesktop we use Electron.

For unit testing we use mstest and custom Fake Builders. So for myObject, there is highly likely to exist a FakeMyObject or FakeIMyObject. NEVER suggest Mock.

When using ApplicationDbContext use DatabaseTest as a base class for testing. This will ensure that the database is properly set up and torn down for each test.

Be concise.

If I say "review": Review the code. When reviewing, start with a list of what needs to improve, then separately compliment on the good stuff. We don't use try catch, errors are caught on a higher level.

Always start with a summary in bullets, then full response.

When I ask to explain: explain from a functional point of view what the code does. Do not explain what the fields or the methods do, because the names should be self-explanatory. Then tell me what technical principles have been used. At the end note the design patterns used and the dependencies.
