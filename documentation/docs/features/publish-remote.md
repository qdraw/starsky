# Auto publish to FTP and local targets

With this feature, Starsky can publish your files automatically to remote destinations right after you use **More → Publish**.

You can publish to:

- FTP
- Local file system folders

## What this means for you

After you publish in the modal, Starsky can immediately continue with remote publishing in the background.

So instead of downloading and uploading yourself, Starsky can do that next step for you.

## Before you start

To enable auto publish, you need both:

1. A publish profile that includes `ContentType: PublishRemote`
2. A remote profile in `AppSettingsPublishProfilesRemote` with your destination(s)

If one of these is missing, the normal publish still works, but remote auto publish will not start.

## How `AppSettingsPublishProfilesRemote` works

`AppSettingsPublishProfilesRemote` has two levels:

- `Profiles`: remote targets for a specific profile id
- `Default`: shared remote targets used as fallback for all profiles

The profile id comes from `publishProfiles` (for example `_default`, `profile1`, `profile2`).

Resolution logic:

1. Starsky first checks `AppSettingsPublishProfilesRemote.Profiles[profileId]`.
2. If no profile-specific entry exists, Starsky uses `AppSettingsPublishProfilesRemote.Default`.

This means:

- Use `Default` for common destinations that should apply broadly.
- Use `Profiles.<id>` when a specific publish profile needs different FTP/local targets.

## Example settings

Use this as a template:

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
          "Type": "ftp",
          "Ftp": {
            "WebFtp": "ftp://anotheruser:anotherpass@ftp2.example.com/anotherpath"
          }
        },
        {
          "Type": "LocalFileSystem",
          "LocalFileSystem": {
            "Path": "/tmp"
          }
        }
      ],
      "profile2": [
        {
          "Type": "ftp",
          "Ftp": {
            "WebFtp": "ftp://user:pass@ftp3.example.com/path"
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

## How it works (simple)

1. You click **Publish** in the modal.
2. Starsky prepares the publish output.
3. Starsky checks if remote publishing is enabled for the selected profile.
4. If enabled, Starsky starts publishing to your configured FTP/local targets.

## Troubleshooting

- **No remote publish started**
  - Check if your selected publish profile contains `PublishRemote`.
  - Check if your profile exists in `AppSettingsPublishProfilesRemote`.
- **Publish fails**
  - Verify FTP URL, username, password, and path.
  - For local publishing, verify the destination path exists and has write permissions.

## Advanced (API behavior)

Starsky checks:

```http
GET /api/publish-remote/status?publishProfileName=_default
```

If result is `true`, it starts:

```http
POST /api/publish-remote/create
```

If result is `false`, remote publish is skipped.

## Security note

- Do not commit real FTP credentials to source control.
