
# env
```sh
eval "$(docker-machine env default)"
```

# build from starky folder

```sh
docker build -t starsky .
```

# run
```sh
docker run -it --rm -p 8000:80 starsky
```

# list of active dockers
```sh
docker ps
```

```sh
docker exec -it 7d9e4970f52f /bin/bash
```
