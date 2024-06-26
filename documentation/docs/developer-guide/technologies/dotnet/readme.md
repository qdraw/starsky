---
sidebar_position: 1
---

# .NET

.NET is a free, cross-platform, open source developer platform for building many kinds of applications.
.NET is built on a high-performance runtime that is used in production by many high-scale apps.

.NET apps (as written in a high-level language like C#) are compiled to an Intermediate Language (IL).
IL is a compact code format that can be supported on any operating system or architecture.
Most .NET apps use APIs that are supported in multiple environments, requiring only the .NET runtime to run.

In this project the backend is written in C# using the .NET

## MSTest unit tests

MSTest is a testing framework for Microsoft .NET applications, written in C#.
It allows developers to write, execute, and manage unit tests in one way across the codebase. MSTest provides
a set of attributes and assertions to test various aspects of code,
including verifying expected output, exceptions, and performance.

> We aim to achieve a code line unit test coverage of more than 90%

## Nuke build tools

Write automation tools and CI/CD pipelines in plain C# and with access to all .NET libraries.
Nuke is a build automation system with first-class support for C# and .NET.
It is designed to be used in any kind of software project, including .NET, .NET Core,
Xamarin, Unity, and many more.

The first argument for me was; you develop your build pipelines in real C#!
A killer feature for a C# developer, because it helps you to start very quickly with all the tools you know.
On top of that, NUKE provides extensions for all major .NET IDE; Rider,
Visual Studio, Visual Studio Code which let you run a target with its dependencies or without,
start a debugging session directly from the target from the hit of a shortcut.

-   [JetBrains Rider NUKE Support plugin](https://nuke.build/docs/ide/rider/)
-   [Visual Studio NUKE Support plugin](https://nuke.build/docs/ide/visual-studio/)
-   [VS Code NUKE Support plugin](https://nuke.build/docs/ide/vscode/)

## Entity Framework Core

Entity Framework Core is a modern object-database mapper for .NET.
Entity Framework Core (EF Core) is an open-source, lightweight,
cross-platform Object-Relational Mapping (ORM) framework for .NET.
It provides a way to interact with relational databases using LINQ
and allows you to work with database data as strongly-typed .NET objects.
EF Core supports multiple database providers. It offers improved performance and greater
flexibility compared to its predecessor, Entity Framework.

### Pomelo.EntityFrameworkCore.MySql

Pomelo.EntityFrameworkCore.MySql is a popular open-source database provider
for Microsoft's Entity Framework Core (EF Core) that allows you to work with MySQL databases in
.NET applications. It provides a high-performance and scalable ADO.NET provider for EF Core,
which is based on the official MySQL Connector/NET. With Pomelo.EntityFrameworkCore.MySql,
you can interact with MySQL databases using LINQ, and map database data to strongly-typed .NET objects.
This provider also supports advanced features like bulk operations, transactions, and more.

### SQLite

The Entity Framework Core SQLite database provider is an open-source database provider
for Microsoft's Entity Framework Core (EF Core) that allows you to work with SQLite databases in
.NET applications. It provides a high-performance,
lightweight and portable ADO.NET provider for EF Core, based on the popular SQLite library.
With the EF Core SQLite provider, you can interact with SQLite databases using LINQ.

## Swagger / OpenAPI

Swagger (now known as OpenAPI) is a widely used open-source framework for describing
and documenting RESTful APIs. It provides a standard, language-agnostic interface to
RESTful APIs, which allows developers to interact with APIs in a human-friendly way.
OpenAPI uses a simple YAML or JSON file to describe the API's endpoints, request/response payloads,
authentication methods, and more. This file is automatically generated to make it easier to work with the API.

## MetadataExtractor

MetadataExtractor is a .NET library for extracting Exif, IPTC, XMP, ICC,
and other metadata from image and document files.
MetadataExtractor supports a wide variety of file formats, including JPEG, PNG, TIFF, PDF,
and many others. The library provides a simple and intuitive API that allows you to extract
metadata properties and values, and to iterate over the metadata directory structure.
With MetadataExtractor, you can extract metadata information such as camera model and settings,
image creation and modification dates, image descriptions, and much more.

## ImageSharp

ImageSharp is a fast, lightweight, and cross-platform library for image processing in C#.
It provides a modern and flexible API for handling images and provides many common image processing
operations, such as resizing, cropping, rotating, and filtering.
ImageSharp supports a variety of image file formats, including JPEG, PNG, GIF, BMP, and others,
and supports both GUI and console applications. The library is designed for high performance
and is optimized for use with multithreaded processors. ImageSharp is easy to use and integrates
well with other libraries, making it a popular choice for developing image-processing applications in C#.

## RazorLight

RazorLight is a fast, lightweight, and flexible library for rendering Razor views in C#.
It is distributed as a NuGet package and can be easily installed in any .NET project.
RazorLight allows you to create and render Razor views, including Razor pages and Razor templates,
outside of an ASP.NET web application context. With RazorLight, you can generate dynamic HTML,
or any other text-based format in a variety of scenarios, such as email templates, report generation,
or dynamic content rendering. The library provides a simple and intuitive API for rendering Razor views,
and supports advanced features like model binding, template inheritance, and more. RazorLight is designed
for high performance and scalability,
and is widely used in various applications that require dynamic content generation in C#.

## Open Telemetry

Open Telemetry is a real-time analytics and monitoring standard that provides insights
into the performance and usage of your applications.
It is a solution that allows you to track application performance,
detect and diagnose issues, and monitor user behavior.
The service provides a wealth of data, including performance metrics, error tracking,
and user telemetry, that can help you quickly identify and resolve issues and improve application quality.

## MedallionShell

MedallionShell is a powerful and flexible command-line shell for .NET applications.
It provides a simple and intuitive API for building and executing shell commands,
and supports advanced features like piping, redirection, and background execution.
MedallionShell allows your code to interact with other command-line tools in your .NET applications.
The library provides a consistent and unified interface for working with the shell,
regardless of the underlying operating system, making it ideal for cross-platform development.

The main usage of MedallionShell is to execute Exiftool commands to update the metadata of images.
