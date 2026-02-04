

find mydir -type f -exec curl -u xxx:psw --ftp-create-dirs -T {} ftp://192.168.1.158/public/demon_test/{} \;