#!/usr/bin/node

const {spawnSync} = require("child_process");
const fs = require("fs");
const path = require("path");
const {exit} = require("process");

const clientAppFolderPath = path.join(
	__dirname,
	"..",
	"..",
	"starsky",
	"starsky",
	"clientapp"
);

if (!fs.existsSync(clientAppFolderPath)) {
	console.log("FAIL -clientAppFolderPath does not exists");
	exit(1);
}

deleteFolderRecursive = function (path) {
	var files = [];
	if (fs.existsSync(path)) {
		files = fs.readdirSync(path);
		files.forEach(function (file, index) {
			var curPath = path + "/" + file;
			if (fs.lstatSync(curPath).isDirectory()) {
				// recurse
				deleteFolderRecursive(curPath);
			} else {
				// delete file
				fs.unlinkSync(curPath);
			}
		});
		fs.rmdirSync(path);
	}
};

const createReactTempFolder = path.join(__dirname, "vite-tmp-folder");
const myAppName = "my-app";
const createReactMyAppFolder = path.join(
	__dirname,
	"vite-tmp-folder",
	myAppName
);

console.log("npm config set fund false --global");
spawnSync("npm", ["config", "set", "fund", "false", "--global"], {
	cwd: clientAppFolderPath,
	env: process.env,
	encoding: "utf-8",
});

function getNpxCreateCreateApp() {
	console.log("check " + createReactTempFolder);
	if (fs.existsSync(createReactTempFolder)) {
		deleteFolderRecursive(createReactTempFolder);
	}
	fs.mkdirSync(createReactTempFolder);

	if (!fs.existsSync(createReactTempFolder)) {
		console.log("FAIL -directory creating failed");
		exit(1);
	}

	console.log("--createReactTempFolder");
	console.log(createReactTempFolder);

	// npm create -y vite@latest my-app -- --template react-ts
	console.log(
		` npm create -y vite@latest ${myAppName} -- --template react-ts`
	);

	const updateSpawn = spawnSync(
		"npm",
		["create", "-y", "vite@latest", myAppName, , "--", "--template", "react-ts"],
		{
			cwd: createReactTempFolder,
			env: process.env,
			encoding: "utf-8",
		}
	);

	console.log("-result of npm create");
	console.log(updateSpawn.stdout);
	console.log(updateSpawn.stout ? updateSpawn.stout : "");

	// run npm install in my-app folder
	const createReactTempFolderMyApp = path.join(createReactTempFolder, myAppName);
	const npmInstallMyAppSpawn = spawnSync(
		"npm",
		["install"],
		{
			cwd: createReactTempFolderMyApp,
			env: process.env,
			encoding: "utf-8",
		}
	);

	console.log("-result of npm install in my-app folder");
	console.log(npmInstallMyAppSpawn.stdout);
	console.log(npmInstallMyAppSpawn.stout ? npmInstallMyAppSpawn.stout : "");

}

if (process.env.DEBUG !== "true") {
	getNpxCreateCreateApp();
}

if (
	!fs.existsSync(path.join(createReactMyAppFolder, "package.json")) ||
	!fs.existsSync(path.join(createReactMyAppFolder, "package-lock.json"))
) {
	console.log("FAIL --- should include package json files");
	exit(1);
}

if (fs.existsSync(path.join(clientAppFolderPath, "node_modules"))) {
	deleteFolderRecursive(path.join(clientAppFolderPath, "node_modules"));
}

const myAppPackageJson = JSON.parse(
	fs
		.readFileSync(path.join(createReactMyAppFolder, "package.json"))
		.toString()
);
const myAppPackageLockJson = JSON.parse(
	fs
		.readFileSync(path.join(createReactMyAppFolder, "package-lock.json"))
		.toString()
);

// backup first
let toClientAppPackageJson = JSON.parse(
	fs.readFileSync(path.join(clientAppFolderPath, "package.json")).toString()
);
fs.writeFileSync(
	path.join(clientAppFolderPath, "package.json.bak"),
	JSON.stringify(toClientAppPackageJson, null, 2)
);

// overwrite
fs.writeFileSync(
	path.join(clientAppFolderPath, "package.json"),
	JSON.stringify(myAppPackageJson, null, 2)
);
fs.writeFileSync(
	path.join(clientAppFolderPath, "package-lock.json"),
	JSON.stringify(myAppPackageLockJson, null, 2)
);

// npm ci
function npmCi() {
	console.log(
		"run > npm ci --no-audit --legacy-peer-deps | in: " +
		clientAppFolderPath
	);
	const npmCiOne = spawnSync(
		"npm",
		["ci", "--no-audit", "--legacy-peer-deps"],
		{
			cwd: clientAppFolderPath,
			env: process.env,
			encoding: "utf-8",
		}
	);

	console.log("-result of npmCiOne");
	console.log(npmCiOne.stdout);
	console.log(npmCiOne.stout ? updateSpawn.stout : "");
}

