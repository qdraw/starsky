# Metadata usage

Starsky uses iptc and xmp metadata to read and write. 
So when you remove the database a single rescan will restore the metadata since it is stored inside the images.

When you use other tools to edit XMP or IPTC metadata, Starsky will able to read the changes and display them in the application.


![Meta data usage](../assets/metadata_usage_v0410.jpg)
_Left Exiftool viewing image and right Starsky with the same info_

## XMP Metadata

Starsky uses XMP metadata to read and write.