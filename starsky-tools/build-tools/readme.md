[< starsky/starsky-tools docs](../readme.md)

# Build Tools Docs

- Scripts that are helping building the application
- depends on nodejs or powershell, 
- **not needed** to run npm install first
- does not need to install dependencies

### app-version-update.js 

Checks the App Version Update. Update the project versions to have the same version. The version is specified in the file

```
npm run app-version-update
```

### Clientapp create-react-app-update

Regenerate npm package.json file for front-end with the latest version of create react app

```
npm run clientapp-create-react-app-update
```

### documentation-create-docusaurus-update

Regenerate npm package.json file for docusaurus

```
npm run documentation-create-docusaurus-update
```

### audit-dotnet.ps1
Powershell script to check if there are vulnerablies in the .NET solution
Requires .NET 5.0.200 or newer installed
And as argument the targetFolder

### audit-npm.ps1
For checking if there are vulnerablies and update in node.js/npm

### nuget-package-list.js
Creates an list to able to cache the Nuget packages fast

```
npm run nuget-package-list
```

### project-guid.js 
Check for duplicate Project GUID's

```
npm run project-guid
```

### release-version-check.js

Make sure the version in files are matching the release version in the branch or tag name. 
When this is not the case auto fix it. 

#### Github Tags:
```
export GITHUB_REF=refs/tags/v0.3.0
```

#### Azure Devops release branches
```
export BUILD_SOURCEBRANCH=refs/heads/release/0.4.2
```

Note: The release branches should **not** start with a v

