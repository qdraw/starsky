# Docker

[Docker](https://www.docker.com/) is an Open Source container virtualization tool. It is ideal for running
applications on any computer without extensive installation, configuration, or performance overhead.

**Note:** Docker is **not** required to build or run the application. 
The output binaries are also run on metal without Docker or .NET installed

We are aware Docker is not widely used by end users despite its many advantages. For this reason, we aim
to provide native binaries for common operating systems.

## Why are we using Docker?

Containers are nothing new; [Solaris Zones](https://en.wikipedia.org/wiki/Solaris_Containers) have been around for
about 15 years, first released publicly in 2004. The chroot system call was introduced during
[development of Version 7 Unix in 1979](https://en.wikipedia.org/wiki/Chroot). It is used ever since for hosting
applications exposed to the public Internet.

Modern Linux containers are an incremental enhancement. A main advantage of Docker is that application images
can be easily made available to users via Internet. It provides a common standard across most operating
systems and devices, which saves our team a lot of time that we can then spend [more effectively](../code-quality.md#effectiveness-efficiency), for example,
providing support and developing one of the many features that users are waiting for.

Human-readable and [versioned Dockerfiles as part of our public source code](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/docker/compose/generic/docker-compose.yml)
also help avoid "works for me" moments and other unwelcome surprises by enabling teams to have the exact same environment everywhere in [development](https://github.com/qdraw/starsky/blob/master/starsky/Dockerfile), staging,
and [production](https://github.com/qdraw/starsky/blob/master/starsky/Dockerfile).

Last but not least, virtually all file format parsers have vulnerabilities that just haven't been discovered yet.
This is a known risk that can affect you even if your computer is not directly connected to the Internet.
Running apps in a container with limited host access is an easy way to improve security without
compromising performance and usability.

## Running Docker Images

Assuming you have Docker installed and want to test Debian 12 "Bookworm", you can simply run this command to open a terminal:

```bash
docker run --rm -v ${PWD}:/test -w /test -ti debian:bookworm bash
```

This will mount the current working directory as `/test`. Of course, you can also specify a full path instead of `${PWD}`.

The available Ubuntu, Debian and Starsky images can be found on Docker Hub:

- https://hub.docker.com/_/ubuntu
- https://hub.docker.com/_/debian
- https://hub.docker.com/r/qdraw/starsky/tags

Additional packages can be installed via `apt`:

```bash
apt update
apt install -y exiftool libheif-examples
```

## Continuous Integration / Deployment ##

Build and push of an updated container image to [Docker Hub](https://hub.docker.com/r/qdraw/starsky/tags) for stable releases and
are automatically performed by [Github Actions CI](../github-actions/readme.md) 

The latest development version is available as `ghcr.io/qdraw/starsky:master` on [Github Actions Hub](https://github.com/qdraw/starsky/pkgs/container/starsky).

## Multi-Stage Build ##

When creating new images, Docker supports so called multi-stage builds, 
that means you can compile an application like Starsky in a container that contains all development dependencies 
(like source code, debugger, compiler,...) and later copy the binary to a fresh container. 
This way we could reduce the compressed container size from ~1 GB to less than 200 MB. 


## Kubernetes ##
- https://forge.sh/ - Define and deploy multi-container apps in Kubernetes, from source
- https://www.telepresence.io/ - a local development environment for a remote Kubernetes cluster

## External Resources ##
- https://github.com/estesp/manifest-tool
- https://github.com/docker/app
- https://github.com/moby/moby
- https://hub.docker.com/r/multiarch/qemu-user-static/ - quemu for building multiarch images with Docker
- [https://github.com/opencontainers/image-spec](https://github.com/opencontainers/image-spec/blob/master/annotations.md#pre-defined-annotation-keys) - standard labels for Docker image metadata
