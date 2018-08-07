1. Add to FileIndexItem
2. Run Migrations
```sh
dotnet ef migrations add ExifOrientation
```
3.  Apply Migration to database
	You could do this using the command line or running the mvc application

4.  Add Read Method to `ExifRead`, this is for tiff-based images, jpeg's, png's, bitmaps and gif's
