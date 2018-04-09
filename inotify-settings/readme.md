# inotifywait

## Currently only for .jpg or .JPG files

First install tools
$ sudo apt-get install inotify-tools


sudo nano /usr/local/sbin/inotify.sh
	''''
	copy content from => inotify.sh
	''''
sudo chmod ug+x /usr/local/sbin/inotify.sh

sudo nano /usr/local/etc/inotify.d/starsky.conf
	''''
	copy content from => starsky.conf
	''''

sudo nano /etc/init.d/inotifyd
	''''
	copy content from => inotifyd.sh
	''''

sudo chmod +x /etc/init.d/inotifyd

sudo update-rc.d -f inotifyd defaults

sudo service inotifyd start

sudo service inotifyd restart

Source:
http://wiki.lenux.org/using-csync2-with-inotifywait/
