# Starsky ClientApp

## List of [Starsky](../../../readme.md) Projects

- [starsky (sln)](../../../starsky/readme.md) _database photo index & import index project_
  - [starsky](../../../starsky/starsky/readme.md) _web api application / interface_
    - **[clientapp](../../../starsky/starsky/clientapp/readme.md) react front-end application**
  - [starskyImporterCli](../../../starsky/starskyimportercli/readme.md) _import command line interface_
  - [starskyGeoCli](../../../starsky/starskygeocli/readme.md) _gpx sync and reverse 'geo tagging'_
  - [starskyWebHtmlCli](../../../starsky/starskywebhtmlcli/readme.md) _publish web images to a content package_
  - [starskyWebFtpCli](../../../starsky/starskywebftpcli/readme.md) _copy a content package to a ftp service_
  - [starskyAdminCli](../../../starsky/starskyadmincli/readme.md) _manage user accounts_
  - [starskySynchronizeCli](../../../starsky/starskysynchronizecli/readme.md) _check if disk changes are updated in the database_
  - [starskyThumbnailCli](../../../starsky/starskythumbnailcli/readme.md) _speed web performance by generating smaller images_
  - [Starsky Business Logic](../../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
  - [starskyTest](../../../starsky/starskytest/readme.md) _mstest unit tests_
- [starsky.netframework](../../../starsky.netframework/readme.md) _Client for older machines (deprecated)_
- [starsky-tools](../../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
- [starskyapp](../../../starskyapp/readme.md) _Desktop Application_
- [Changelog](../../../history.md) _Release notes and history_

## starsky/starsky/clientapp docs

On top of the Starsky API there is React front-end. This is required to run the web application

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.<br>
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br>
You will also see any lint errors in the console.

Make sure you run the Starsky API on http://localhost:5000 or us a localtunnel proxy (which you can find in `./starsky-tools/localtunnel`)

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

## Project structure

The clientapp uses the following folder structure

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

### Upgrade `Create React App` to a newer version

The default Create React App package is used to keep future upgrades less painfull.
In the repository of [Create React App releases](https://github.com/facebook/create-react-app/releases) you can find if we are using the latest version.

#### To keep all CRA dependencies in place

```
npx create-react-app my-app --template typescript
```

- copy the `package.json` and `package-lock.json` from the `my-app` folder to the `clientapp` folder

```
npm ci
```

The following packages are added:

```
npm install --save abortcontroller-polyfill
npm install --save @reach/router
npm install --save intersection-observer
npm install --save @types/reach__router
npm install --save leaflet
npm install --save @types/leaflet
npm install --save enzyme
npm install --save @types/enzyme
npm install --save-dev @storybook/react
npm install --save @wojtekmaj/enzyme-adapter-react-17
npm install --save eslint-config-prettier
npm install --save eslint-plugin-prettier
npm install --save prettier
```

Remove this package

```
npm uninstall --save web-vitals
```

#### Update the name of the project

```json
 "name": "clientapp",
```

#### Proxy tag for backend services & homepage

Used when running `npm start`

```json
"proxy": "http://localhost:5000",
"homepage": "/starsky/",
```

#### `npm run test:ci` is used by the build-script to run all tests and ESlint

This is added to the `package.json`

```json
"lint": "node node_modules/eslint/bin/eslint.js \"src/**\" --max-warnings 0",
"test:ci": "npm run lint && react-scripts test --watchAll=false --coverage --reporters=default 2>&1",
"storybook": "start-storybook",
"upgrade": "echo 'check readme.md 20210214  v4.0.2 (2021-02-03)'"
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
      "branches": 80,
      "functions": 80,
      "lines": 80,
      "statements": 80
    }
  }
},
```

## ESlint/prettier

You should replace the existing eslintConfig chapter

```json
  "eslintConfig": {
    "extends": [
      "react-app",
      "react-app/jest",
       "plugin:prettier/recommended"
    ],
    "plugins": ["prettier"],
    "rules": {
      "prettier/prettier": [
        "error",
        {
          "endOfLine": "auto"
        }
      ]
    }
  },
  "prettier": {
		"trailingComma": "none",
    "bracketSpacing": true,
    "semi": true,
    "singleQuote": false,
    "tabWidth": 2,
    "useTabs": false
  },
```

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

## Known issues

I keep getting this errors:

Argument for '--jsx' option must be: 'preserve', 'react-native', 'react'
Cannot use JSX unless the '--jsx' flag is provided.

read this:
https://stackoverflow.com/questions/64974648/problem-with-visual-studio-code-using-react-jsx-as-jsx-value-with-create-react