npmCi();

function npmUnInstall(packageName) {
	console.log(`run > npm uninstall ${packageName} --save --legacy-peer-deps`);
	const uninstall = spawnSync(
		"npm",
		[
			"uninstall",
			packageName,
			"--no-audit",
			"--save",
			"--legacy-peer-deps",
			"--no-fund",
		],
		{
			cwd: clientAppFolderPath,
			env: process.env,
			encoding: "utf-8",
		}
	);

	console.log("-result of package");
	console.log(uninstall.stdout);
	console.log(uninstall.stout ? updateSpawn.stout : "");
}

// npmUnInstall can be done here

// update packages in clientapp package json
console.log("next: overwrite package json file");
toClientAppPackageJson.dependencies = {
	...toClientAppPackageJson.dependencies,
	...myAppPackageJson.dependencies,
};
fs.writeFileSync(
	path.join(clientAppFolderPath, "package.json"),
	JSON.stringify(toClientAppPackageJson, null, 2)
);
fs.rmSync(path.join(clientAppFolderPath, "package.json.bak"));

function npmInstall(packageName, force, dev) {
	let forceText = "";
	if (force) {
		forceText = "--force";
	}
	let saveText = "--save";
	if (dev) {
		saveText = "--save-dev";
	}
	console.log(
		"npm" +
		" " +
		"install --no-audit" +
		" " +
		packageName +
		" " +
		saveText +
		" " +
		forceText
	);
	const npmInstallSpawn = spawnSync(
		"npm",
		["install", "--no-audit", packageName, saveText, forceText],
		{
			cwd: clientAppFolderPath,
			env: process.env,
			encoding: "utf-8",
		}
	);

	console.log("-result of " + packageName);
	console.log(npmInstallSpawn.stdout);
	console.log(npmInstallSpawn.stout ? updateSpawn.stout : "");
	if (npmInstallSpawn.stout) {
		exit(1);
	}
}

// uninstall not needed at the moment

console.log("next install");
// npmInstall: name, force, dev

npmInstall('leaflet', false, false);
npmInstall('core-js', false, false);
npmInstall('react-router-dom', false, false);
npmInstall('prettier', false, true);
npmInstall('ts-jest', false, true);
npmInstall('ts-node', false, true);
npmInstall('jest', false, true);
npmInstall('jest-environment-jsdom', false, true);
npmInstall('identity-obj-proxy', false, true);
npmInstall('isomorphic-fetch', false, true);
npmInstall('eslint-plugin-react', false, true);
npmInstall('eslint-config-prettier', false, true);
npmInstall('eslint-plugin-prettier', false, true);
npmInstall('eslint-plugin-jest-react', false, true);
npmInstall('eslint-plugin-storybook', false, true);
npmInstall('eslint-plugin-testing-library', false, true);
npmInstall('@types/leaflet', false, true);
npmInstall('@types/node', false, true);
npmInstall('@types/jest', false, true);
npmInstall('storybook', false, true);
npmInstall('@storybook/addon-essentials', false, true);
npmInstall('@storybook/addon-interactions', false, true);
npmInstall('@storybook/addon-links', false, true);
// @storybook/blocks is skipped
npmInstall('@storybook/builder-vite', false, true);
npmInstall('@storybook/react', false, true);
npmInstall('@storybook/react-vite', false, true);
// @storybook/testing-library is skipped
npmInstall('@testing-library/jest-dom', false, true);
npmInstall('@testing-library/react', false, true);
// @testing-library/user-event is skipped

console.log("npm install result:");
const npmInstallSpawnResult = spawnSync(
	"npm",
	[
		"install"
	],
	{
		cwd: clientAppFolderPath,
		env: process.env,
		encoding: "utf-8",
	}
);
console.log(npmInstallSpawnResult.stdout);


// clean afterwards
if (process.env.DEBUG !== "true") {
	console.log("when exists rm " + createReactTempFolder);
	if (fs.existsSync(createReactTempFolder)) {
		deleteFolderRecursive(createReactTempFolder);
	}
}

// run linter (and fix issues)
console.log("next: run linter");
const lintSpawn = spawnSync("npm", ["run", "lint:fix"], {
	cwd: clientAppFolderPath,
	env: process.env,
	encoding: "utf-8",
});
console.log(lintSpawn.stdout);

console.log("next: build project");
const buildSpawn = spawnSync("npm", ["run", "build"], {
	cwd: clientAppFolderPath,
	env: process.env,
	encoding: "utf-8",
});
console.log(buildSpawn.stdout);


console.log("next: test project");
const testSpawn = spawnSync("npm", ["run", "test:ci"], {
	cwd: clientAppFolderPath,
	env: process.env,
	encoding: "utf-8",
});
console.log(testSpawn.stdout);

console.log("done");
