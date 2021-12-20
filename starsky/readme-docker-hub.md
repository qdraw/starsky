[< readme](readme.md)

# Starsky photo management app on dockerhub

Starsky is a free photo-management tool, which runs on your server or web-space.
Installing is a matter of running the application.
It acts as an accelerator to find and organize images driven by meta information.
Browse and search images in your own cloud. Starsky is the name of the
Starsky DAM (Digital asset management) system that runs as a desktop application and web service.
You could add how to add users, set up your archives, upload content, control metadata, share content and more.

## Online demo
There is an demo environment on heroku, please wait a few seconds before the app is started

[See the online demo on heroku](https://demostarsky.herokuapp.com?classes=btn,btn-default)

> Use the username: `demo@qdraw.nl` and Password: `demo@qdraw.nl` to access the demo

## Pull latest stable release with docker

Make sure that Docker or Docker Desktop is installed

```
docker pull qdraw/starsky:latest
```

### And run the image on port 12837
You can change the port number if you like
```
docker run -e PORT=12837 qdraw/starsky:latest 
```

## Architecture
- linux/amd64 or Intel 64 bits
- linux/arm64 or Raspberry PI 64 bits
- linux/arm/v7 or Raspberry PI 32 bits `*`

`*` All Raspberry Pi's are supported. Except the: 'Raspberry Pi 1 (2014)'
and 'Raspberry Pi Zero (W) (2017)' those two are arm/v6 and the app will not run on those machines

## Changelog
All notable changes to this project will be documented in the following file:
[Changelog on github](https://qdraw.github.io/starsky/history.html)

## Github & source code download
When you want to build and inspect the source code yourself,
please check: [@qdraw/starsky](https://github.com/qdraw/starsky)
