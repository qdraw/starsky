#!/usr/bin/node

const {spawnSync} = require('child_process');
const fs = require("fs");
const path = require('path');
const {exit} = require('process');

const documentationAppFolderPath = path.join(__dirname, "..", "..", "documentation");

if (!fs.existsSync(documentationAppFolderPath)) {
	console.log('FAIL -documentationAppFolderPath does not exists');
	exit(1)
}

const deleteFolderRecursive = function (path) {
	// macOS (Finder/IDE) can recreate .DS_Store while deleting, which causes ENOTEMPTY; so retry
	try {
		fs.rmSync(path, {recursive: true, force: true, maxRetries: 10, retryDelay: 200});
	} catch (error) {
		console.log('rmSync failed, trying rm -rf: ' + error.code);
		spawnSync('rm', ['-rf', path]);
		if (fs.existsSync(path)) {
			// only temp/clean-install folders are deleted here, so do not abort the update
			console.log('WARN: could not fully remove ' + path);
		}
	}
};


const createDocusaurusTempFolder = path.join(__dirname, 'create-docusaurus-tmp-folder');
const myAppName = "my-app";
const createDocusaurusMyAppFolder = path.join(__dirname, 'create-docusaurus-tmp-folder', myAppName);


console.log('npm config set fund false --global');
spawnSync('npm', ['config', 'set', 'fund', 'false', '--global'], {
	cwd: documentationAppFolderPath,
	env: process.env,
	encoding: 'utf-8'
});


function getNpxCreateCreateApp() {
	console.log('check ' + createDocusaurusTempFolder);
	if (fs.existsSync(createDocusaurusTempFolder)) {
		deleteFolderRecursive(createDocusaurusTempFolder)
	}
	fs.mkdirSync(createDocusaurusTempFolder);

	if (!fs.existsSync(createDocusaurusTempFolder)) {
		console.log('FAIL -directory creating failed');
		exit(1)
	}

	console.log('--createDocusaurusTempFolder');
	console.log(createDocusaurusTempFolder);

	console.log(`running --> npx create-docusaurus@latest ${myAppName} classic --typescript`);
	const updateSpawn = spawnSync('npx', ['create-docusaurus@latest', myAppName, 'classic', '--typescript'], {
		cwd: createDocusaurusTempFolder,
		env: process.env,
		encoding: 'utf-8'
	});

	console.log('-result of npx');
	console.log(updateSpawn.stdout);
	console.log(updateSpawn.stout ? updateSpawn.stout : "");
}

if (process.env.DEBUG !== "true") {
	getNpxCreateCreateApp();
}


if (!fs.existsSync(path.join(createDocusaurusMyAppFolder, 'package.json')) || !fs.existsSync(path.join(createDocusaurusMyAppFolder, 'package-lock.json'))) {
	console.log('FAIL --- should include package json files');
	exit(1);
}

if (fs.existsSync(path.join(documentationAppFolderPath, 'node_modules'))) {
	deleteFolderRecursive(path.join(documentationAppFolderPath, 'node_modules'))
}

const myAppPackageJson = JSON.parse(fs.readFileSync(path.join(createDocusaurusMyAppFolder, 'package.json')).toString());
const myAppPackageLockJson = JSON.parse(fs.readFileSync(path.join(createDocusaurusMyAppFolder, 'package-lock.json')).toString());

// backup first
let toClientAppPackageJson = JSON.parse(fs.readFileSync(path.join(documentationAppFolderPath, 'package.json')).toString());
fs.writeFileSync(path.join(documentationAppFolderPath, 'package.json.bak'), JSON.stringify(toClientAppPackageJson, null, 2));

// overwrite 
fs.writeFileSync(path.join(documentationAppFolderPath, 'package.json'), JSON.stringify(myAppPackageJson, null, 2));
fs.writeFileSync(path.join(documentationAppFolderPath, 'package-lock.json'), JSON.stringify(myAppPackageLockJson, null, 2));

// npm install
function npmBasicInstall() {
	console.log('run > npm install --no-audit --legacy-peer-deps | in: ' + documentationAppFolderPath);
	const npmCiOne = spawnSync('npm', ['install', '--no-audit', '--legacy-peer-deps'], {
		cwd: documentationAppFolderPath,
		env: process.env,
		encoding: 'utf-8'
	});

	console.log('-result of npmCiOne');
	console.log(npmCiOne.stdout);
	console.log(npmCiOne.stout ? updateSpawn.stout : "");
}

npmBasicInstall();

function npmUnInstall(packageName) {

	console.log(`run > npm uninstall ${packageName} --save --legacy-peer-deps`);
	const uninstall = spawnSync('npm', ['uninstall', packageName, '--no-audit', '--save', '--legacy-peer-deps', '--no-fund'], {
		cwd: documentationAppFolderPath,
		env: process.env,
		encoding: 'utf-8'
	});

	console.log('-result of package');
	console.log(uninstall.stdout);
	console.log(uninstall.stout ? updateSpawn.stout : "");
}

// uninstall not needed packages here that come with create-docusaurus
// install to have updates here from packages that come with create-docusaurus


// update packages in clientapp package json
console.log('next: overwrite package json file');
toClientAppPackageJson.dependencies = {...toClientAppPackageJson.dependencies, ...myAppPackageJson.dependencies};
fs.writeFileSync(path.join(documentationAppFolderPath, 'package.json'), JSON.stringify(toClientAppPackageJson, null, 2));
fs.rmSync(path.join(documentationAppFolderPath, 'package.json.bak'))

function npmInstall(packageName, force, dev) {
	let forceText = ""
	if (force) {
		forceText = "--force";
	}
	let saveText = "--save"
	if (dev) {
		saveText = "--save-dev";
	}
	console.log('npm' + " " + 'install --no-audit' + " " + packageName + " " + saveText + " " + forceText);
	const npmInstallSpawn = spawnSync('npm', ['install', '--no-audit', packageName, saveText, forceText].filter(Boolean), {
		cwd: documentationAppFolderPath,
		env: process.env,
		encoding: 'utf-8'
	});

	console.log('-result of ' + packageName);
	console.log(npmInstallSpawn.stdout);
	console.log(npmInstallSpawn.stout ? updateSpawn.stout : "");
	if (npmInstallSpawn.stout) {
		exit(1)
	}
}

// install other packages here
// for example: docusaurus-plugin-openapi (which is not used at the moment)

// not part of the create-docusaurus template, so it is not updated by the merge above
npmInstall("@cmfcmf/docusaurus-search-local@latest", false, false);
npmInstall("@docusaurus/types@latest", false, true);

npmBasicInstall();

// clean afterwards
if (process.env.DEBUG !== "true") {
	console.log('when exists rm ' + createDocusaurusTempFolder);
	if (fs.existsSync(createDocusaurusTempFolder)) {
		deleteFolderRecursive(createDocusaurusTempFolder)
	}
}


// // run linter
// const lintSpawn = spawnSync('npm', ['run', 'lint:fix'], {
//   cwd: documentationAppFolderPath,
//   env: process.env,
//   encoding: 'utf-8'
// });
// console.log(lintSpawn.output);

console.log('done');
