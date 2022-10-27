#!/usr/bin/node

const { spawnSync } = require('child_process');
const fs = require("fs"); 
const { join, dirname } = require("path");
const { exit } = require('process');
const { prefixPath } = require("./lib/prefix-path.const.js");

let searchPath = join(__dirname, prefixPath);

const argv = process.argv.slice(2);

if (argv) {
	// regex: ^(\d+\.)?(\d+\.)?(\*|x|\d+)$
	for (const argItem of argv) {
		if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`)
		}
	}
}

console.log(searchPath);

const gitVersion = spawnSync('git', ['--version'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (gitVersion.stdout.indexOf("git version") === -1) {
    console.error("git not found");
    exit(1);
}

const gitStatusPorcelain = spawnSync('git', ['status', '--porcelain'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (!gitStatusPorcelain.stdout) {
    console.log("no changes");
    exit(0);
}

console.log(gitStatusPorcelain.stdout);
console.error("git status not clean");



// const clientAppFolderPath = path.join(__dirname, "..", "..", "starsky", "starsky", "clientapp" );

// if (!fs.existsSync(clientAppFolderPath)) {
//     console.log('FAIL -clientAppFolderPath does not exists');
//     exit(1)
// }

// deleteFolderRecursive = function(path) {
//     var files = [];
//     if( fs.existsSync(path) ) {
//         files = fs.readdirSync(path);
//         files.forEach(function(file,index){
//             var curPath = path + "/" + file;
//             if(fs.lstatSync(curPath).isDirectory()) { // recurse
//                 deleteFolderRecursive(curPath);
//             } else { // delete file
//                 fs.unlinkSync(curPath);
//             }
//         });
//         fs.rmdirSync(path);
//     }
// };


// const createReactTempFolder = path.join(__dirname,'create-react-tmp-folder');
// const myAppName = "my-app";
// const createReactMyAppFolder = path.join(__dirname,'create-react-tmp-folder',myAppName);



// console.log('npm config set fund false --global');
// spawnSync('npm', ['config', 'set', 'fund', 'false', '--global'], {
//   cwd: clientAppFolderPath,
//   env: process.env,
//   encoding: 'utf-8'
// });


// function getNpxCreateCreateApp() {
//     console.log('check ' + createReactTempFolder);
//     if (fs.existsSync(createReactTempFolder)) {
//         deleteFolderRecursive(createReactTempFolder)
//     }
//     fs.mkdirSync(createReactTempFolder);
    
//     if (!fs.existsSync(createReactTempFolder)) {
//         console.log('FAIL -directory creating failed');
//         exit(1)
//     }
    
//     console.log('--createReactTempFolder');
//     console.log(createReactTempFolder);
    
//     console.log(`running --> npx create-react-app ${myAppName} --template typescript`);
//     const updateSpawn = spawnSync('npx', ['create-react-app', myAppName, '--template', 'typescript'], {
//         cwd: createReactTempFolder,
//         env: process.env,
//         encoding: 'utf-8'
//     });
        
//     console.log('-result of npx');
//     console.log(updateSpawn.stdout);
//     console.log(updateSpawn.stout ? updateSpawn.stout : "");
// }

// if (process.env.DEBUG !== "true") {
//   getNpxCreateCreateApp();
// }


// if (!fs.existsSync(path.join(createReactMyAppFolder, 'package.json')) || !fs.existsSync(path.join(createReactMyAppFolder, 'package-lock.json'))) {
//     console.log('FAIL --- should include package json files');
//     exit(1);
// }

// if (fs.existsSync(path.join(clientAppFolderPath, 'node_modules'))) {
//     deleteFolderRecursive(path.join(clientAppFolderPath, 'node_modules'))
// }

// const myAppPackageJson = JSON.parse(fs.readFileSync(path.join(createReactMyAppFolder, 'package.json')).toString());
// const myAppPackageLockJson = JSON.parse(fs.readFileSync(path.join(createReactMyAppFolder, 'package-lock.json')).toString());

// // backup first
// let toClientAppPackageJson = JSON.parse(fs.readFileSync(path.join(clientAppFolderPath, 'package.json')).toString());
// fs.writeFileSync(path.join(clientAppFolderPath, 'package.json.bak'), JSON.stringify(toClientAppPackageJson, null, 2));

// // overwrite 
// fs.writeFileSync(path.join(clientAppFolderPath, 'package.json'), JSON.stringify(myAppPackageJson, null, 2));
// fs.writeFileSync(path.join(clientAppFolderPath, 'package-lock.json'), JSON.stringify(myAppPackageLockJson, null, 2));

// // npm ci
// function npmCi() {
//   console.log('run > npm ci --no-audit --legacy-peer-deps | in: ' + clientAppFolderPath);
//   const npmCiOne = spawnSync('npm', ['ci', '--no-audit', '--legacy-peer-deps'], {
//       cwd: clientAppFolderPath,
//       env: process.env,
//       encoding: 'utf-8'
//   });

//   console.log('-result of npmCiOne');
//   console.log(npmCiOne.stdout);
//   console.log(npmCiOne.stout ? updateSpawn.stout : "");
// }
// npmCi();

// function npmUnInstall(packageName) {

//   console.log(`run > npm uninstall ${packageName} --save --legacy-peer-deps`);
//   const uninstall = spawnSync('npm', ['uninstall', packageName, '--no-audit', '--save', '--legacy-peer-deps'], {
//       cwd: clientAppFolderPath,
//       env: process.env,
//       encoding: 'utf-8'
//   });

//   console.log('-result of package');
//   console.log(uninstall.stdout);
//   console.log(uninstall.stout ? updateSpawn.stout : "");
// }

// // web-vitals is not needed
// npmUnInstall('web-vitals');
// // install later again (newer version)
// npmUnInstall('@testing-library/user-event');

// // update packages in clientapp package json
// console.log('next: overwrite package json file');
// toClientAppPackageJson.dependencies = {...toClientAppPackageJson.dependencies, ...myAppPackageJson.dependencies};
// fs.writeFileSync(path.join(clientAppFolderPath, 'package.json'), JSON.stringify(toClientAppPackageJson, null, 2));
// fs.rmSync(path.join(clientAppFolderPath, 'package.json.bak'))

// function npmInstall(packageName, force, dev) {
//   let forceText = ""
//   if (force) {
//     forceText = "--force";
//   }
//   let saveText = "--save"
//   if (dev) {
//     saveText = "--save-dev";
//   }
//   console.log('npm'   + " " + 'install --no-audit'  + " " + packageName  + " " + saveText + " " +  forceText);
//   const npmInstallSpawn = spawnSync('npm', ['install', '--no-audit', packageName, saveText, forceText], {
//       cwd: clientAppFolderPath,
//       env: process.env,
//       encoding: 'utf-8'
//   });

//   console.log('-result of '+packageName);
//   console.log(npmInstallSpawn.stdout);
//   console.log(npmInstallSpawn.stout ? updateSpawn.stout : "");
//   if (npmInstallSpawn.stout) {
//     exit(1)
//   }
// }

// npmUnInstall('web-vitals')
// npmInstall('abortcontroller-polyfill', false, false);
// npmInstall('@reach/router', true, false);
// npmInstall('intersection-observer', false, false);
// npmInstall('@types/reach__router', false, false);
// npmInstall('abortcontroller-polyfill', false, false);
// npmInstall('leaflet', false, false);
// npmInstall('@types/storybook__react', false, false);
// npmInstall('@storybook/react', true, true);
// npmInstall('eslint-config-prettier', false), false;
// npmInstall('eslint-plugin-prettier', false, false);
// npmInstall('prettier', false, false);
// npmInstall('eslint-plugin-prettier', false, false);
// npmUnInstall('@types/node')
// npmInstall('@types/node', false, false);
// npmInstall('concurrently', false, true);
// npmInstall('@testing-library/user-event',false, false);

// npmCi();

// // clean afterwards
// if (process.env.DEBUG !== "true") {
//   console.log('when exists rm ' + createReactTempFolder);
//   if (fs.existsSync(createReactTempFolder)) {
//       deleteFolderRecursive(createReactTempFolder)
//   }
// }


// // run linter
// const lintSpawn = spawnSync('npm', ['run', 'lint:fix'], {
//   cwd: clientAppFolderPath,
//   env: process.env,
//   encoding: 'utf-8'
// });
// console.log(lintSpawn.output);

console.log('done');