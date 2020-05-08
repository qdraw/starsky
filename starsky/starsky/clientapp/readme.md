# Starsky ClientApp
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
    * [starskyAdminCli](../../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [Starsky Business Logic](../../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## starsky/starsky/clientapp docs

On top of the Starsky API there is React front-end

## Project structure

```
•
└── src
    └── components
    |   └── atoms
    |   |    |    Atoms are the basic building blocks of matter.
    |   |    └── button-styled
    |   |       ├── button-styled.tsx
    |   |       └── button-styled.stories.tsx
    |   |       └── button-styled.spec.tsx
    |   |      Folder has .tsx for component
    |   |      Folder has stories.tsx for storybook
    |   |      Folder has spec.tsx for unit-tests
    |   |         
    |   └── molecules
    |   |     Molecules are groups of atoms bonded together
    |   └── organisms
    |         Molecules are building blocks 
    └── containers
    |     large containers that contain parts of the UI
    └── contexts
    |   └── archive-context
    |   |     React Context for lists of files
    |   └── detailview-context
    |         React Context for viewing a detailed file
    └── contexts-wrapper
    |      Combine contexts with input from the API
    └── hooks
    |      React hooks to handle API/Keyboard calls
    └── interfaces
    |      Typescript interfaces
    └── pages
    |      Full pages
    └── routes
    |      Routes based on the browser url
    └── shared
    |      Business logic that is shared between multiple components
    └── style
           CSS Styling
```

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.<br>
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br>
You will also see any lint errors in the console.

Make sure you run the Starsky API on http://localhost:5000 or us a localtunnel proxy (which you can find in `./starsky-tools`)

### `npm test`

Launches the test runner in the interactive watch mode.<br>
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `npm test:ci`

Run all unittests and check if there are any errors

### `npm run build`

Builds the app for production to the `build` folder.<br>
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br>
Your app is ready to be deployed!

In this application we use the `build.sh` or `build.ps1` to combine the frontend application with the .NET Core backend.

### `npm run storybook`

Storybook is an open source tool for developing UI components in isolation for React and other platfoms. It makes building stunning UIs organized and efficient.

A Storybook is a collection of stories. Each story represents a single visual state of a component.
    Technically, a story is a function that returns something that can be rendered to screen.

```
•
└── src
    └── components
        └── atoms
            └── button-styled
                ├── button-styled.tsx
                └── button-styled.stories.tsx
```

### Upgrade `Create React App` to a newer version

The default Create React App package is used to keep future upgrades less painfull.

#### To keep all CRA dependencies in place 

```
npx create-react-app my-app --template typescript
```

- copy the `package.json` + `package-lock.json` from the `my-app` folder


The following packages are added:
```
npm install --save abortcontroller-polyfill
npm install --save @reach/router
npm install --save intersection-observer
npm install --save @types/reach__router
npm install --save leaflet
npm install --save @types/leaflet
npm install --save @types/storybook__react
npm install --save enzyme
npm install --save @types/enzyme
npm install --save enzyme-adapter-react-16
npm install --save-dev @storybook/react
npm install --save-dev @storybook/preset-create-react-app
```

#### Proxy tag for backend services:
```json
 "name": "clientapp",
```

```json
"proxy": "http://localhost:5000",
```

#### Homepage
```json
"homepage": "/starsky/",
```

#### `npm run test:ci` is used by the build-script to run all tests and ESlint
This is added to the `package.json`

```json
"lint": "node node_modules/eslint/bin/eslint.js \"src/**\" --max-warnings 0",
"test:ci": "npm run lint && react-scripts test --watchAll=false --coverage --reporters=default 2>&1",
"storybook": "start-storybook",
"upgrade": "echo 'check readme.md 20200406 3.4.1 (2020-03-20)'"
```

### collectCoverageFrom and coverageReporters
With jest `collectCoverageFrom` and `coverageReporters` are used to get the right output

```json
"jest": {
  "collectCoverageFrom": [
    "**/*.{ts,tsx}",
    "!coverage/**",
    "!**/*.stories.{ts,tsx}",
    "!node_modules/**",
    "!src/index.*ts*",
    "!src/service-worker.ts",
    "!src/react-app-env.d.ts",
    "!src/setupTests.js",
    "!public/**",
    "!build/**"
  ],
  "coverageReporters": [
    "text",
    "lcov",
    "json",
    "cobertura"
  ],
  "coverageThreshold": {
    "global": {
      "branches": 65,
      "functions": 70,
      "lines": 78,
      "statements": 75
    }
  }
},
```

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).
