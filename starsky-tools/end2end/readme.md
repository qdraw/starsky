[< starsky/starsky-tools docs](../readme.md)

# Cypress end2end testing

Running end to end testing

## install

```sh
npm ci
```

## Run with UI enabled

```sh
npm run start
```

And ready :)

## With docker compose

Running on http://localhost:12837

```sh
npm run open:docker
```

## With Env variables

`CYPRESS_BASE_URL` - main domain its running
`cypress_name` 'local', heroku-demo or no-create-account
`cypress_AUTH_USER` account to login, default "demo@qdraw.nl"
`cypress_AUTH_PASS` password to login default "demo@qdraw.nl"

Example to set values before testing:

```sh
export CYPRESS_BASE_URL="https://demostarsky.herokuapp.com"
export cypress_name="no-create-account"
export cypress_AUTH_USER="demo@qdraw.nl"
export cypress_AUTH_PASS="demo@qdraw.nl"
```

Open with window

```sh
npm run start:env
```

Run headless

```sh
npm run e2e:env
```

Run for example starsky on any input

```powershell
.\starsky.exe --urls "http://*:5100;https://*:5101"
```

# With cypress.io dashboard enabled

```sh
export CYPRESS_PROJECT_ID={projectId}
export CYPRESS_RECORD_KEY=abc-key-123
```

## ReInstall

In case there is a major upgrade of all dependencies

This is the base `package.json`

```json
{
    "name": "end2end",
    "version": "0.5.0-beta.0",
    "description": "End2End testing of the application",
    "scripts": {
        "start": "cypress open --env configFolder=starsky,configEnv=local,CYPRESS_RETRIES=2",
        "start:env": "cypress open",
        "open:env": "npm run start:env",
        "start:heroku": "npm run start:heroku-demo",
        "open:heroku": "npm run start:heroku-demo",
        "open:heroku-demo": "npm run start:heroku-demo",
        "start:heroku-demo": "cypress open --env configFolder=starsky,configEnv=heroku-demo,CYPRESS_RETRIES=2",
        "e2e:heroku-demo": "cypress run --env configFolder=starsky,configEnv=heroku-demo,CYPRESS_RETRIES=2",
        "open:docker": "cypress open --env configFolder=starsky,configEnv=docker,CYPRESS_RETRIES=2",
        "start:docker": "npm run open:docker",
        "e2e:docker": "cypress run --env configFolder=starsky,configEnv=docker,CYPRESS_RETRIES=2",
        "e2e:env": "cypress run",
        "e2e:env:record": "cypress run --record",
        "cache-path": "cypress cache path",
        "cache": "cypress cache path",
        "lint": "node node_modules/eslint/bin/eslint.js \"**\" --max-warnings 0"
    }
}
```

And the following npm packages are installed

```
npm install --save-dev @types/node
npm install --save-dev @typescript-eslint/eslint-plugin
npm install --save-dev @typescript-eslint/parser
npm install --save-dev cypress
npm install --save-dev eslint
npm install --save-dev eslint-plugin-import
npm install --save-dev eslint-plugin-node
npm install --save-dev eslint-plugin-promise
npm install --save-dev get-port
npm install --save-dev typescript
npm install --save-dev eslint-config-standard-with-typescript --force
```
