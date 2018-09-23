# Starsky
## List of [Starsky](../readme.md) Projects
 * __[inotify-settings](../inotify-settings/readme.md) to setup auto indexing on linux__
 * [starsky (sln)](../starsky/readme.md) _database photo index & import index project_
    * [starsky](../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskysynccli](../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyTests](../starsky/starskyTests/readme.md)  _mstest unit tests_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
 * _[starsky-node-client](../starsky-node-client/readme.md) nodejs tools to add-on tasks_
 * _[starskyapp](../starskyapp/readme.md) _React-Native app (Pre-alpha code)_


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



#Cron Tab


sudo nano /usr/local/sbin/starskyhourly.sh
	''''
	copy content from => starskyhourly.sh
	''''

sudo chmod +x /usr/local/sbin/starskyhourly.sh


crontab -e
0       *            *       *       *       /usr/local/sbin/starskyhourly.sh > /home/pi/z-starskycli.log 2>&1
