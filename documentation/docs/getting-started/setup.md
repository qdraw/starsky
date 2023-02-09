---
sidebar_position: 4
---

# Setup server app

Starsky can be installed on all operating systems supporting Docker, as well as FreeBSD, Raspberry Pi, and many NAS devices.

There are multiple ways of installing Starsky:

1. **As background service (systemd or pm2 service)** <br />
   Run it as system service. All dependencies are included in the application
   There are multiple options to run it as a service, see [systemd](linux-systemd.md), [macOS launchctl](macos-launchctl.md), [windows service](windows-as-server/windows-service.md) or [pm2](pm2.md) for more information

2. **Docker** <br />
   When using Docker we recommend running Starsky with Docker Compose when hosting it on a private server. It is available for Mac, Linux, and Windows. [Read more about docker configuration here](docker/docker-compose.md)

3. **In IIS** (Windows Pro and Server Only)<br />
   When running the Pro and Server version of windows the IIS webserver can be used [Read more about IIS configuration here](windows-as-server/iis.md)

Once the initial setup is complete, our [First Steps 👣 ](first-steps) tutorial guides you through the user interface and settings to ensure your library is indexed according to your individual preferences.

> > Our stable version and development preview have been built into a single multi-arch Docker image for 64-bit AMD, Intel, and ARM processors. That means, Raspberry Pi 3 / 4, Apple Silicon, and other ARM64-based devices can pull from the same repository, enjoy the exact same functionality, and can follow the regular installation instructions after going through a short list of requirements. See FAQs for instructions and notes on alternative installation methods.

## Roadmap

Our vision is to provide the most user- and privacy-friendly solution to keep your pictures organized and accessible. The roadmap shows what tasks are in progress, what needs testing, and which features are going to be implemented next.

We have a low bug policy and do our best to help users when they need support or have other questions. This comes at a price, as we can't give exact deadlines for new features.

Having said that, funding really has the highest impact. [So users can do their part and become a sponsor to get their favorite features as soon as possible.](https://www.paypal.me/qdrawmedia)

## System Requirements

You should host Starsky on a server with at least 2 cores, 3 GB of physical memory, 1 and a 64-bit operating system. Beyond these minimum requirements, the amount of RAM should match the number of CPU cores. Indexing large photo and video collections also benefits greatly from local SSD storage, especially for the database and cache files.

If your server has less than 4 GB of swap space or a manual memory/swap limit is set, this can cause unexpected restarts, for example, when the indexer temporarily needs more memory to process large files. High-resolution panoramic images may require additional swap space and/or physical memory above the recommended minimum.

> We take no responsibility for instability or performance problems if your device does not meet the requirements.

### Databases

Starsky is compatible with SQLite 3 and MariaDB 10.5.12+.2 Note that SQLite is generally not a good choice for users who require scalability and high performance, and that support for MySQL 8 has been discontinued due to low demand and missing features.

### Browsers

Built as a Progressive Web App (PWA), the web interface works with most modern browsers, and runs best on Chrome, Chromium, Safari, Firefox, and Edge. You can conveniently install it on the home screen of all major operating systems and mobile devices. Internet Explorer is not supported.

### Video playback

Not all video and audio formats can be played with every browser. For example, AAC - the default audio codec for MPEG-4 AVC / H.264 - is supported natively in Chrome, Safari, and Edge, while it is only optionally supported by the OS in Firefox and Opera.

### HTTPS

If you install Starsky on a public server outside your home network, always run it behind a secure HTTPS reverse proxy such as Traefik or Caddy. Your files and passwords will otherwise be transmitted in clear text and can be intercepted by anyone, including your provider, hackers, and governments.

## Getting Support

If you need help installing our software at home, you post your question in GitHub Discussions. Common problems can be quickly diagnosed and solved using our Troubleshooting Checklists.

### Sponsor us

We'll do our best to answer all your questions. In return, we ask you can sponsor us. Think of "free software" as in "free speech," not as in "free beer". Thank you!

In exchange for their continued support, sponsors are also welcome to request direct technical support via email. Please bear with us if we are unable to get back to you immediately due to the high volume of emails and contact requests we receive.

> > We kindly ask you not to report bugs via GitHub Issues unless you are certain to have found a fully reproducible and previously unreported issue that must be fixed directly in the app. Contact us or a community member if you need help, it could be a local configuration problem, or a misunderstanding in how the software works.
