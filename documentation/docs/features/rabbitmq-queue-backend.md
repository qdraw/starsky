# Queue backends (RabbitMQ and Database)

Starsky can process background jobs with different queue backends. Besides `RabbitMq`, you can also use `Database` as transfer backend.

## What this feature does

Queue backends control where background jobs are stored and picked up.

- `InMemory` keeps jobs in one process only (single instance setup)
- `Database` stores jobs in the Starsky database
- `RabbitMq` stores jobs in RabbitMQ

This is useful when you run multiple app instances and need a shared transfer mechanism.

Benefits:

- Better scaling for multi-instance setups
- Shared queue state across instances
- Option to choose infrastructure that fits your environment (`Database` or `RabbitMq`)

## When to choose Database vs RabbitMq

- Choose `InMemory` or `Database` when you want the simplest setup and already run a stable Starsky database.
- Choose `InMemory` or`Database` when you prefer fewer external services to manage.
- Choose `RabbitMq` when you run multiple app instances and want dedicated queue infrastructure.
- Choose `RabbitMq` when you need RabbitMQ monitoring and queue tooling.
- Use per-queue overrides when most queues fit one backend, but one queue needs different behavior.

## Quick setup

1. Choose a backend: `Database` or `RabbitMq`.
2. Configure the queue backend in your `appsettings` file.
3. Restart Starsky.

## Database backend example

Use database-backed queues for all queue keys:

```json
{
  "app": {
    "queue": {
      "default": "Database",
      "databasePollIntervalInMilliseconds": 500
    }
  }
}
```

## RabbitMQ backend example

Use RabbitMQ for all queue keys:

```json
{
  "app": {
    "queue": {
      "default": "RabbitMq",
      "rabbitMq": {
        "host": "localhost",
        "port": 5672,
        "username": "guest",
        "password": "guest",
        "virtualHost": "/"
      }
    }
  }
}
```

## Per-queue override (optional)

You can keep one global default and override specific queues.

Known queue keys:

- `Update`
- `Thumbnail`
- `DiskWatcher`
- `ImageClassification`

Example (default `Database`, but `ImageClassification` uses `RabbitMq`):

```json
{
  "app": {
    "queue": {
      "default": "Database",
      "queues": {
        "ImageClassification": "RabbitMq"
      },
      "rabbitMq": {
        "host": "localhost",
        "port": 5672,
        "username": "guest",
        "password": "guest",
        "virtualHost": "/"
      }
    }
  }
}
```

Each queue key uses one backend at a time. Changing backend does not migrate existing in-flight messages between backends.

## Environment variable override

If you deploy with containers or CI/CD, you can override the same settings by environment variables:

- `app__queue__default=Database`
- `app__queue__queues__imageclassification=RabbitMq`
- `app__queue__databasepollintervalinmilliseconds=500`
- `app__queue__default=RabbitMq`
- `app__queue__rabbitmq__host=localhost`
- `app__queue__rabbitmq__port=5672`
- `app__queue__rabbitmq__username=guest`
- `app__queue__rabbitmq__password=guest`
- `app__queue__rabbitmq__virtualhost=/`

## If your change is not picked up

If you changed queue backend settings but Starsky still behaves differently, check these points:

1. You edited the file used by your current run target.
2. Another file overrides your value (for example `appsettings.patch.json`).
3. Environment variables override your file values.
4. For `RabbitMq`: broker settings are valid and reachable.
5. For `Database`: database connectivity and permissions are valid.

Tip: check startup logs for lines like:

- `[QueueBackendFactory] Queue Update uses backend Database`
- `[QueueBackendFactory] Queue Update uses backend RabbitMq`

That confirms the active backend per queue.

## Roll back to in-memory queue

If you need to disable external queue backends quickly, set:

```json
{
  "app": {
    "queue": {
      "default": "InMemory"
    }
  }
}
```

Then restart Starsky.