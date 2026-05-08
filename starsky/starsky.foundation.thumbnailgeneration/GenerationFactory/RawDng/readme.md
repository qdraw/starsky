The RawDNG does decode the DNG file and then passes the decoded data to the next step in the
pipeline. It is used for generating thumbnails from DNG files, which are a type of raw image format.
The RawDNG class is responsible for handling the specific decoding process required for DNG files,
ensuring that the image data is correctly processed for thumbnail generation.
Forbidden to use embedded jpeg files in DNG files.
For using the embedded jpeg files, see EmbeddedRawThumbnail class.

# DNG Bit Depth and Compression Overview

| Bit Depth | Compression         | Typical File Size | Quality Impact | Editing Flexibility | Common Usage                  |
|-----------|---------------------|-------------------|----------------|---------------------|-------------------------------|
| 10-bit    | Uncompressed        | Medium            | None           | Moderate            | Smartphone RAW                |
| 10-bit    | Lossless Compressed | Small–Medium      | None           | Moderate            | Mobile photography            |
| 10-bit    | Lossy Compressed    | Small             | Slight         | Lower               | Computational photography     |
| 12-bit    | Uncompressed        | Large             | None           | High                | Mirrorless/DSLR RAW           |
| 12-bit    | Lossless Compressed | Medium            | None           | High                | Most modern RAW workflows     |
| 12-bit    | Lossy Compressed    | Small–Medium      | Slight         | Medium              | Cloud/mobile workflows        |
| 14-bit    | Uncompressed        | Very Large        | None           | Very High           | High-end photography          |
| 14-bit    | Lossless Compressed | Large             | None           | Very High           | Professional RAW editing      |
| 14-bit    | Lossy Compressed    | Medium            | Slight         | High                | Space-saving archival         |
| 16-bit    | Uncompressed        | Huge              | None           | Extremely High      | Linear/HDR intermediate files |
| 16-bit    | Lossless Compressed | Very Large        | None           | Extremely High      | HDR merges, advanced editing  |
| 16-bit    | Lossy Compressed    | Large             | Slight         | High                | Specialized workflows         |

## Practical differences

| Combination         | What it feels like in practice                           |
|---------------------|----------------------------------------------------------|
| 10-bit lossy        | Small and fast, but shadows break apart sooner           |
| 12-bit lossless     | Sweet spot for most photographers                        |
| 14-bit lossless     | Maximum RAW recovery and grading headroom                |
| 14-bit uncompressed | Huge files with minimal real-world benefit over lossless |
| 16-bit linear       | More like an editing intermediate than true camera RAW   |

# Need to understand this:

| Area               | Examples                                | Required?         |
|--------------------|-----------------------------------------|-------------------|
| Bit depth          | 10/12/14/16-bit                         | Yes               |
| Compression        | Uncompressed, lossless JPEG, lossy JPEG | Yes               |
| Packing layout     | Packed bits vs padded words             | Yes               |
| Endianness         | Little-endian / big-endian              | Yes               |
| CFA pattern        | RGGB, BGGR, GBRG, X-Trans               | Usually           |
| Linear vs mosaic   | Linear DNG vs true RAW                  | Yes               |
| Tile/strip layout  | TIFF strips or tiles                    | Yes               |
| Predictor decoding | JPEG predictors in lossless compression | Often             |
| Black/white levels | Sensor calibration                      | Important         |
| Color matrices     | Camera → XYZ/RGB conversion             | Important         |
| Opcode processing  | Lens correction, gain maps              | Optional/advanced |
