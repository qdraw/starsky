# Export and Download

Starsky provides flexible options for exporting and downloading your photos, whether you need individual files or bulk collections. The export feature allows you to create zip archives of selected photos for easy sharing or backup.

## 📦 Export Features

### Create Zip Archives

Export multiple photos at once by creating zip archives:

- **Select Multiple Files** - Choose which photos to include in the export
- **Collections Support** - Include files with the same name (different extensions/formats)
- **Thumbnail Export** - Option to export thumbnails instead of full-resolution images
- **Bulk Download** - Download entire folders or search results

### Export Options

- **Original Files** - Export full-resolution original images
- **With Thumbnails** - Include smaller thumbnail versions for preview
- **Collections Mode** - Automatically include related files (RAW + JPEG pairs, etc.)

---

## 📥 Download Options

### Individual File Downloads

- **Download Original** - Get the full-resolution original file
- **Download Thumbnail** - Download a smaller 1000px thumbnail (if supported)
- **Download Sidecar** - Download XMP sidecar files (e.g., `image.xmp`)
- **Cache Control** - Optional client-side caching for faster subsequent downloads

### Download from Search Results

Export photos directly from search results:

1. Perform a search to find specific photos
2. Select the images you want to export
3. Create a zip archive
4. Download the generated zip file

---

## 💡 Use Cases

### Sharing Photos

- Create a zip of selected vacation photos to share with family
- Export thumbnails for quick preview sharing
- Include metadata via sidecar files

### Backup Selected Images

- Export high-priority images marked with ColorClass ratings
- Create periodic backups of specific folders
- Download originals with all metadata intact

### Publishing Workflow

- Export reduced-size thumbnails for web use
- Download originals for external editing
- Maintain collections of RAW + JPEG pairs

### Client Delivery

- Create zip archives of edited photos for clients
- Include watermarked versions via the publish feature
- Export with or without location metadata for privacy

---

## ⚙️ Technical Details

- Zip archives are created server-side for optimal performance
- Large exports are handled asynchronously
- Cache headers support for efficient repeated downloads
- Maintains original file structure and naming
- Preserves all metadata in downloaded files
- Supports all file formats (images, videos, RAW files, XMP sidecars)

---

## 🗂️ Related Features

- **[Publish with Watermarks](webhtmlpublish.md)** - Export and publish with watermarks
- **[Bulk Editing](bulk-editing.md)** - Edit metadata before exporting
- **[Search](search.md)** - Find photos to export
- **[Stacks](stacks.md)** - Export stacked files as collections
- **[Metadata](metadata.md)** - Metadata is preserved in exports

---

## 📝 Notes

- Exported files retain all IPTC and XMP metadata
- Zip creation may take time for large selections
- Check export status before downloading
- Some file formats may have size limitations
- GPX files are included in exports (not ignored)
