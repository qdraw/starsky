---
sidebar_position: 15
---

# Pull Requests


If you're submitting a pull request for the Starsky project, 
it's a good idea to use the provided template to ensure that your submission is complete and easy for the maintainers to review. 
The template includes sections for a title, description, motivation, and proposed changes, as well as a checklist to ensure that your code meets certain standards, 
such as being well-documented and passing all tests. By using this template, you can save time for both yourself and the maintainers by providing 
all of the necessary information upfront and making it easy to evaluate your contribution. Simply copy the contents of the template into your pull request 
and fill in the relevant sections to get started.

You can find the template here: https://github.com/qdraw/starsky/blob/master/PULL_REQUEST_TEMPLATE.md


## Why are you being so hard on my pull request?

A pull request can be viewed differently from the perspective of a "contributor" or a "maintainer". 
From the contributor's point of view, they might say, "I added a new feature to your project for you!" 
However, the maintainer may interpret it as, "I wrote some code, and now I want you to test, support, document and maintain it indefinitely."

The maintainer is responsible for ensuring that any code changes submitted are something they feel comfortable taking responsibility for. 
This means that the code should be well-tested, clearly implemented, and fit into the overall architecture. 
However, what happens if the existing code doesn't meet these requirements? Is it fair to expect changes in a pull request?

In some cases, the existing code has an advantage simply by being legacy code. For example:

>   If there is no test, but the existing code has been running in production without issue, 
    then it's reasonable to expect tests for any code replacing it.

>    If there is no test, but your code fixes a bug in the existing code, 
    then it's clear that a test should have been implemented earlier, and this is a great opportunity to add one.
    If the code is not clear to the reviewer, a test can help clarify and lock down what the code does, 
    preventing potential future issues.

Another factor that may make the maintainer cautious is whether the code fully solves the problem or at least enough to stand on its own. 
This is particularly relevant for new features and includes questions like:

>    Is the feature general enough to be used by other users? If not, do we really need it, or can it be part of a more general feature?
    Is the feature fully implemented? For example, 
    if a new feature is added, it should be available in the GUI, emit relevant trace information for debugging, 
    be correctly saved in the configuration, etc. If any of these components are missing, the maintainer will have to do additional work after accepting the pull request.

Overall, a well-designed pull request reduces the workload for the maintainer rather than adding to it.