# Starsky ClientApp

## List of [Starsky](../../../readme.md) Projects

- [By App documentation](../../../starsky/readme.md) _database photo index & import index project_
  - [starsky](../../../starsky/starsky/readme.md) _web api application / interface_
    - **[clientapp](../../../starsky/starsky/clientapp/readme.md) react front-end application**
  - [starskyImporterCli](../../../starsky/starskyimportercli/readme.md) _import command line interface_
  - [starskyGeoCli](../../../starsky/starskygeocli/readme.md) _gpx sync and reverse 'geo tagging'_
  - [starskyWebHtmlCli](../../../starsky/starskywebhtmlcli/readme.md) _publish web images to a content package_
  - [starskyWebFtpCli](../../../starsky/starskywebftpcli/readme.md) _copy a content package to a ftp service_
  - [starskyAdminCli](../../../starsky/starskyadmincli/readme.md) _manage user accounts_
  - [starskySynchronizeCli](../../../starsky/starskysynchronizecli/readme.md) _check if disk changes are updated in the database_
  - [starskyThumbnailCli](../../../starsky/starskythumbnailcli/readme.md) _speed web performance by generating smaller images_
  - [Starsky Business Logic](../../../starsky/starskybusinesslogic/readme.md) _business logic libraries (.NET)_
  - [starskyTest](../../../starsky/starskytest/readme.md) _mstest unit tests (for .NET)_
