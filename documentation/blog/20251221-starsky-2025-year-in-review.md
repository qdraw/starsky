---
slug: starsky-2025-year-in-review
title: "Starsky 2025 Year in Review – A Year of Maturity, Stability, and Big Steps Forward"
authors: dion
tags: [photo mangement, software update]
date: 2025-12-21
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# Starsky 2025 Year in Review – A Year of Maturity, Stability, and Big Steps Forward

The past year has been an important one for **Starsky**. What started as a powerful self-hosted photo and media management tool continued its transition into a more mature, robust, and scalable platform. From foundational breaking changes to long-term stability improvements, 2025 was less about flashy features and more about _getting the core right_.

Looking back over the releases from **0.6.7 through 0.7.6**, several clear themes emerge: modernization, resilience, better tooling, and preparing Starsky for the future.

## A Major Milestone: The 0.7 Line

The release of **Starsky 0.7.0** marked the most significant shift this year. It wasn’t just another version bump—it represented a conscious decision to evolve core assumptions and clean up long-standing technical debt.

### Smarter Imports and Structural Changes

One of the biggest breaking changes was the **new import structure**, enabling smarter, rule-based imports with conditional logic, structure definitions, and color classes. This change laid the groundwork for more predictable, extensible media organization and was important enough to deserve its own dedicated blog post.

While breaking changes are never easy, this one clearly signaled that Starsky is designed for the long term—not frozen by backward compatibility at the cost of clarity.

### WebP as the Default Thumbnail Format

Switching the default thumbnail format to **WebP** was another bold but forward-looking move. It improved performance, reduced storage usage, and aligned Starsky with modern web standards. The beta period helped iron out platform-specific issues (notably on macOS), leading to a stable rollout.

## Backend: Stability, Performance, and Observability

Much of the work this year happened quietly in the backend—but its impact is significant.

### Reliability Improvements

-   Better exception handling (ZIP files, HttpClient timeouts)
-   Improved database concurrency handling
-   Reduced timeouts and safer background jobs
-   More predictable temp file behavior

These changes don’t add new buttons to the UI, but they make Starsky feel _solid_. Fewer edge cases, fewer surprises.

### Thumbnail Generation Grows Up

Thumbnail generation saw continuous attention:

-   Background jobs for small thumbnails
-   Native QuickLook and Shell thumbnails on macOS and Windows
-   Better logging and diagnostics
-   Fixes for race conditions and stale thumbnails after renames

This culminated in a system that is both more performant and easier to reason about when something goes wrong.

## Front-end and Desktop: Modernization Without Disruption

On the front-end side, 2025 was about staying current without reinventing everything.

-   Dependency updates across the board
-   Migration work leading up to **React 19**
-   Code style consistency improvements
-   UX refinements like better input defaults and clearer error handling

The Electron-based desktop app followed the same philosophy: frequent dependency updates, fewer surprises, and better alignment with the web app.

## The Quiet Wins

Some of the most meaningful improvements were small, targeted fixes:

-   GPX parsing edge cases
-   Slug handling for special characters
-   Regex timeouts
-   WebSocket update reliability
-   Better defaults and clearer error messages

Individually minor, collectively transformative. This is the kind of work that turns a promising tool into one people rely on daily.

## Looking Ahead

With **0.7.6** still unreleased and focused on correctness rather than features, Starsky feels like it’s entering a new phase: one where the foundation is strong enough to support bolder ideas again.

After a year of:

-   breaking old assumptions,
-   modernizing the stack,
-   tightening security,
-   and improving stability,

Starsky is better positioned than ever to grow—without losing its focus.

If 2024 was about momentum, **2025 was about maturity**.

Here’s to the next chapter.
