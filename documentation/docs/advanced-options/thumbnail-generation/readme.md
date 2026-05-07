---
sidebar_position: 15
---

# Thumbnail logic

This component contains the thumbnail-generation logic used by Starsky. It provides a single service-oriented entrypoint (`IThumbnailService`) and a small CLI helper (`ThumbnailCli`) to generate thumbnails for images, RAW files and videos.

General concept
The thumbnail pipeline follows a few simple rules that make it robust and efficient across many source types:

- Preflight: before doing work the pipeline checks which sizes already exist and which must be generated. This avoids unnecessary processing.
- Largest-first: the pipeline generates the largest requested thumbnail from the best available source, then derives smaller sizes from that resulting image. This preserves quality and reduces repeated decoding of the original source.
- Pluggable generators: each file type is handled by a specialized generator. Generators are implemented behind a common `IThumbnailGenerator` interface and the `ThumbnailGeneratorFactory` chooses which generator(s) to use for a given file.
- Composite strategy: for some extensions (notably JPEGs) the factory will attempt multiple generators in order (embedded RAW preview → native preview → ImageSharp) so the best/fastest method is used.
- File-hash based output: generated thumbnails are stored/retrieved using a file-hash based naming scheme. The service resolves or requires a `fileHash` to operate and many helpers rely on that to find or stream thumbnails.
- Error reporting and status: each target size produces a `GenerationResultModel` describing success, failure, warnings and messages; status updates are collected and persisted using the update-status service.

Key concepts
- ThumbnailService (`GenerationFactory/Interfaces/IThumbnailService.cs`)
    - Public API for generating thumbnails for a folder or a single file.
    - Supports: bulk generation, single file generation, streaming a single thumbnail, and rotating a thumbnail.
- Generator factory (`GenerationFactory/ThumbnailGeneratorFactory.cs`)
    - Chooses a generator based on file extension and capabilities:
        - ImageSharp generator (Image files supported by ImageSharp)
        - FfmpegVideo generator (video thumbnails)
        - Native preview generator (platform-specific extractors)
        - Embedded RAW preview extractor (pulls JPEG previews embedded in RAW containers)
    - The factory returns a composite generator in some cases so multiple strategies can be attempted in order.
- Shared pipeline
    - Common logic to preflight (skip already existing sizes), generate the largest needed image, then generate smaller sizes from that thumbnail.
    - Error/result reporting uses `GenerationResultModel` to express per-size success/failure and messages.

Public API (high level)
- IThumbnailService (mainly used by the rest of Starsky)
    - `Task<List<GenerationResultModel>>` GenerateThumbnail(string fileOrFolderPath, ThumbnailGenerationType type = ThumbnailGenerationType.All)
        - Generate thumbnails for a folder or a single file path. The path is the project's "subPath" storage representation (see `SelectorStorage`).
    - `Task<List<GenerationResultModel>>` GenerateThumbnail(string subPath, string fileHash, ThumbnailGenerationType type = ThumbnailGenerationType.All)
        - Generate thumbnails for a single file where `fileHash` is already known.
    - `Task<(Stream?, GenerationResultModel)>` GenerateThumbnail(string subPath, string fileHash, ThumbnailImageFormat imageFormat, ThumbnailSize size)
        - Return a Stream for a single thumbnail size (useful for streaming a response to clients).
    - `Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000, int height = 0)`
        - Rotate / regenerate thumbnail image for a given file hash.

Implementation notes
- Pluggable generators
    - ImageSharp generator (`ImageSharpThumbnailGenerator`) uses ImageSharp to load and resize images.
    - Ffmpeg video generator extracts a frame via the video pipeline (`starsky.foundation.video`) and then resizes it with the ImageSharp helpers.
    - Native preview generator integrates with native preview extraction (via `starsky.foundation.native` / `IPreviewImageNativeService`) when available.
    - Embedded RAW preview extraction extracts embedded JPEG previews from RAW containers (see `raw-embedded-preview-extraction.md` for deep details).
- Resizing flow
    - The pipeline first preflights which sizes need to be generated (avoids work if thumbnails already exist).
    - The largest target size is generated from source and smaller sizes are produced from the generated large thumbnail to preserve quality and reduce CPU.




