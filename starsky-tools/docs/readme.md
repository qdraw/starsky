[< starsky/starsky-tools docs](../readme.md)

# Starsky Tools Docs
The goal is to generate html files from the readme.md file in this repository.

## Install dependencies
```
npm ci
```

## To generate html files from the markdown files that are included in the repository
```
npm run start
```

## Results
Please check `readme.html`

## Copy
Copy is used to copy to the root of the repository and optional to include in runtime folders

### To use the default option
To copy to the root of the repository
```
npm run copy
```

And with a arg it copies to that parent folder

```
node copy.js "/data/git/starsky/starsky/linux-arm"
```
which does: copy /data/git/starsky/starsky-tools/docs --> /data/git/starsky/starsky/linux-arm/docs
