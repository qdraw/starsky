# Architecture Test Documentation

## Overview

The `ArchitectureTest.cs` file contains automated tests to ensure that the project's architecture
adheres to specific guidelines. These guidelines are based on the Sitecore Helix principles, which
help maintain a clean and scalable architecture over time.

## Key Concepts

### Fitness Functions

Fitness functions are tests that confirm whether a particular characteristic of the solution's
architecture is maintained over time. These tests help ensure that the architecture remains
consistent and adheres to predefined rules.

### Helix Principles

The Helix principles define a set of guidelines for structuring projects within a solution. The key
rules enforced by these tests are:

- **Foundation projects** should only reference other Foundation projects.
- **Feature projects** should only reference Foundation projects.
- **Project projects** should only reference Foundation or Feature projects.

## Test Class: `TheSolutionShould`

### Purpose

The `TheSolutionShould` class contains tests that validate the project reference structure within
the solution. It ensures that the inter-project references comply with the Helix principles.

### Key Methods

#### `ContainNoHelixInvalidProjectReferences`

This test method verifies that there are no invalid project references within the solution. It
performs the following steps:

1. **Arrange**: Retrieves a list of projects and their references.
2. **Act**: Reviews the project list for invalid references.
3. **Assert**: Checks if any invalid references are found and asserts that there should be none.

#### `GetProjectsWithReferences`

This method retrieves a dictionary of projects and their references from the solution file.

#### `GetSolutionFilePath`

This method locates the solution file path based on the base directory.

#### `GetProjectsFromSolutionFileContents`

This method extracts project references from the solution file contents using a regular expression.

#### `GetReferencedProjects`

This method retrieves the referenced projects from a given project file.

#### `GetInvalidReferences`

This method identifies invalid references for a given project based on the specified invalid layers.

#### `AssertLayerReferences`

This method asserts that there are no invalid references for a given layer.

## Known Issues

- The `starsky.feature.trash` project references the `starsky.feature.metaupdate` project, which is
  a known issue and should be refactored in the future.

## References

- [Microservices and Evolutionary Architecture](https://www.thoughtworks.com/insights/blog/microservices-evolutionary-architecture)
- [Building Evolutionary Architectures](http://shop.oreilly.com/product/0636920080237.do)
- [Sitecore Helix Documentation](http://helix.sitecore.net/introduction/index.html)

---

This documentation provides an overview of the purpose and functionality of the
`ArchitectureTest.cs` file, along with explanations of key concepts and methods.