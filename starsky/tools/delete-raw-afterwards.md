# When adding raw files back to the directory and remove the jpg files afterwards
1) First select the good images using Starsky, and delete the bad ones.
2) Remove some jpg files from folder
3) When I'm back from holiday I add raws to folder
4) Remove raw files where the jpg is missing from.

### For lowercase files
```sh
for f in *.arw
do
    JPGFILE=${f/arw/jpg}

    if [ ! -f "$JPGFILE" ]; then
        echo $JPGFILE

        rm $f
        XMPFILE=${f/arw/xmp}

        if [ -f "$XMPFILE" ]; then
            echo $XMPFILE
            rm $XMPFILE
        fi
    fi
done
```

### For uppercase files
```sh
for f in *.ARW
do
    # echo $f

    JPGFILE=${f/ARW/JPG}

    if [ ! -f "$JPGFILE" ]; then
        echo $JPGFILE

        rm $f
        XMPFILE=${f/ARW/xmp}

        if [ -f "$XMPFILE" ]; then
            echo $XMPFILE
            rm $XMPFILE
        fi
    fi
done
```
