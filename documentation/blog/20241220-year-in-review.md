---
slug: starsky-2024-year-in-review
title: "Starsky 2024 Year in Review 🚀"
authors: dion
tags: [photo mangement, software update]
date: 2024-12-20
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/01_video_search_cloud_starsky_v050.gif
---

# Starsky 2024 Year in Review

2024 has been a transformative year for the Starsky project with significant updates and enhancements across various components. Below is a summary of the key updates and improvements:

#### Version 0.6.4 - 2024-12-19
- Fixed back-end notification duplicate error handling.
- Added cache headers for download publish for Cloudflare.
- Improved back-end readability and filename checks.
- Enhanced tests for the port mapper.
- Updated ESLint configurations and removed the use of `any`.

#### Version 0.6.3 - 2024-11-14
- Fixed front-end upload modal status issue.
- Changed thumbnail generation behavior to run in the background.
- Added cleanup of non-linked thumbnails on startup.
- Moved stacktrace logging out of the endpoint for security.
- Fixed several bugs related to thumbnail cleaning, logging, and API responses.
- Improved date parsing and added new UI elements in the front-end.

#### Version 0.6.2 - 2024-10-11
- Fixed query execution timeouts and download issues.
- Improved password hashing security and updated models.
- Upgraded Cypress and ESLint for the tools.

#### Version 0.6.1 - 2024-05-16
- Improved front-end contrast for navigation.
- Fixed demo site issues and back-end query timeouts.
- Addressed concurrency conflicts and updated Electron version.

#### Version 0.6.0 - 2024-03-15
- Major upgrade to .NET 8.
- Fixed various issues related to version checks, regex timeouts, and logging.
- Updated URLs and documentation links in the front-end.

#### Beta Versions
- **0.6.0-beta.3 (2024-03-11)**: Fixed back-end dispose issues, updated ImageSharp, and added support for Apple Silicon Mac OS in the desktop app.
- **0.6.0-beta.2 (2024-03-05)**: Added native open file support on Windows & Mac OS, and renamed `UseLocalDesktopUi` to `UseLocalDesktop`.
- **0.6.0-beta.1 (2024-02-18)**: Fixed image import issues and updated Docker base packages.
- **0.6.0-beta.0 (2024-02-11)**: Added support for OpenTelemetry and upgraded to .NET 8 SDK 8.0.100.

These updates reflect the continuous efforts to enhance the Starsky project, ensuring better performance, security, and user experience. For more detailed release notes, you can refer to the [history file](https://github.com/qdraw/starsky/blob/e5b16cd83a1dada4066e7c909a272bd6f5b47589/history.md).

This year has marked a significant improvement in the Starsky ecosystem, focusing on robustness, user-centric features, and maintaining a high standard of security and performance. We look forward to continuing this journey and achieving new milestones in the coming year.
