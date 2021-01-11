# Slack Notification

This script is used in some piplelines to display notifications in a Slack group

```
cd starsky-tools/slack-notification
```

The following environment variables are used

```
export SLACK_WEBHOOK=https://hooks.slack.com/services/?
export SLACK_TITLE=example title
export SLACK_MESSAGE=example message
```

```
node slack-notification
```

There are no external dependencies required
