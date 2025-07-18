#!/usr/bin/node

/**
 * Update the project versions to have the same version
 */

// other script: use release-version-check.js to check if the version is correct based on the branch name in the CI

const {join} = require("path");
const {readFile, writeFile} = require("fs").promises;
const {getFiles} = require("./lib/get-files-directory");
const {prefixPath} = require("./lib/prefix-path.const.js");

let newVersion = "0.7.1";

// allow version as single argument
const argv = process.argv.slice(2);
if (argv && argv.length === 1) {
	newVersion = argv[0];
}

function checkNewVersion() {
	const versionRegexChecker = new RegExp(
		"^([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?$",
		"g"
	);
	const versionRegexMatch = newVersion.match(versionRegexChecker);
	if (versionRegexMatch == null) {
		console.log(
			`✖ - Version  ${newVersion} is not supported - please updated it and run it again.`
		);
		process.exit(1);
	}
}

checkNewVersion();

console.log(
	`\nUpgrade version in csproj-files and package.json to ${newVersion}\n`
);

getFiles(join(__dirname, prefixPath, "starsky"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "starskydesktop"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "starsky-tools"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "documentation"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

async function updateVersions(filePathList) {
	checkNewVersion();
	await filePathList.forEach(async (filePath) => {
		if (
			filePath.match(
				new RegExp(
					"[a-z]((.feature|.foundation)|core)?(.[a-z]+)?.csproj$",
					"i"
				)
			)
		) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			// unescaped: (<Version>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(<\/Version>)
			const versionXMLRegex = new RegExp(
				"(<Version>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(</Version>)",
				"g"
			);
			const fileXmlMatch = fileContent.match(versionXMLRegex);
			if (fileXmlMatch == null) {
				console.log("✖ " + filePath + " - Version tag is not included");
			} else if (fileXmlMatch != null) {
				fileContent = fileContent.replace(
					versionXMLRegex,
					`<Version>${newVersion}</Version>`
				);
				await writeFile(filePath, fileContent);
				console.log(
					`✓ ${filePath} - Version is updated to ${newVersion}`
				);
			}
		} else if (filePath.match(new RegExp("package.json?$", "i"))) {
			let buffer = await readFile(filePath);
			let fileJsonContent = buffer.toString("utf8");
			const versionJsonRegex = new RegExp(
				'"version": ?"([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(s?)"(s?)',
				"g"
			);
			const fileJsonMatch = fileJsonContent.match(versionJsonRegex);
			if (fileJsonMatch == null) {
				console.log(
					"✖ " + filePath + "  - Version tag is not included "
				);
			} else if (fileJsonMatch != null) {
				fileJsonContent = fileJsonContent.replace(
					versionJsonRegex,
					`"version": "${newVersion}"`
				);
				await writeFile(filePath, fileJsonContent);
				console.log(
					`✓ ${filePath} - Version is updated to ${newVersion}`
				);
			}
		}
	});
}
