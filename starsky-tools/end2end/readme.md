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
