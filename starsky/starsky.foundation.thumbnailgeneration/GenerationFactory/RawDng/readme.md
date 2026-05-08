The RawDNG does decode the DNG file and then passes the decoded data to the next step in the
pipeline. It is used for generating thumbnails from DNG files, which are a type of raw image format.
The RawDNG class is responsible for handling the specific decoding process required for DNG files,
ensuring that the image data is correctly processed for thumbnail generation.
Forbidden to use embedded jpeg files in DNG files.
For using the embedded jpeg files, see EmbeddedRawThumbnail class.