# Raspberry Pi

Our stable version and development preview have been built into a single multi-arch Docker image for 64-bit AMD, 
Intel, and ARM processors.

As a result, Raspberry Pi 3 / 4, Apple Silicon (M1/M2), and other ARM64-based devices can pull from the same repository, 
enjoy the same functionality, and can follow the regular installation instructions after 
going through a short list of system requirements.

### System Requirements ###

- Your device should have at least 3 GB of physical memory and a 64-bit operating system
- While the app has should work on devices with less memory, we take no responsibility for instability or performance problems
- Indexing large photo and video collections significantly benefits from [local SSD storage](troubleshooting/performance.md#storage) and plenty of memory for caching, especially the conversion of RAW images and the transcoding of videos are very demanding
- If less than [4 GB of swap space](troubleshooting/docker.md#adding-swap) is configured or a manual memory/swap limit is set, this can cause unexpected restarts, for example, when the indexer temporarily needs more memory to process large files
- High-resolution panoramic images may require additional swap space and/or physical memory above the recommended minimum
- We recommend disabling [kernel security](troubleshooting/docker.md#kernel-security) in your
  [docker-compose.yml](https://raw.githubusercontent.com/qdraw/starsky/master/starsky/docker/compose/generic/docker-compose.yml), especially if you do
  not have experience with the configuration:
  ```yaml
  starsky:
    security_opt:
      - seccomp:unconfined
      - apparmor:unconfined
  ```
- If you install Starsky on a public server outside your home network, **always run it behind a secure HTTPS reverse proxy** such as Traefik or Caddy

### Architecture Specific Notes ###

#### Modern ARM64-based Devices ####

| Image               | Name                            |
|---------------------|---------------------------------|
| Stable Release      | `qdraw/starsky:latest`          | 
| MariaDB             | `arm64v8/mariadb:10.9`          | 

Running 64-bit Docker images under Raspbian Linux requires a minimum of technical experience to perform the necessary [configuration changes](#raspberry-pi-os). This is because it is a 32-bit operating system with merely a 64-bit kernel to ensure compatibility with legacy software.  If you don't need compatibility with 32-bit apps, we recommend choosing a standard 64-bit Linux distribution instead as it will save you time and requires less experience:

- [Raspberry Pi Debian](https://raspi.debian.net/)
- [Ubuntu for Raspberry Pi](https://ubuntu.com/raspberry-pi)
- [UbuntuDockerPi](https://github.com/guysoft/UbuntuDockerPi) is a 64-bit Ubuntu Server with Docker pre-configured


> **Info**<br />
Other distributions that target the same use case as Raspbian, such as CoreELEC, will have similar issues and should therefore also be avoided to run modern server applications.

### Is a Raspberry Pi fast enough? ###

This largely depends on your expectations and the number of files you have. Most users report that
Starsky runs smoothly on their Raspberry Pi 4. However, initial indexing typically takes much longer
than on standard desktop computers.
