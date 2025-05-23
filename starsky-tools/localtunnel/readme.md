---
title: Localtunnel
---

[< starsky/starsky-tools docs](../readme.md)

# Localtunnel

Proxy the Create React App on: `localhost:3000` and for the .NET Core application `localhost:5000` to debug on mobile and remote devices.
Cookies with a secure-label will be rewritten to support a non-secure context

## Edit: `.env`

```sh
SUBDOMAIN=gentle-ladybug-63
PORT=6501
STARSKYURL=http://localhost:5000
```

## Due Cookie protecion:

Clear cookies first
Before login visit http://localhost:4000/account/login
This will set an valid X-XSRF-TOKEN:"CfDJ8Hx..." cookie
and go back to http://localhost:4000

## Keep the Front-end watcher running in `starksy/clientapp`

The Create React App can now operate without backend service running

## install

```sh
npm ci
```

## run

```sh
npm run start
```

And ready :)

Node will output something like this:

```
http://localhost:6501
Your localtunnel is ready on:
https://gentle-ladybug-63.localtunnel.me
```
