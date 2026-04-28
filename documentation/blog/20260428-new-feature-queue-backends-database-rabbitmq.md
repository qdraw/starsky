---
slug: new-feature-queue-backends-database-rabbitmq
title: "New feature: queue backends with Database and RabbitMq"
authors: dion
tags: [photo management, queue, infrastructure]
date: 2026-04-28
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: queue backends with Database and RabbitMq

Starsky now supports configurable queue backends per queue key. You can use `Database` or `RabbitMq` as transfer backend, and keep different backends per workload when needed.

<!-- truncate -->

## What changed

Background processing is no longer limited to one queue strategy.

You can now configure:

- `default` backend for all queues
- per-queue overrides for specific jobs
- shared RabbitMQ connection settings

This gives you more control for single-instance setups and multi-instance deployments.

## Available backends

- `InMemory`: fastest and simplest in a single process
- `Database`: persistent queueing through your Starsky database
- `RabbitMq`: dedicated message broker for distributed workloads

## When to choose Database vs RabbitMq

Choose `Database` when:

- you already run a stable Starsky database
- you want fewer external services
- you prefer simpler operations

Choose `RabbitMq` when:

- you run multiple app instances
- you want dedicated queue infrastructure
- you want RabbitMQ monitoring and queue tooling

## Example configuration

Use `Database` as the global default and move one queue to `RabbitMq`:

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

## Important note

Each queue key uses one backend at a time. Switching backend does not migrate in-flight messages between backends.

## Learn more

Read the full end-user guide here:

- [Queue backends (RabbitMQ and Database)](../docs/features/rabbitmq-queue-backend)

