---
slug: project-starsky-2022-a-year-in-review
title: Project Starsky 2022 a year in review
authors: dion
tags: [photo mangement, year in review]
date: 2022-12-31
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/01_video_search_cloud_starsky_v050.gif
---

# Project Starsky 2022 a year in review

So it was high time to optimise the time-consuming process of photo management. I really enjoy going out and taking pictures. But when I get home, it’s time to properly organise all those photos. That task is typically something we postpone, but it is nice to share the photos and experiences. I have developed a piece of software for this process and in this blog I will tell you what I have improved in this in 2022. This year it really started for the project. The first blog post about the project is a fact. In addition, a demo can also be viewed at [demo.qdraw.nl](https://demo.qdraw.nl)

/* truncate */

## 12 releases done in 2022

What have I worked on in the past year? For example, I’ve been working on real-time updates from the file system so you’re always up-to-date! Added a new type of sort in the list, write a temporary file first before writing a file, to avoid corrupted files. Upgraded to Microsoft’s .NET 6 framework. Added configuration to exclude directories from keeping index up to date. We now keep notifications in a uniform way, even if you have lost the connection for a while, you can retrieve the recent updates. Upgraded the front-end library: React 17 to 18 and the desktop app library: Electron upgraded to the latest version several times. The dependency on Exiftool to write the metadata has been reduced as it is already added during the build. Server side version added for M1/M2 Macs, the desktop version will follow at a later time, I’m running into Gatekeeper (security of Mac OS) issues there. The Diskwatcher functionality has been completely rewritten so that it takes a maximum of 20 seconds to check for duplicate events and filter them out, resulting in a huge performance gain. Furthermore, comparing the database and the file system has changed so that it is a lot faster than before. All this ensured that this works well and released version 0.5.0. Then worked on better documentation, there is still a lot to be gained here and adding locations manually. On to a good 2023!


## What’s next?

The next functionality that will be added is to ensure that small versions of the images are always available, so that you can easily see the images in the overview. There are also several milestones where there are ideas to improve the project. If you want to help, [contribute](https://github.com/qdraw/starsky) or make a contribution, it is appreciated.


![Demo site on mobile](https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg)

