[< readme](readme.md)

# Docker instructions

One of the build options is to run Starsky from a docker container

```sh
cd starsky
```

## Build from Starsky folder

```sh
docker build -t starsky .
```

## run
```sh
docker run -it --rm -p 8000:80 starsky
```

## list of active dockers
```sh
docker ps
```

```sh
docker exec -it d8094eb990de /bin/bash
```

## And check if runs
```sh
curl  curl http://localhost:8000/api/health -X GET
```
