# Geo Features

Starsky provides comprehensive geolocation features to help you manage and visualize where your photos were taken. From storing GPS coordinates to syncing location data from GPX files and viewing maps, Starsky makes it easy to organize your photos by location.

## 📍 Overview

The geo features in Starsky include:

- **GPS Metadata Storage** - Store latitude, longitude, and altitude in your photos
- **GPX File Support** - Import location tracks from GPS devices and mobile apps
- **Reverse Geocoding** - Automatically convert coordinates to city, state, and country names
- **Map Visualization** - View GPX tracks and photo locations on interactive maps
- **Location Editing** - Manually edit location data in the detail view
- **API Endpoints** - Programmatic access to geo sync and lookup features

---

## 🗺️ GPS Metadata

Starsky stores GPS information in your photos using IPTC and XMP metadata standards, ensuring compatibility with other photo management tools.

### Supported GPS Fields

- **GPS Latitude and Longitude** - The exact coordinates where the photo was taken
- **GPS Altitude** - Elevation above sea level
- **Location City** - The city where the photo was taken
- **Location State** - The state or province (optional since v0.7.4)
- **Location Country** - The country name
- **Location Country Code** - ISO 3166-1 alpha-2 country code

All this information is stored directly in the image file metadata, so even if you remove the database, a rescan will restore the location information.

---

## 📲 GPX File Support

GPX (GPS Exchange Format) files contain location tracks recorded by GPS devices, fitness apps, or mobile tracking applications. Starsky can use these files to automatically add location data to photos.

### How GPX Sync Works

1. **Record Your Location** - Use a sports app, fitness tracker, or GPS device to record your location while taking photos
2. **Export GPX File** - Export the location track as a GPX file
3. **Import to Starsky** - Place the GPX file in the same folder as your photos
4. **Sync Locations** - Use the geo sync feature to match photo timestamps with GPS track points

The sync process matches the date and time of each photo with the corresponding location in the GPX track. **Important:** Your camera clock must be accurate for the locations to match correctly.

### Viewing GPX Files

Starsky can display GPX files with an interactive map powered by Leaflet and OpenStreetMap. When viewing a GPX file:

- See the complete track on an interactive map
- Zoom in/out to explore the route (GPX mode only)
- Use the unlock button to pan around the map
- View current location markers
- Enable touch zoom and double-click zoom when unlocked

---

## 🌐 Reverse Geocoding

Reverse geocoding converts GPS coordinates into human-readable location names (city, state, country). This feature runs automatically during import or can be triggered manually.

### Automatic Reverse Geocoding

Since version 0.7.0-beta.1, Starsky can automatically perform reverse geocoding during the import process. This means:

- Photos with GPS coordinates are automatically enriched with location names
- City, state, and country fields are populated without manual intervention
- Works seamlessly with the import workflow

### Manual Reverse Geocoding

You can also trigger reverse geocoding manually:

- Through the web interface's detail view
- Using API endpoints (`/api/geo/sync` or `/api/geo-reverse-lookup`)
- Via command-line tools

### Data Source

Starsky uses offline geocoding data files for reverse geocoding, which are downloaded automatically at startup (unless disabled via `GeoFilesSkipDownloadOnStartup`). This allows geocoding to work without requiring external API calls.

---

## 🖼️ Location in Detail View

When viewing a photo's details, you can:

- **View GPS Coordinates** - See the exact latitude, longitude, and altitude
- **View Location Names** - See the city, state, and country
- **Edit Location Data** - Manually update coordinates or location names
- **Trigger Geo Sync** - Initiate reverse geocoding for the current photo

The detail view provides a convenient way to verify and correct location information for individual photos.

---

## 🔧 API Endpoints

Starsky provides several API endpoints for geo operations:

### `/api/geo/status` (GET)
Get the current geo sync status - useful for monitoring long-running sync operations.

### `/api/geo/sync` (POST)
Perform reverse lookup for geo information and/or add geo location based on a GPX file. This is the main endpoint for triggering geo sync operations.

### `/api/geo-reverse-lookup` (GET)
Perform reverse geo lookup for specific coordinates. Returns city, state, and country information.

---

## ⚙️ Configuration

### GeoFilesSkipDownloadOnStartup

By default, Starsky downloads geo-related files on startup. You can disable this behavior with the `GeoFilesSkipDownloadOnStartup` configuration flag.

**Recommended setting:** `false` (default)

This ensures that geo data files are available when needed.

---

## 🚀 Use Cases

### Camera Without GPS

Many DSLR and mirrorless cameras don't have built-in GPS. Use Starsky's GPX sync feature to:

1. Track your location with a mobile app while shooting
2. Export the GPX file
3. Let Starsky automatically match timestamps and add locations

### Organizing by Location

With location metadata properly tagged:

- Search for photos by city or country
- Create albums based on travel destinations
- Filter photos by geographic region
- Export location data with your photos

### Privacy Control

Location data is stored in the image metadata, giving you complete control. When publishing photos, you can:

- Choose to strip location metadata
- Keep GPS data private while sharing
- Maintain your own copy with full location details

---

## 📱 Mobile Workflow

The typical mobile workflow for adding GPS data:

1. Use a fitness or tracking app (Strava, Runkeeper, etc.) on your phone
2. Record your activity while taking photos
3. Export the GPX track from the app
4. Import photos and GPX file together in Starsky
5. Run geo sync to automatically add locations

This workflow is especially useful for hiking, travel photography, or any situation where your camera lacks GPS.

---

## 🗂️ Related Features

- **[Metadata Management](metadata.md)** - Learn more about metadata storage in Starsky
- **[Import Options](import.md)** - Configure automatic reverse geocoding during import
- **[Search](search.md)** - Search photos by location
- **[Bulk Editing](bulk-editing.md)** - Update location data for multiple photos at once

---

## 🔍 Technical Details

- Uses IPTC and XMP metadata standards
- Compatible with other photo management tools
- Location data stored directly in image files
- OpenStreetMap integration for maps and geocoding
- Leaflet-powered interactive map viewer
- Timestamp-based GPX track matching
- ISO 3166-1 alpha-2 country code support

---

## 📚 Additional Resources

- [API Documentation](../developer-guide/api/readme.md)
- [History & Changelog](../advanced-options/history.md)
- Blog: [7 Things I Missed When Managing My Photo Collection](https://docs.qdraw.nl/blog/the-7-things-i-missed-when-managing-my-photo-collection)
