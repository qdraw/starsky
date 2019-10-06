[< starsky/starsky-tools docs](../readme.md)

# Starksy Remote Thumbnail Helper
Client side thumbnail generation interacting with a Starsky WebApi

## Usage of Starksy Remote Thumbnail Helper
- use numbers (e.g. 1) to search relative
- use a range to search relative in that range (e.g. 1-7 to search for last week)
- use the keyword 'IMPORT' to search for recent imported files (case-sensitive)
- use a keyword to search and check if thumbnails are created

## Requirements
- Node.js 10+
- Starsky Web running

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
