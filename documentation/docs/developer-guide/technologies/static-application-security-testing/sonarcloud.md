# Sonarcloud

Sonarcloud is a static code analysis platform that empowers developers to enhance code quality and maintainability effortlessly. By automatically scanning code for various languages, SonarCloud identifies and flags issues such as bugs, security vulnerabilities, and code smells early in the development process. With its seamless integration into CI/CD pipelines, SonarCloud provides actionable insights, fostering a culture of continuous improvement and enabling teams to deliver more reliable and secure software.

## Code smells & Bugs
The goal is the reduce the amount of code smells and bugs in the repository and add **no** new ones via an PR

## Suppressions are done in code with comments

Suppressions should be added directly in the code using comment-based directives. For C#, use attributes like `[SuppressMessage]` or `#pragma warning disable`, and always provide a clear explanation for each suppression. For SonarQube, use `// NOSONAR` or language-specific suppression comments. This ensures future maintainers understand the reason for the suppression, promotes transparency, and helps prevent accidental reintroduction of issues. Suppressions should only be used when there is a valid technical or business justification, and not to hide code quality problems.

**Example in C#:**

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Sonar", "S1075:URIs should not be hardcoded", Justification = "Test code, not production")] 
public void MyTestMethod()
{
    var url = "http://localhost";
    // ...
}
```

## Sonarcloud Dashboard

This should be 95% or higher and 0 Code Smells. If it is lower, you should investigate the issues and fix them.
The Sonarcloud dashboard will show you the details of the issues.
There are two types of issues: bugs and vulnerabilities.
Bugs are problems in the code that can cause a crash or other unexpected behavior.
Vulnerabilities are problems in the code that can be exploited by an attacker.

[Because this is an open source project the Sonarcloud dashboard is public](https://sonarcloud.io/project/overview?id=starsky)

![Sonarcloud Dashboard](../../../assets/developer-guide-technologies-static-application-security-testing-sonarcloud-dashboard.jpg)

> [See Sonarcloud dashboard here](https://sonarcloud.io/project/overview?id=starsky)

## PR builds

When a build is succesful run you see a new status update by Sonarcloud where there is a summary of the Qualitygate

Important to check:

- New Issues: Bugs or code smells
- Code coverage
- And avoid duplications

![Sonarcloud PR Build](../../../assets/developer-guide-technologies-static-application-security-testing-sonarcloud-pr-build.jpg)

## Disable rules
See sonar-project.properties in the root of the project for the rules that are disabled.

### Rules for PL/SQL are disabled 
See: https://docs.sonarsource.com/sonarqube-cloud/advanced-setup/languages/pl-sql
