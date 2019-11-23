# Starsky
## List of [Starsky](../../../readme.md) Projects
 * [inotify-settings](../../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../../starsky/starsky/readme.md) _web api application / interface_
      *  __[clientapp](../../../starsky/starsky/clientapp/readme.md) react front-end application__
    * [starskySyncCli](../../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyCore](../../../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyGeoCore](../../../starsky/starskygeocore/readme.md) _business geolocation logic (netstandard 2.0)_
    * [starskyTest](../../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starsky/clientapp docs

On top of the Starsky API there is React front-end

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.<br>
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br>
You will also see any lint errors in the console.

Make sure you run the Starsky API on http://localhost:5000 or us a localtunnel proxy (which you can find in `starsky-tools`)

### `npm test`

Launches the test runner in the interactive watch mode.<br>
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `npm run build`

Builds the app for production to the `build` folder.<br>
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br>
Your app is ready to be deployed!

In this application we use the `build.sh` or `build.ps1` to combine the frontend application with the .NET Core backend.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).
