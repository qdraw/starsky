[< readme](readme.md)

# Docker instructions

One of the build options is to run Starsky from a docker container

### Using docker compose

1. To get started clone the repository

```sh
git clone "https://github.com/qdraw/starsky.git"
```

2. Install Docker Desktop
   https://www.docker.com/products/docker-desktop

3. Go to the child directory
```sh
cd starsky
```

4. Build with docker
```sh
docker compose build
```

5. Enable containers
```sh
docker compose up
```

6. Check if succeed
```sh
curl http://localhost:12837/api/health -X GET
```

### Using classic docker CLI
1. To get started clone the repository

```sh
git clone "https://github.com/qdraw/starsky.git"
```

2. Install Docker Desktop or the docker-cli
   https://www.docker.com/products/docker-desktop

3. Go to the child directory
```sh
cd starsky
```

4. Build project
```sh
export DOCKER_BUILDKIT=0
docker build -t starsky .
```

    ### Optional: Build Starsky with demo user and demo content
    ```sh
    docker build -t starsky . --build-arg ISDEMO=true
    ```

5. Run project

```sh
docker run -it --rm -p 8000:80 starsky
```

6. list of active dockers
```sh
docker ps
```

```sh
docker exec -it d8094eb990de /bin/bash
```

7. And check if it runs
```sh
curl http://localhost:8000/api/health -X GET
```
