---
slug: new-feature-auto-publish-remote-ftp-local
title: "New feature: Auto publish to FTP and local targets"
authors: dion
tags: [photo mangement, software update]
date: 2026-02-28
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: Auto publish to FTP and local targets

Publishing in Starsky is now easier. After using **More → Publish**, Starsky can automatically continue with remote publishing to your configured targets.

That means less manual work: no extra download/upload step if your remote settings are configured.

<!-- truncate -->

## What is new?

Starsky now supports automatic remote publish right after the modal publish flow is ready.

You can publish to:

- FTP destinations
- Local file system destinations

## Why this helps

If you regularly publish to the same destination, this saves time and reduces repetitive steps.

Typical flow before:

1. Publish
2. Download result
3. Upload/copy manually

Now, with remote auto publish enabled, Starsky can do step 3 automatically.

## How to enable it

You need two configuration parts:

1. `publishProfiles` must include `ContentType: PublishRemote` in the profile you use.
2. `AppSettingsPublishProfilesRemote` must contain remote targets.

If one of these is missing, regular publish still works, but remote auto publish will be skipped.

## How `AppSettingsPublishProfilesRemote` works

`AppSettingsPublishProfilesRemote` has two layers:

- `Profiles`: specific remote targets per profile id
- `Default`: fallback targets for all profiles

The selected profile id comes from `publishProfiles`.

Resolution order:

1. Starsky checks `Profiles[profileId]` first.
2. If that does not exist, Starsky uses `Default`.

This gives you a clean setup:

- Put shared destinations in `Default`
- Override only where needed in `Profiles.<id>`

## Example

```json
{
  "publishProfiles": {
    "_default": [
      {
        "ContentType": "PublishRemote"
      }
    ]
  },
  "AppSettingsPublishProfilesRemote": {
    "Profiles": {
      "profile1": [
        {
          "Type": "ftp",
          "Ftp": {
            "WebFtp": "ftp://user%40example.com:password@ftp.example.com/path"
          }
        },
        {
          "Type": "LocalFileSystem",
          "LocalFileSystem": {
            "Path": "/tmp"
          }
        }
      ]
    },
    "Default": [
      {
        "Type": "ftp",
        "Ftp": {
          "WebFtp": "ftp://defaultuser:defaultpass@defaultftp.example.com/defaultpath"
        }
      }
    ]
  }
}
```

## What happens during publish

1. You click **Publish**.
2. Starsky prepares the publish output.
3. Starsky checks whether remote publish is enabled for that profile.
4. If enabled, Starsky starts remote publish automatically.

## Security reminder

Never commit real FTP credentials to source control. Use secure configuration management for production secrets.
