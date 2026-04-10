1. Add to FileIndexItem
2. Run Migrations
    ```sh
    dotnet ef migrations add ExifOrientation
    ```
3. Apply Migration to database
   You could do this using the command line or running the mvc application

4. **For syncing the files**
5. Add Read Method to `ExifRead`, this is for tiff-based images, jpeg's, png's, bitmaps and gif's
   This is using the https://github.com/drewnoakes/metadata-extractor-dotnet

6. Implement sitecar .xmp file reading `XmpReadHelper`. This is based on
   XMPCore https://github.com/drewnoakes/xmp-core-dotnet

7. **For updating files**
8. Check the ApiController for update, and add the feature
9. In `ExifTool` add the `Update` and `Info`
   Update is used to write and Info is read the write data realtime

10. Implement feature on the GUI
