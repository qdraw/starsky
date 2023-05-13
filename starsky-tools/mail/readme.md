---
title: Mail
---

[< starsky/starsky-tools docs](../readme.md)

# Node Mail Docs

The goal is to auto download gpx files from gmail and import them into Starksy

This is using the following environment variables

```sh
IMAPUSER=
IMAPPASSWORD=
STARKSYACCESSTOKEN=base64-username:password
STARKSYURL=https://example.com/import
STARKSYGEOURL=http://localhost:5000/starsky/api/geo/sync
```

## When using gmail

In https://myaccount.google.com/security, do you see 2-step verification set to ON? If yes, then visiting https://myaccount.google.com/apppasswords should allow you to set up application specific passwords. 

Select the app and device for which you want to generate the app password.

give it an name and copy the generated password
