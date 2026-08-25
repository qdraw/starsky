---
sidebar_position: 20
---

# Kubernetes

This guide describes how to deploy Starsky on Kubernetes.

## Security context

The Starsky image remaps the `app` user to **uid/gid 1000** at build time, so it
aligns with the typical ownership of `hostPath` volumes on Linux nodes (usually
owned by the `ubuntu` user at uid 1000).

Set the pod security context accordingly:

```yaml
securityContext:
  runAsUser: 1000
  runAsGroup: 1000
  fsGroup: 1000
```

> **Background:** the `mcr.microsoft.com/dotnet/aspnet` base image ships `app`
> at uid 1654 and `ubuntu` at uid 1000. The Dockerfile removes `ubuntu` and
> remaps `app` to uid 1000 so that both `/app/temp` (owned by `app` in the
> image) and host-mounted volumes (owned by uid 1000 on the node) are accessible
> with a single `runAsUser`. `fsGroup` has no effect on `hostPath` mounts, so
> matching `runAsUser` to the node's file ownership is the only reliable approach.

## Example Deployment

```yaml
---
kind: Service
apiVersion: v1
metadata:
  name: starsky
  namespace: applications
spec:
  ports:
    - port: 80
      targetPort: 8080
  selector:
    k8s-app: starsky
---
kind: Deployment
apiVersion: apps/v1
metadata:
  name: starsky
  namespace: applications
spec:
  replicas: 1
  selector:
    matchLabels:
      k8s-app: starsky
  template:
    metadata:
      labels:
        k8s-app: starsky
        app: starsky
    spec:
      securityContext:
        runAsUser: 1654
        runAsGroup: 1654
        fsGroup: 1654
      containers:
        - name: starsky
          image: ghcr.io/qdraw/starsky:latest
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
            - name: app__storageFolder
              value: "/mnt/photos"
            - name: app__ThumbnailTempFolder
              value: "/mnt/thumbnails"
            - name: app__databaseType
              value: "mysql"
            - name: app__databaseConnection
              valueFrom:
                secretKeyRef:
                  name: starsky-secrets
                  key: connectionstring
          livenessProbe:
            httpGet:
              path: /api/health
              port: 8080
            initialDelaySeconds: 30
            timeoutSeconds: 10
            periodSeconds: 60
            failureThreshold: 5
          readinessProbe:
            httpGet:
              path: /api/health
              port: 8080
            initialDelaySeconds: 15
            timeoutSeconds: 10
            periodSeconds: 60
            failureThreshold: 5
          resources:
            requests:
              memory: "512Mi"
              cpu: "100m"
            limits:
              memory: "2Gi"
              cpu: "500m"
          volumeMounts:
            - name: photos
              mountPath: /mnt/photos
            - name: thumbnails
              mountPath: /mnt/thumbnails
      volumes:
        - name: photos
          hostPath:
            path: /mnt/photos
            type: DirectoryOrCreate
        - name: thumbnails
          hostPath:
            path: /mnt/thumbnails
            type: DirectoryOrCreate
```

## Troubleshooting

### IOException when uploading files

If you see errors like `[CreateDirectory] IOException caught, /app/temp/stream_...`
or `DirectoryNotFoundException` in the logs, the process does not have write
permission on `/app/temp`.

Cause: the pod security context `runAsUser` does not match uid 1000 (the uid
`app` is remapped to in the Starsky image).

Fix: set `runAsUser: 1000`, `runAsGroup: 1000` in the pod `securityContext` as
shown above.

### UnauthorizedAccessException on mounted volumes

If you see `Access to the path '/mnt/...' is denied` from `DiskWatcher` or sync,
the process uid does not match the ownership of the `hostPath` directories on
the node.

`fsGroup` does **not** apply to `hostPath` volumes — matching `runAsUser` to the
node's file owner is the only reliable fix. Ensure the host paths are owned by
uid 1000, or adjust `runAsUser` to whichever uid owns them.

### groups: cannot find name for group ID NNN

This warning appears when `fsGroup` is set to a gid that has no name in the
container's `/etc/group`. It is cosmetic — the kernel still applies the group
numerically — but it indicates the gid does not exist in the image. Use
`fsGroup: 1000` to match the remapped `app` group.
