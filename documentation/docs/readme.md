---
sidebar_position: 1
---

import Button from "@site/src/components/button";

# What is Starsky?

Starsky is a free, self-hosted photo manager. It runs on your own computer, NAS or server, and
indexes the photos and videos you already have in normal folders. Search, tag, rename, geotag
and publish them from a web interface or from the macOS and Windows desktop apps.

Photos are never moved into a proprietary library. Tags, descriptions and locations are written
into the files themselves (EXIF/XMP), so you can switch to another tool at any time.

<Button href="getting-started" color="#25c2a0">
  Get started
</Button>

## Why it exists

Starsky started as a personal tool for a growing photo archive. The things that were missing
elsewhere became its features:

- **Fast search** on keywords, dates, descriptions and camera data
- **No lock-in**: all metadata lives in the files
- **Works offline**, for example on a Raspberry Pi at home, without a cloud service
- **Automated import** from an SD card, sorted into folders by date
- **Geotagging** photos from a GPX track recorded on a phone
- **Publishing** a selection of photos as a web page with a watermark

## Try it

There is an online demo of the Starsky application. Please have **some patience** while it warms up.

<Button href="https://demo.qdraw.nl" color="#25c2a0">
  See the online demo
</Button>
<br />
<br />

> The demo is hosted on Azure, which stops the application after 30 minutes of inactivity.

> Content changes in the demo environment are not stored.

## Where to go next

- [Getting started](getting-started/readme.mdx): install the desktop app or run the Docker image
- [Features](features/readme.md): everything Starsky can do
- [Developer guide](developer-guide/readme.md): architecture, conventions and how to contribute
- [Source code on GitHub](https://github.com/qdraw/starsky)
