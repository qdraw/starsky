[< starsky/starsky-node-client docs](../readme.md)

# Thumbnail node client
Client side thumbnail generation interacting with a Starsky WebApi

## Requirements
- Node.js 9+
- Starsky Web

```sh
nano .env
```
```sh
STARKSYACCESSTOKEN=base64username:password
STARKSYBASEURL=
```
`STARKSYACCESSTOKEN` is a base64 encoded username and password
`STARKSYBASEURL` is the root url of the installation

### Build Typescript files:

```sh
npm run build-ts
```

And start with the default options
```sh
npm run start
```
