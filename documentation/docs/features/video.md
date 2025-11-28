# Video Support

Starsky provides comprehensive support for video files, including playback, thumbnail generation, and metadata management. Since version 0.7.0, video thumbnails are automatically generated for quick preview and browsing.

## 🎥 Video Features

### Supported Formats

Starsky can handle various video formats, though browser playback support depends on your browser and operating system:

- **Common Formats** - MP4, MOV, AVI, MKV, and more
- **Codec Support** - H.264, H.265/HEVC, VP9, and others
- **Audio Formats** - Are not supported yet

---

## 🖼️ Video Thumbnails

### Automatic Thumbnail Generation

Since version 0.7.0-beta.0, Starsky automatically generates thumbnails for video files:

- **FFmpeg-powered** - Uses FFmpeg to extract preview frames from videos
- **WebP Format** - Video thumbnails are generated in WebP format by default (since v0.7.0)
- **Fast Browsing** - Preview videos without loading the full file
- **Grid View** - Video thumbnails appear alongside photo thumbnails

### Native Thumbnails

Starsky can also use platform-native thumbnail generation:

- **macOS** - Uses QuickLook for video preview generation
- **Windows** - Leverages Windows Shell for video thumbnails
- **Better Compatibility** - Native previews support more video formats and codecs

---

## ▶️ Video Playback

### In-Browser Playback

Watch videos directly in the Starsky interface:

- **Built-in Player** - View videos without external software
- **Browser-dependent** - Playback depends on browser codec support
- **Streaming** - Videos are streamed from your library
- **Full Metadata** - View video metadata alongside playback

### Video Metadata

Starsky reads and manages metadata for video files:

- **DateTime** - When the video was recorded
- **Duration** - Video length
- **Resolution** - Video dimensions (width × height)
- **GPS Location** - Where the video was recorded (if available)
- **Tags and Description** - Add keywords and descriptions to videos
- **ColorClass** - Rate and organize videos like photos


## 🔧 Technical Details

### FFmpeg Integration

Starsky uses FFmpeg for video processing:

- **Thumbnail Extraction** - Captures frames for preview thumbnails
- **Format Support** - Handles a wide range of video codecs and containers
- **Background Processing** - Thumbnails are generated asynchronously
- **Quality Optimization** - Balances thumbnail quality and file size

## 📊 Video in Collections

Videos integrate seamlessly with your photo library:

- **Mixed Collections** - Videos and photos appear together in folders
- **Search by Metadata** - Find videos using tags, dates, and locations
- **Stacks** - Group related videos and photos together
- **Bulk Editing** - Update metadata for multiple videos at once
- **Export** - Include videos in zip exports

---

## 🗂️ Related Features

- **[Import](import.md)** - Import videos from cameras and devices
- **[Metadata](metadata.md)** - Manage video metadata
- **[Search](search.md)** - Search for videos by metadata
- **[Export](export.md)** - Download and share videos
- **[Geo Features](geo.md)** - GPS location data in videos
