[< starsky/starsky-tools docs](../readme.md)
# Cypress end2end testing

Running end to end testing

## install
```sh
npm ci
```

## run
```sh
npm run start
```

And ready :)


## With Env variables

`CYPRESS_BASE_URL` - main domain its running
`cypress_name` 'local', heroku-demo or no-create-account
`cypress_AUTH_USER` account to login, default "demo@qdraw.nl"
`cypress_AUTH_PASS` password to login default "demo@qdraw.nl"


Example to set values before testing:

```sh
export CYPRESS_BASE_URL="https://starskydemo.herokuapp.com"  
export cypress_name="no-create-account"
export cypress_AUTH_USER="demo@qdraw.nl"
export cypress_AUTH_PASS="demo@qdraw.nl"
```

Open with window
```
npm run start:env
```

Run headless
```
npm run e2e:env
```

Run for example starsky on any input
```
.\starsky.exe --urls "http://*:5100;https://*:5101"
```