- [starsky-tools](../../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
- [Starsky Desktop](../../../starskydesktop/readme.md) _Desktop Application_
  - [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and Mac OS version_
- [Changelog](../../../history.md) _Release notes and history_

## starsky/starsky/clientapp docs

On top of the Starsky API there is React front-end. This is required to run the web application

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.
Open http://localhost:3000 to view it in the browser.

The page will reload if you make edits.
You will also see any lint errors in the console.

Make sure you run the Starsky API on http://localhost:4000 or us a local tunnel proxy (which you can find in `./starsky-tools/localtunnel`)

### `npm test`

Launches the test runner in the interactive watch mode.
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.
Although we **don't** use Create React App, we do use the same test runner.
See https://jestjs.io/ and https://testing-library.com/docs/react-testing-library/intro/ for more information.

### `npm test:ci`

Run all unit tests and check if there are any errors

### `npm run build`

Builds the app for production to the `build` folder.<br/>
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br/>
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

The client app uses the following folder structure

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

### Upgrade `Vite` to a newer version

#### Auto upgrade

In this project there is a script that auto upgrades the `Vite` to the latest version

```
npm run update:install
```

Run the tests afterwards to check if everything is working

```
npm ci && npm test:ci
```

#### Manual Upgrade

The default Vite / Typescript package is used to keep future upgrades less painfull.
In the repository of [Vite releases](https://github.com/vitejs/vite/releases) you can find if we are using the latest version.

#### To keep all Vite dependencies in place

```
npm create -y vite@latest clientapp -- --template react-ts

cd clientapp
npm install

```

**Normal dependencies, that are included**
react, react-dom

**Dev dependencies, that are included**
@types/react, @types/react-dom, @typescript-eslint/eslint-plugin, @typescript-eslint/parser,
@vitejs/plugin-react, eslint, eslint-plugin-react-hooks, eslint-plugin-react-refresh, typescript, vite

npm install --save leaflet
npm install --save core-js
npm install --save react-router-dom

npm install --save-dev prettier

npm install --save-dev ts-jest
npm install --save-dev ts-node
npm install --save-dev jest
npm install --save-dev jest-environment-jsdom
npm install --save-dev identity-obj-proxy
npm install --save-dev isomorphic-fetch

npm install --save-dev eslint-plugin-react
npm install --save-dev eslint-config-prettier
npm install --save-dev eslint-plugin-prettier
npm install --save-dev eslint-plugin-jest-react
npm install --save-dev eslint-plugin-storybook
npm install --save-dev eslint-plugin-testing-library

npm install --save-dev @types/leaflet
npm install --save-dev @types/node
npm install --save-dev @types/jest

npm install --save-dev storybook
npm install --save-dev @storybook/addon-essentials
npm install --save-dev @storybook/addon-interactions
npm install --save-dev @storybook/addon-links
npm install --save-dev @storybook/blocks
npm install --save-dev @storybook/builder-vite
npm install --save-dev @storybook/react
npm install --save-dev @storybook/react-vite
npm install --save-dev @storybook/testing-library

npm install --save-dev @testing-library/jest-dom
npm install --save-dev @testing-library/react

#### Update the name of the project

```
 "name": "clientapp",
```

#### Proxy tag for backend services & homepage

Used when running `npm start`

```
"proxy": "http://localhost:4000",
"homepage": "/starsky/",
```

#### `npm run test:ci` is used by the build-script to run all tests and ESlint

This is added to the `package.json`

```
"dev": "vite",
"start": "vite --port 3000",
"build": "tsc -p tsconfig.prod.json && vite build",
"lint": "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 800",
"lint:fix": "eslint --fix . --ext ts,tsx --report-unused-disable-directives --max-warnings 800",
"format": "prettier --write './**/*.{js,jsx,ts,tsx,css,md,json}'",
"test": "jest --watch",
"test:ci": "jest --ci --coverage --silent",
"test-ci": "npm run test:ci",
"test:ci:debug": "jest --ci --coverage",
"preview": "vite preview",
"tsc": "tsc",
"storybook": "storybook dev -p 6006",
"build-storybook": "storybook build"
```

### collectCoverageFrom and coverageReporters

With jest `collectCoverageFrom` and `coverageReporters` are used to get the right output

```
  "jest": {
    "testEnvironment": "jest-environment-jsdom",
    "transform": {
      "^.+\\.tsx?$": "ts-jest"
    },
    "moduleNameMapper": {
      ".+\\.(css|styl|less|sass|scss|png|jpg|ttf|woff|woff2|svg|gif)$": "identity-obj-proxy"
    },
    "setupFilesAfterEnv": [
      "<rootDir>/jest.setup.ts"
    ],
    "collectCoverageFrom": [
      "**/*.{ts,tsx}",
      "!coverage/**",
      "!**/*.stories.{ts,tsx}",
      "!node_modules/**",
      "!.storybook/**",
      "!vite.config.ts",
      "!jest.setup.ts",
      "!src/index.*ts*",
      "!src/main.*ts*",
      "!src/service-worker.ts",
      "!src/react-app-env.d.ts",
      "!src/setupTests.js",
      "!public/**",
      "!build/**"
    ],
    "coverageReporters": [
      "text",
      [
        "lcov",
        {
          "projectRoot": "../../"
        }
      ],
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

```
  "eslintConfig": {
    "root": true,
    "env": {
      "browser": true,
      "es2020": true
    },
    "extends": [
      "eslint:recommended",
      "plugin:@typescript-eslint/recommended",
      "plugin:react-hooks/recommended",
      "plugin:react/recommended",
      "plugin:prettier/recommended",
      "plugin:jest-react/recommended",
      "plugin:storybook/recommended"
    ],
    "ignorePatterns": [
      "dist",
      ".eslintrc.cjs",
      "jest.setup.ts",
      "jest.config.ts"
    ],
    "parser": "@typescript-eslint/parser",
    "plugins": [
      "react-refresh",
      "testing-library",
      "prettier",
      "jest-react",
      "react-hooks"
    ],
    "rules": {
      "prettier/prettier": [
        "error",
        {
          "endOfLine": "auto"
        }
      ],
      "react-refresh/only-export-components": [
        "warn",
        {
          "allowConstantExport": true
        }
      ],
      "@typescript-eslint/no-explicit-any": "warn",
      "@typescript-eslint/ban-types": "warn",
      "no-case-declarations": "warn",
      "react/display-name": "warn",
      "react/prop-types": "warn",
      "@typescript-eslint/no-loss-of-precision": "warn",
      "react/react-in-jsx-scope": "off"
    },
    "parserOptions": {
      "ecmaVersion": "latest",
      "sourceType": "module",
      "project": [
        "./tsconfig.json",
        "./tsconfig.node.json"
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
  }
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

---

# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react/README.md) uses [Babel](https://babeljs.io/) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type aware lint rules:

- Configure the top-level `parserOptions` property like this:

```js
   parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module',
    project: ['./tsconfig.json', './tsconfig.node.json'],
    tsconfigRootDir: __dirname,
   },
```

- Replace `plugin:@typescript-eslint/recommended` to `plugin:@typescript-eslint/recommended-type-checked` or `plugin:@typescript-eslint/strict-type-checked`
- Optionally add `plugin:@typescript-eslint/stylistic-type-checked`
- Install [eslint-plugin-react](https://github.com/jsx-eslint/eslint-plugin-react) and add `plugin:react/recommended` & `plugin:react/jsx-runtime` to the `extends` list
