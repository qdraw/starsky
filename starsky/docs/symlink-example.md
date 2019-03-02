# Example symlinks
For Mac OS and Linux

## Make sure the files are executable
```sh
chmod +x starsky
chmod +x starskygeocli
chmod +x starskysynccli
chmod +x starskyimportercli
chmod +x starskywebhtmlcli
```

## Link from the current folder to `/opt/bin`
```sh
ln -s /data/git/starsky/starsky/osx.10.12-x64/starskygeocli /opt/bin/starskygeocli
ln -s /data/git/starsky/starsky/osx.10.12-x64/starsky /opt/bin/starsky
ln -s /data/git/starsky/starsky/osx.10.12-x64/starskysynccli /opt/bin/starskysynccli
ln -s /data/git/starsky/starsky/osx.10.12-x64/starskyimportercli /opt/bin/starskyimportercli
ln -s /data/git/starsky/starsky/osx.10.12-x64/starskywebhtmlcli /opt/bin/starskywebhtmlcli
```

```sh
ln -s /home/pi/starsky/starskygeocli /opt/bin/starskygeocli
ln -s /home/pi/starsky/starsky /opt/bin/starsky
ln -s /home/pi/starsky/starskysynccli /opt/bin/starskysynccli
ln -s /home/pi/starsky/starskyimportercli /opt/bin/starskyimportercli
ln -s /home/pi/starsky/starskywebhtmlcli /opt/bin/starskywebhtmlcli
```
## Dot profile
```sh
nano .profile
```
```sh
export PATH="/opt/bin:$PATH"
```
